using GoBetGoal_BackEnd.Models;
using GoBetGoal_BackEnd.Models.DTOs;
using Newtonsoft.Json;
using Swashbuckle.Swagger;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;


namespace GoBetGoal_BackEnd.Controllers
{
    public class PaymentSupaController : ApiController
    {
        private readonly string MerchantID = ConfigurationManager.AppSettings["NewebPay_MerchantID"];
        private readonly string HashKey = ConfigurationManager.AppSettings["NewebPay_HashKey"];
        private readonly string HashIV = ConfigurationManager.AppSettings["NewebPay_HashIV"];
        private readonly string PayGateWay = ConfigurationManager.AppSettings["NewebPay_ApiUrl_Test"];

        // <summary>
        // 建立交易並回傳給前端需要的資料
        // </summary>
        [AllowAnonymous]
        [HttpPost]
        [Route("api/payments/create")]
        public async Task<IHttpActionResult> CreatePayment(PaymentRequestDto request)
        {
            //發送給supabase 建立新訂單資料
            // 1. 先建立訂單到 Supabase (沒有 order_no)
            var newOrder = new
            {
                user_id = request.UserId,
                get_bagel = request.BagelCount,
                deposit_money = request.Amount,
                status = "pending"
            };

            using (var client = new HttpClient())
            {
                string supabaseUrl = "https://rbrltczejudsoxphrxnq.supabase.co";
                string tableEndpoint = $"{supabaseUrl}/rest/v1/deposit";

                client.DefaultRequestHeaders.Add("apikey", ConfigurationManager.AppSettings["Supabase_ApiKey"]);
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", ConfigurationManager.AppSettings["Supabase_ApiKey"]);
                client.DefaultRequestHeaders.Add("Prefer", "return=representation");

                var json = JsonConvert.SerializeObject(newOrder);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(tableEndpoint, content);
                if (!response.IsSuccessStatusCode)
                {
                    return BadRequest("建立訂單失敗");
                }

                var resultJson = await response.Content.ReadAsStringAsync();
                var insertedOrders = JsonConvert.DeserializeObject<List<SupabaseOrderDto>>(resultJson);

                if (insertedOrders == null || insertedOrders.Count == 0)
                {
                    return BadRequest("Supabase 沒有回傳新訂單");
                }

                var insertedOrder = insertedOrders.First();
                string orderId = insertedOrder.id.ToString();

                // 2. 用 Supabase 回傳的 id 來產生訂單編號
                string cleanGuid = orderId.Replace("-", "");
                string shortGuid = cleanGuid.Length >= 12 ? cleanGuid.Substring(0, 12) : cleanGuid;
                string timestamp = DateTime.Now.ToString("yyyyMMddHHmm");
                string orderNo = $"ORD{shortGuid}{timestamp}";

                // 3. 更新 Supabase 的 order_no 欄位
                var updateOrder = new { order_no = orderNo };
                var updateJson = JsonConvert.SerializeObject(updateOrder);
                var updateContent = new StringContent(updateJson, Encoding.UTF8, "application/json");

                var patchRequest = new HttpRequestMessage(new HttpMethod("PATCH"), $"{tableEndpoint}?id=eq.{orderId}")
                {
                    Content = updateContent
                };

                var updateResponse = await client.SendAsync(patchRequest);

                if (!updateResponse.IsSuccessStatusCode)
                {
                    return BadRequest("更新訂單編號失敗");
                }


                ////1. 產生訂單編號
                ////(1) 先移除 GUID 的 "-" 符號
                //string cleanGuid = OrderId.Replace("-", "");

                ////(2) 取前 12 碼作為訂單編號的一部分
                //string shortGuid = cleanGuid.Length >= 12 ? cleanGuid.Substring(0, 12) : cleanGuid;

                ////(3) 使用年月日時分 (12 碼)
                //string timestamp = DateTime.Now.ToString("yyyyMMddHHmm");

                ////(4) 組成訂單編號 (ORD + shortGuid + timestamp = 3 + 12 + 12 = 27 碼)
                //string orderNo = $"ORD{shortGuid}{timestamp}";


                //商品描述
                string itemDesc = $"儲值{request.BagelCount}個貝果";

                //建立 tradeData
                var tradeData = new Dictionary<string, string>
            {
                {"MerchantID", MerchantID},
                {"RespondType", "JSON"},
                {"TimeStamp", DateTimeOffset.Now.ToUnixTimeSeconds().ToString()},
                {"Version", "2.0"},
                {"MerchantOrderNo", orderNo},
                {"Amt", request.Amount.ToString()},
                {"ItemDesc", itemDesc},
                {"Email", request.UserEmail},
                // 關鍵參數：由後端指定，優先於平台設定
                {"ReturnURL", "https://gobetgoal.rocket-coding.com/api/payments/return"},
                {"NotifyURL", "https://gobetgoal.rocket-coding.com/api/payments/result"}
            };

                //加密資料
                string tradeInfo = EncryptAES(tradeData);

                //生成 SHA256 驗證碼
                string tradeSha = GetSHA256($"HashKey={HashKey}&{tradeInfo}&HashIV={HashIV}");

                // 回傳前端
                var responseDto = new PaymentOrderFormDto
                {
                    MerchantID = MerchantID,
                    MerchantOrderNo = orderNo,
                    TradeInfo = tradeInfo,
                    TradeSha = tradeSha,
                    Version = "2.0",
                    PayGateWay = PayGateWay,
                };

                return Ok(responseDto);
            }
       
        }



        // 前景通知 (ReturnURL)、使用者付款流程結束後會 redirect 到這裡
        [AllowAnonymous]
        [HttpPost,HttpGet]
        [Route("api/payments/return")]
        public IHttpActionResult ReturnURL()
        {
            string tradeInfo = null;

            try
            {
                if (HttpContext.Current.Request.HttpMethod.Equals("POST", StringComparison.OrdinalIgnoreCase))
                {
                    // POST 取 x-www-form-urlencoded
                    //tradeInfo = HttpContext.Current.Request.Form["TradeInfo"];
                    tradeInfo = HttpContext.Current.Request.Unvalidated.Form["TradeInfo"]
                     ?? HttpContext.Current.Request.Form["TradeInfo"]
                     ?? HttpContext.Current.Request.Params["TradeInfo"];
                }
                else
                {
                    // GET 取 query string
                    tradeInfo = HttpContext.Current.Request.QueryString["TradeInfo"];
                }

                if (string.IsNullOrEmpty(tradeInfo))
                {
                    Trace.TraceWarning("ReturnURL: No TradeInfo received");
                    return BadRequest("No TradeInfo received.");
                }

                //解密 TradeInfo
                var decrypted = "";
                try
                {
                    //測試用
                    if (tradeInfo == "test")
                    {
                        decrypted = "{\"Status\":\"SUCCESS\",\"Message\":\"付款成功\",\"Result\":{\"MerchantOrderNo\":\"12345\",\"Amt\":1000}}";

                    }
                    else
                    {
                        decrypted = DecryptAES(tradeInfo);
                    }
                }
                catch (Exception ex)
                {
                    Trace.TraceError($"ReturnURL 解密失敗: {ex.Message}\n{ex.StackTrace}");
                    return BadRequest("Invalid TradeInfo format");
                }

                //解析 JSON
                TradeInfoResponseDto result = null;
                //var decrypted = DecryptAES(tradeInfo);
                try
                {
                    result = JsonConvert.DeserializeObject<TradeInfoResponseDto>(decrypted);
                    if (result == null || result.Result == null)
                    {
                        Trace.TraceWarning("ReturnURL: TradeInfo format invalid");
                        return BadRequest("Invalid TradeInfo format");
                    }
                }
                catch (Exception ex)
                {
                    Trace.TraceError($"ReturnURL 解析 JSON 失敗: {ex.Message}\n{ex.StackTrace}");
                    return BadRequest("Invalid TradeInfo format");
                }


                //判斷交易狀態
                string status = result.Status?.Trim().ToLower() ?? "fail";         //成功或錯誤代碼 (可用ToLower()轉為小寫)
                string message = result.Message ?? "";          
                string orderNo = result.Result?.MerchantOrderNo ?? "";
                string amount = result.Result?.Amt.ToString() ?? "0";

                //後端設定判斷 前端路由模式 使用
                bool useHashRouter = true; //true = HashRouter, false = BrowserRouter

                //組成導向網址
                string baseUrl = "https://gobetgoal.vercel.app";
                string path = "/shop";
                //string query = $"?status={status}&orderNo={orderNo}&message={Uri.EscapeDataString(message)}";
                string formattedMessage = $"{status}:{message}";
                string redirectUrl;
                string html;

                if (useHashRouter)
                {
                    redirectUrl = $"{baseUrl}";
                    //redirectUrl = $"{baseUrl}/# {path}{query}";
                }
                else
                {
                    redirectUrl = $"{baseUrl}";
                    //redirectUrl = $"{baseUrl}{path}{query}";
                }

                if (status=="success")
                {
                    //回傳 HTML + JS 導向 交易成功頁，並顯示提示訊息
                    html = $@"<html>
                                    <head>
                                        <meta charset='utf-8'/>
                                        <title>付款結果導向</title>
                                        <script>
                                            window.location.href = '{redirectUrl}{path}?status=success&orderNo={orderNo}';
                                        </script>
                                    </head>
                                    <body>
                                        <p>付款結果處理中，請稍候...</p>
                                    </body>
                                 </html>";
                }
                else 
                {
                    //回傳 HTML + JS 導向 交易失敗頁，並顯示提示訊息
                     html = $@"<html>
                                    <head>
                                        <meta charset='utf-8'/>
                                        <title>付款結果導向</title>
                                        <script>
                                            window.location.href = '{redirectUrl}{path}?status=fail&orderNo={orderNo}&message={Uri.EscapeDataString(formattedMessage)}'; 
                                        </script>
                                    </head>
                                    <body>
                                        <p>付款結果處理中，請稍候...</p>
                                    </body>
                                 </html>";
                }
               

                var response = new HttpResponseMessage
                {
                    Content = new StringContent(html, Encoding.UTF8, "text/html")
                };

                 return ResponseMessage(response);
            }catch (Exception ex)
            {
                // 捕捉所有未預期錯誤，避免整個 API 500
                Trace.TraceError($"ReturnURL 未預期錯誤: {ex.Message}\n{ex.StackTrace}");
                return BadRequest("Server error processing TradeInfo");
            }
        }



        // 藍新背景通知 (NotifyURL)
        [AllowAnonymous]
        [HttpPost, Route("api/payments/result")]
        public IHttpActionResult NotifyURL()
        {

            // 藍新會以 form-data/ x-www-form-urlencoded 回傳
            string tradeInfo = HttpContext.Current.Request.Form["TradeInfo"];

            if (string.IsNullOrEmpty(tradeInfo))
            {
                Trace.TraceWarning("NotifyURL: No TradeInfo received");
                return Ok("1|OK");
                //return BadRequest("No TradeInfo received.");
            }

            TradeInfoResponseDto result = null;
            try
            {
                var decrypted = DecryptAES(tradeInfo);
                // 解析 JSON
                result = JsonConvert.DeserializeObject<TradeInfoResponseDto>(decrypted);

                if (result == null || result.Result == null)
                {
                    Trace.TraceWarning("NotifyURL: No TradeInfo received");
                    return Ok("1|OK");
                    //return BadRequest("Invalid TradeInfo format.");
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError($"NotifyURL 解密/解析失敗: {ex.Message}\n{ex.StackTrace}");
                return Ok("1|OK");
            }

            //更新資料庫訂單狀態邏輯
            try
            {
                //更新 Supabase 資料
                using (var client = new HttpClient())
                {
                    // 要更新的欄位
                    var payload = new { status = result?.Status?.ToLower() ?? "fail" };
                    var json = JsonConvert.SerializeObject(payload);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    // Supabase 必要 Header
                    client.DefaultRequestHeaders.Add("apikey", ConfigurationManager.AppSettings["Supabase_ApiKey"]);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ConfigurationManager.AppSettings["Supabase_ApiKey"]);

                    // PATCH 請求
                    var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"https://rbrltczejudsoxphrxnq.supabase.co/rest/v1/deposit?order_no=eq.{result.Result.MerchantOrderNo}")
                    {
                        Content = content
                    };

                    var response = client.SendAsync(request).Result;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError($"NotifyURL 更新 Supabase 失敗: {ex.Message}\n{ex.StackTrace}");
            }

            return Ok("1|OK"); // 必須回傳表示接收成功，否則藍新會重複通知
        }



        #region AES / SHA 加解密方法

        private string EncryptAES(Dictionary<string, string> tradeData)
        {
            string query = string.Join("&", tradeData.Select(x => $"{x.Key}={x.Value}"));

            using (var aes = new AesCryptoServiceProvider())
            {
                aes.Key = Encoding.UTF8.GetBytes(HashKey);
                aes.IV = Encoding.UTF8.GetBytes(HashIV);
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                var src = Encoding.UTF8.GetBytes(query);

                var encrypted = encryptor.TransformFinalBlock(src, 0, src.Length);
                return BitConverter.ToString(encrypted).Replace("-", "").ToLower();
            }
        }

        private string DecryptAES(string encryptedHex)
        {
            byte[] encrypted = new byte[encryptedHex.Length / 2];
            for (int i = 0; i < encrypted.Length; i++)
                encrypted[i] = Convert.ToByte(encryptedHex.Substring(i * 2, 2), 16);

            using (var aes = new AesCryptoServiceProvider())
            {
                aes.Key = Encoding.UTF8.GetBytes(HashKey);
                aes.IV = Encoding.UTF8.GetBytes(HashIV);
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                var decrypted = decryptor.TransformFinalBlock(encrypted, 0, encrypted.Length);
                return Encoding.UTF8.GetString(decrypted);
            }
        }

        private string GetSHA256(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                var hash = sha256.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToUpper();
            }
        }

        #endregion
    }
}
