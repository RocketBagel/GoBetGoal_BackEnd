using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace GoBetGoal_BackEnd.Controllers
{
    // 這是為了讓前端能傳送圖片 URL 給我們
    public class ImageTestRequest
    {
        public string ImageUrl { get; set; }
    }

    public class AITestController : ApiController
    {
        //    // 我們將建立一個路由為 /api/AITest/AnalyzeImage 的 POST 端點
        private const string OpenAI_Api_Url = "https://api.openai.com/v1/chat/completions";

        [HttpGet]
        [Route("api/test/openai")]
        public async Task<IHttpActionResult> TestOpenAI()
        {
            var apiKey = System.Configuration.ConfigurationManager.AppSettings["OpenAI_ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
                return BadRequest("未設定 OpenAI API Key");

            // 簡單 prompt
            string prompt = "Say hello in Traditional Chinese.";

            // 組裝訊息
            var requestBody = new
            {
                model = "gpt-4o",
                messages = new[]
                {
                new { role = "user", content = prompt }
            },
                max_tokens = 100
            };

            var jsonContent = JsonConvert.SerializeObject(requestBody);
            using (var client = new HttpClient())
            using (var content = new StringContent(jsonContent, Encoding.UTF8, "application/json"))
            using (var request = new HttpRequestMessage(HttpMethod.Post, OpenAI_Api_Url))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                request.Content = content;

                var response = await client.SendAsync(request);
                var responseJson = await response.Content.ReadAsStringAsync();

                // 回傳完整 JSON，包含 usage
                return Ok(responseJson);
            }
        }


    }
}