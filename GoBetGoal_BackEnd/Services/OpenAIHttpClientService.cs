using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using GoBetGoal_BackEnd.Models; // 確保引用了您的模型命名空間
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GoBetGoal_BackEnd.Services
{
    public static class OpenAIHttpClientService
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private const string OpenAI_Api_Url = "https://api.openai.com/v1/chat/completions";

        /// <summary>
        /// 這是我們唯一需要對外公開的 AI 分析方法。
        /// 它接收一個或多個圖片 URL，並回傳 AI 的分析結果字串。
        /// </summary>
        // 【修正 #1】在方法簽名中加上 async 關鍵字
        public static async Task<string> AnalyzeAsync(List<string> imageUrls, string model, string systemPrompt, string userPrompt)
        {
            var apiKey = ConfigurationManager.AppSettings["OpenAI_ApiKey"];
            if (string.IsNullOrEmpty(apiKey) || apiKey.StartsWith("sk-YOUR"))
            {
                throw new ArgumentException("Web.config 中未設定 OpenAI_ApiKey。");
            }

            // 【修正 #2】更清晰的 contentList 建立方式
            // 我們先建立一個空的列表，然後依序加入文字和圖片。
            var contentList = new List<object>
            {
                // 第一個元素：文字指令 (Prompt)
                new { type = "text", text = userPrompt }
            };

            // 迴圈加入所有圖片 URL
            foreach (var url in imageUrls)
            {
                contentList.Add(new { type = "image_url", image_url = new { url = url } });
            }

            var requestBody = new
            {
                model = model,
                messages = new List<object>
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = contentList }
                },
                max_tokens = 200
            };

            try
            {
                string jsonContent = JsonConvert.SerializeObject(requestBody);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, OpenAI_Api_Url))
                {
                    httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                    httpRequestMessage.Content = httpContent;

                    var response = await _httpClient.SendAsync(httpRequestMessage);
                    string jsonResponse = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        return JsonConvert.SerializeObject(new { safety_rating = "unsafe", compliance_rating = "non_compliant", reason = $"API 請求失敗: {response.StatusCode}" });
                    }

                    var openAIResponse = JObject.Parse(jsonResponse);
                    var messageContent = openAIResponse["choices"]?[0]?["message"]?["content"]?.ToString();

                    return messageContent ?? "{\"safety_rating\": \"unsafe\", \"compliance_rating\": \"non_compliant\", \"reason\": \"AI 未提供有效回應。\"}";
                }
            }
            catch (Exception ex)
            {
                return $"{{\"safety_rating\": \"unsafe\", \"compliance_rating\": \"non_compliant\", \"reason\": \"程式執行異常：{ex.Message}\"}}";
            }
        }
    }
}