using GoBetGoal_BackEnd.Models;
using GoBetGoal_BackEnd.Models.DTOs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace GoBetGoal_BackEnd.Services.Verifiers
{
    /// <summary>
    /// 「多圖綜合審核」的專科醫生
    /// </summary>
    public class CollectiveVerifier : IVerifier
    {
        private readonly Stage _stage;

        public CollectiveVerifier(Stage stage)
        {
            _stage = stage;
        }

        // In ~/Services/Verifiers/CollectiveVerifier.cs

        public async Task<ChallengeSubmissionResponse> VerifyAsync(List<string> imageUrls)
        {
            // 1. 準備規則與 Prompt (與之前相同)
            string mainRule = JsonConvert.DeserializeObject<List<string>>(_stage.StageDescription).FirstOrDefault() ?? "";
            var generalRules = JsonConvert.DeserializeObject<List<string>>(_stage.TrialTemplate.TrialRule);
            string systemPrompt = ChallengeHelper.GetMasterSystemPrompt();
            string userPrompt = ChallengeHelper.BuildCollectivePrompt(mainRule, generalRules);

            // 2. 一次性呼叫 AI，傳送所有圖片
            string rawAiResponse = await OpenAIHttpClientService.AnalyzeAsync(imageUrls, "gpt-4o", systemPrompt, userPrompt);

            // 3. 【關鍵修正】使用正確的模型來解析 AI 的「詳細報告」
            var aiResult = ChallengeHelper.ParseAIResponse<AICollectiveResponse>(rawAiResponse);

            // 4. 建立要回傳給前端的 Response 物件
            var response = new ChallengeSubmissionResponse();
            if (aiResult?.OverallAssessment == null)
            {
                response.OverallResult = false;
                response.OverallMessage = "AI 審核時發生錯誤或回應格式不符，請稍後再試。";
                return response;
            }
            // 【修正 #2】直接使用 AIVerificationResult 中的 IsCompliant (bool)
            response.OverallResult = aiResult.OverallAssessment.IsCompliant;
            response.OverallMessage = aiResult.OverallAssessment.Reason;

            // 【修正 #3】將 ImageResults 賦值為一個強型別的 List<CollectiveImageResult>
            response.ImageResults = imageUrls.Select((url, index) =>
            {
                var imageDetail = aiResult.PerImageDetails.FirstOrDefault(d => d.ImageIndex == index);

                return new CollectiveImageResult
                {
                    ImageUrl = url,
                    IsSafe = imageDetail?.IsSafe ?? false // 【修正 #4】補上這行程式碼
                };
            }).ToList();

            return response;
        }
    }
}