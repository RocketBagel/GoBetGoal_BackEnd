using GoBetGoal_BackEnd.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace GoBetGoal_BackEnd.Services
{
    public class AiVerificationService
    {
        /// <summary>
        /// 接收一組圖片 URL 和對應的規則，平行地進行 AI 驗證。
        /// </summary>
        /// <param name="imageUrls">要驗證的圖片 URL 列表</param>
        /// <param name="rules">每張圖片對應的驗證規則</param>
        /// <returns>一個包含所有圖片審核結果的列表</returns>
        public async Task<List<ImageResult>> VerifyImagesAsync(List<string> imageUrls, List<string> rules, List<string> generalRules, string aiType)
        {
            var analysisTasks = new List<Task<AiServiceResponse>>();
            string systemPrompt = ChallengeHelper.GetMasterSystemPrompt();

            for (int i = 0; i < imageUrls.Count; i++)
            {
                var imageUrl = imageUrls[i];
                // 找到對應的規則
                string specificRule = (rules.Count > i) ? rules[i] : rules.FirstOrDefault() ?? "";

                // 準備 User Prompt (這裡可以簡化，因為 controller 會提供)
                string userPrompt = ChallengeHelper.BuildUserPrompt(specificRule, generalRules, aiType);

                // 建立非同步任務
                var task = OpenAIHttpClientService.AnalyzeAsync(new List<string> { imageUrl }, "gpt-4o", systemPrompt, userPrompt);
                analysisTasks.Add(task);
            }

            // 平行執行所有 AI 分析
            AiServiceResponse[] allResults = await Task.WhenAll(analysisTasks);

            // 將 AI 的原始回覆，轉換成我們自訂的、更乾淨的結果物件
            var verificationResults = new List<ImageResult>();
            for (int i = 0; i < allResults.Length; i++)
            {
                var aiServiceResponse = allResults[i];
                var aiResult = ChallengeHelper.ParseAIResponse<AIVerificationResult>(aiServiceResponse.MessageContent);

                verificationResults.Add(new ImageResult
                {
                    ImageUrl = imageUrls[i],
                    IsCompliant = aiResult.IsCompliant,
                    IsSafe = aiResult.IsSafe,
                    Reason = aiResult.Reason
                });
            }

            return verificationResults;
        }
    }
}