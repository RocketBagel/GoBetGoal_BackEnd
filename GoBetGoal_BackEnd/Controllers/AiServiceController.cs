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

            // --- 3. 核心審核迴圈：逐張審核圖片 ---
            for (int i = 0; i < request.ImageUrls.Count; i++)
            {
                var imageUrl = request.ImageUrls[i];

                // 決定此圖片對應的規則：若規則列表長度足夠，則一對一對應；否則所有圖片共用第一條規則。
                string specificRule = (request.StageDescriptions.Count > i)
                    ? request.StageDescriptions[i]
                    : request.StageDescriptions.FirstOrDefault() ?? "";

                // --- 4. 準備 Prompt ---
                string systemPrompt = ChallengeHelper.GetMasterSystemPrompt();
                string userPrompt = ChallengeHelper.BuildUserPrompt(
                    specificRule,
                    request.TrialRules,
                    request.ChallengeType
                );

                // --- 5. 呼叫 OpenAI 服務 ---
                string rawAiResponse = await OpenAIHttpClientService.AnalyzeAsync(
                    new List<string> { imageUrl },
                    "gpt-4o",
                    systemPrompt,
                    userPrompt
                );

                // --- 6. 解析 AI 回應並處理結果 ---
                var aiResult = ChallengeHelper.ParseAIResponse<AIVerificationResult>(rawAiResponse);

                var imageResult = new ImageResult
                {
                    ImageUrl = imageUrl,
                    IsSafe = aiResult.IsSafe,
                    IsCompliant = aiResult.IsCompliant,
                    Reason = aiResult.Reason
                };
                response.ImageResults.Add(imageResult);

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