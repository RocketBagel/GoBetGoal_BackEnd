using GoBetGoal_BackEnd.Models;
using GoBetGoal_BackEnd.Models.DTOs;
using Newtonsoft.Json;
using Swashbuckle.Swagger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace GoBetGoal_BackEnd.Services.Verifiers
{
    public class PerImageVerifier : IVerifier
    {
        private readonly Stage _stage; // 儲存從資料庫撈出來的、包含所有規則的關卡資訊

        public PerImageVerifier(Stage stage)
        {
            _stage = stage;
        }

        public async Task<ChallengeSubmissionResponse> VerifyAsync(List<string> imageUrls)
        {
            var response = new ChallengeSubmissionResponse();
            // 明確指定 ImageResults 的列表型別
            var imageResultsList = new List<ImageResult>();

            // 從資料庫來的 JSON 字串規則，在這裡一次性反序列化
            var mealRules = JsonConvert.DeserializeObject<List<string>>(_stage.StageDescription);
            var generalRules = JsonConvert.DeserializeObject<List<string>>(_stage.TrialTemplate.TrialRule);

            // [防呆機制] 如果是多對多規則，則嚴格匹配圖片數量
            if (mealRules.Count > 1 && imageUrls.Count != mealRules.Count)
            {
                response.OverallResult = false;
                response.OverallMessage = $"驗證失敗：此關卡需要 {mealRules.Count} 張照片，但您提供了 {imageUrls.Count} 張。";
                return response;
            }

            bool isStagePassed = true;

            // PerImage 模式：對每一張圖獨立進行審核
            for (int i = 0; i < imageUrls.Count; i++)
            {
                var imageUrl = imageUrls[i];

                //計算出「這張圖」應該對應的「單一餐點/任務規則」
                string ruleForThisImage = (mealRules.Count > i) ? mealRules[i] : mealRules.FirstOrDefault() ?? "";

                string systemPrompt = ChallengeHelper.GetMasterSystemPrompt();

                //將「單一規則」傳給 ChallengeHelper 來建立專屬的 User Prompt
                string userPrompt = ChallengeHelper.BuildUserPrompt(ruleForThisImage, generalRules, _stage.ChallengeType);


                // 將 System Prompt 和 User Prompt 一起傳給 AI
                // 現在我們會收到一個包含 MessageContent 和 Usage 的完整物件
                AiServiceResponse aiResponse = await OpenAIHttpClientService.AnalyzeAsync(
                    new List<string> { imageUrl }, "gpt-4o", systemPrompt, userPrompt);

                // --- 【使用數據】 ---
                // 1. 在後端記錄 Token 用量，用於成本分析和簡報展示
                //    這行程式碼會將 Token 數印在 Visual Studio 的「輸出」視窗中
                System.Diagnostics.Debug.WriteLine($"AI VERIFICATION LOG - StageId: {_stage.Id}, ImageUrl: {imageUrl}, Total Tokens: {aiResponse.Usage.TotalTokens}");

                // 2. 只取出 messageContent 進行後續的業務邏輯處理
                string rawAiResponseString = aiResponse.MessageContent;
                var result = ChallengeHelper.ParseAIResponse<AIVerificationResult>(rawAiResponseString);


                if (!result.IsSafe)
                {
                    isStagePassed = false;
                }
                if (!result.IsCompliant)
                {
                    isStagePassed = false;
                }

                imageResultsList.Add(new ImageResult
                {
                    ImageUrl = imageUrl,
                    IsSafe = result.IsSafe,
                    IsCompliant = result.IsCompliant,
                    Reason = result.Reason ?? "AI 未提供原因。"
                });
            }
            response.OverallResult = isStagePassed;
            response.OverallMessage = isStagePassed ? "所有圖片均通過審核！" : "有圖片未通過審核，挑戰失敗。";
            response.ImageResults = imageResultsList; // 將強型別的列表賦值給 object
            return response;
        }
    }
}