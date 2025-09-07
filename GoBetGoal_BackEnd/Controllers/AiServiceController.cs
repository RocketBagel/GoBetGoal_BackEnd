using GoBetGoal_BackEnd.Models.DTOs;
using GoBetGoal_BackEnd.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace GoBetGoal_BackEnd.Controllers
{
    public class AiServiceController:ApiController
    {
        /// <summary>
        /// 【V2 版】接收一組圖片和相關規則，進行 AI 驗證並回傳結果。
        /// </summary>
        [HttpPost]
        [Route("api/challenge/submit")]
        [AllowAnonymous] // 暫時允許匿名，方便前端整合測試
        public async Task<IHttpActionResult> VerifyStage([FromBody] AiVerificationRequestV2 request)
        {
            // --- 1. 驗證傳入的參數 ---
            if (!ModelState.IsValid || request.ImageUrls == null || !request.ImageUrls.Any())
            {
                return Content(HttpStatusCode.BadRequest, new { ErrorCode = "INVALID_REQUEST", Message = "請求參數不完整或圖片連結為空。" });
            }

            // [防呆機制] 如果是多對多規則，則嚴格匹配圖片與規則的數量
            if (request.StageDescriptions.Count > 1 && request.ImageUrls.Count != request.StageDescriptions.Count)
            {
                return Content(HttpStatusCode.BadRequest, new { ErrorCode = "RULE_IMAGE_MISMATCH", Message = $"驗證失敗：此關卡需要 {request.StageDescriptions.Count} 條規則對應 {request.StageDescriptions.Count} 張照片，但您提供了 {request.ImageUrls.Count} 張。" });
            }

            // --- 2. 準備回應物件和總體結果旗標 ---
            var response = new AiVerificationResponseV2();
            bool isOverallPassed = true;

            // --- 3. 核心審核邏輯：改為平行處理 ---

            // 步驟 1: 建立一個列表來存放所有圖片的非同步分析任務
            var analysisTasks = new List<Task<AiServiceResponse>>();

            // 準備共用的 System Prompt
            string systemPrompt = ChallengeHelper.GetMasterSystemPrompt();

            // 遍歷所有圖片 URL，為每一張圖片建立一個獨立的審核任務
            for (int i = 0; i < request.ImageUrls.Count; i++)
            {
                var imageUrl = request.ImageUrls[i];

                // 決定此圖片對應的規則
                string specificRule = (request.StageDescriptions.Count > i)
                    ? request.StageDescriptions[i]
                    : request.StageDescriptions.FirstOrDefault() ?? "";

                // 準備 User Prompt
                string userPrompt = ChallengeHelper.BuildUserPrompt(
                    specificRule,
                    request.TrialRules,
                    request.ChallengeType
                );

                // 建立分析任務，並將其加入到任務列表中。
                // 注意：這裡沒有 `await`，所以程式不會在此處等待，會立刻開始準備下一個任務
                var task = OpenAIHttpClientService.AnalyzeAsync(
                    new List<string> { imageUrl },
                    "gpt-4o",
                    systemPrompt,
                    userPrompt
                );
                analysisTasks.Add(task);
            }

            // 步驟 2: 使用 Task.WhenAll 一次性執行所有任務，並等待它們全部完成
            // 這是整個流程中最關鍵的改變，所有 API 請求會同時發出
            AiServiceResponse[] allResults = await Task.WhenAll(analysisTasks);

            // --- 4. 處理所有已完成的審核結果 ---
            for (int i = 0; i < allResults.Length; i++)
            {
                var aiServiceResponse = allResults[i];
                var imageUrl = request.ImageUrls[i]; // 透過索引值取回對應的 URL

                // --- 接下來的處理邏輯與您原本的幾乎一樣 ---

                // 在後端日誌中紀錄 Token 使用量
                System.Diagnostics.Debug.WriteLine(
                    $"AI VERIFICATION LOG - ImageUrl: {imageUrl}, " +
                    $"Prompt Tokens: {aiServiceResponse.Usage.PromptTokens}, " +
                    $"Completion Tokens: {aiServiceResponse.Usage.CompletionTokens}, " +
                    $"Total Tokens: {aiServiceResponse.Usage.TotalTokens}"
                );

                // 解析 AI 回應
                string rawAiMessageContent = aiServiceResponse.MessageContent;
                var aiResult = ChallengeHelper.ParseAIResponse<AIVerificationResult>(rawAiMessageContent);

                // 建立圖片結果物件
                var imageResult = new ImageResult
                {
                    ImageUrl = imageUrl,
                    IsSafe = aiResult.IsSafe,
                    IsCompliant = aiResult.IsCompliant,
                    Reason = aiResult.Reason
                };
                response.ImageResults.Add(imageResult);

                // 更新總體結果旗標
                if (!aiResult.IsSafe || !aiResult.IsCompliant)
                {
                    isOverallPassed = false;
                }
            }

            // --- 7. 設定最終的總體回應 ---
            response.OverallResult = isOverallPassed;
            response.OverallMessage = isOverallPassed ? "恭喜！此關卡所有照片均審核通過！" : "很遺憾，此關卡有照片未通過審核。";

            // --- 8. 回傳結果 ---
            return Ok(response);
        }


    }
}