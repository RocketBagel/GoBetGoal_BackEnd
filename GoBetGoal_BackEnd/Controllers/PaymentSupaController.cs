using GoBetGoal_BackEnd.Models;
using GoBetGoal_BackEnd.Models.DTOs;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
using static Jose.Jwk;


namespace GoBetGoal_BackEnd.Controllers
{
    public class PaymentSupaController : ApiController
    {
        private readonly string MerchantID = ConfigurationManager.AppSettings["NewebPay_MerchantID"];
        private readonly string HashKey = ConfigurationManager.AppSettings["NewebPay_HashKey"];
        private readonly string HashIV = ConfigurationManager.AppSettings["NewebPay_HashIV"];
        private readonly string PayGateWay = ConfigurationManager.AppSettings["NewebPay_ApiUrl_Test"];
        private readonly string SupabaseUrl = "https://rbrltczejudsoxphrxnq.supabase.co";
        private readonly string ApiKey = ConfigurationManager.AppSettings["Supabase_ApiKey"];
        // <summary>
        // 建立交易並回傳給前端需要的資料
        // </summary>
        [AllowAnonymous]
        [HttpPost]
        [Route("api/payments/create")]
        public async Task<IHttpActionResult> CreatePayment([FromBody] PaymentRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                // 回傳 ModelState 的第一個錯誤訊息
                var errorMessage = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return BadRequest(errorMessage);
            }


            //// UserId 與 UserEmail 不得為空
            //if (request.UserId == null || request.UserEmail == null)
            //{
            //    return BadRequest("格式錯誤：UserId 與 UserEmail 不可為空");
            //}

            //// Amount 與 BagelCount 必須為正整數
            //if (!int.TryParse(request.Amount.ToString(), out int amount))
            //{
            //    return BadRequest("格式錯誤");
            //}

            //if (!int.TryParse(request.BagelCount.ToString(), out int bagelCount))
            //{
            //    return BadRequest("BagelCount格式錯誤");
            //}

            // 先檢查 UserId 與 UserEmail 是否匹配
            using (var client = new HttpClient())
            {
               
                string userEndpoint = $"{SupabaseUrl}/auth/v1/admin/users/{Uri.EscapeDataString(request.UserId.ToString())}";

                client.DefaultRequestHeaders.Add("apikey", ConfigurationManager.AppSettings["Supabase_ApiKey"]);
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", ConfigurationManager.AppSettings["Supabase_ApiKey"]);

                var userResponse = await client.GetAsync(userEndpoint);
                if (!userResponse.IsSuccessStatusCode)
                {
                    return BadRequest("查詢使用者失敗");
                }

                var userJson = await userResponse.Content.ReadAsStringAsync();
                var user = JsonConvert.DeserializeObject<SupabaseAuthUserDto>(userJson);

                if (user == null || string.IsNullOrEmpty(user.Id))
                {
                    return BadRequest("使用者不存在");
                }

                if (!string.Equals(user.Email, request.UserEmail, StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest("Email 與 User 不符");
                }
            }

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
                
                string tableEndpoint = $"{SupabaseUrl}/rest/v1/deposit";

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
                    {"LoginType","0" },
                    {"ReturnURL", $"https://gobetgoal.rocket-coding.com/api/payments/return/{orderNo}"},
                    {"NotifyURL", $"https://gobetgoal.rocket-coding.com/api/payments/result/{orderNo}"}
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
        [Route("api/payments/return/{orderNo}")]
        public IHttpActionResult ReturnURL(string orderNo)
        {
            string tradeInfo = null;
            string PaymentStatus = null;

            try
            {
                if (HttpContext.Current.Request.HttpMethod.Equals("POST", StringComparison.OrdinalIgnoreCase))
                {
                    // POST 取 x-www-form-urlencoded
                    //tradeInfo = HttpContext.Current.Request.Form["TradeInfo"];
                    PaymentStatus = HttpContext.Current.Request.Unvalidated.Form["Status"] 
                     ?? HttpContext.Current.Request.Form["Status"]
                     ?? HttpContext.Current.Request.Params["Status"];
                    tradeInfo = HttpContext.Current.Request.Unvalidated.Form["TradeInfo"]
                     ?? HttpContext.Current.Request.Form["TradeInfo"]
                     ?? HttpContext.Current.Request.Params["TradeInfo"];
                    WriteLog($"return_TradeInfo: POST:{tradeInfo}");

                }
                else
                {
                    // GET 取 query string
                    tradeInfo = HttpContext.Current.Request.QueryString["TradeInfo"];
                    PaymentStatus = HttpContext.Current.Request.QueryString["Status"];
                    WriteLog($"return_TradeInfo: GET:{tradeInfo}");
                }

                if (string.IsNullOrEmpty(tradeInfo))
                {
                    return ReturnFailPage(orderNo,"ReturnURL: No TradeInfo received", isError: false);
                    //Trace.TraceWarning("ReturnURL: No TradeInfo received");
                    //WriteLog("ReturnURL: No TradeInfo received:IsNullOrEmpty(tradeInfo)");
                  
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
                        WriteLog($"ReturnURL:{decrypted}");
                    }

                //}
                //catch (Exception ex)
                //{
                //    return ReturnFailPage(orderNo, $"ReturnURL 解密失敗: {ex.Message}\n{ex.StackTrace}", isError: true);
                //}
                }
                catch (Exception ex)
                {
                   
                    Trace.TraceError($"ReturnURL 解密失敗: {ex.Message}\n{ex.StackTrace}");
                    WriteLog($"ReturnURL 解密失敗: {ex.Message}\n{ex.StackTrace}");
                    return BadRequest("Invalid TradeInfo format");
                }


                //解析 JSON
                TradeInfoResponseDto result = null;
                //var decrypted = DecryptAES(tradeInfo);
                try
                {
                    result = JsonConvert.DeserializeObject<TradeInfoResponseDto>(decrypted);
                  
                    //if (result == null || result.Result == null)
                    //{
                    //    return ReturnFailPage(orderNo, "ReturnURL: TradeInfo format invalid (result == null)", isError: false);
                    //}
                }
                catch (Exception ex)
                {
                    WriteLog($"ReturnURL 解析 JSON 失敗: {ex.Message}\n{ex.StackTrace}");
                    //return ReturnFailPage(orderNo, $"ReturnURL 解析 JSON 失敗: {ex.Message}\n{ex.StackTrace}", isError: true);
                    if (result == null || result.Result == null)
                    {
                        Trace.TraceWarning("ReturnURL: TradeInfo format invalid");
                        WriteLog($"ReturnURL: TradeInfo format invalid:result == null || result.Result == null");
                        return BadRequest("Invalid TradeInfo format");
                    }
                }
                //catch (Exception ex)
                //{
                //    Trace.TraceError($"ReturnURL 解析 JSON 失敗: {ex.Message}\n{ex.StackTrace}");
                //    WriteLog($"ReturnURL 解析 JSON 失敗: {ex.Message}\n{ex.StackTrace}");
                //    return BadRequest("Invalid TradeInfo format");
                //}


                //判斷交易狀態
                string status = result.Status?.Trim().ToLower() ?? "fail";         //成功或錯誤代碼 (可用ToLower()轉為小寫)
                string message = result.Message ?? "";          
                string MerchantorderNo = result.Result?.MerchantOrderNo ?? "";
                string amount = result.Result?.Amt.ToString() ?? "0";

                //後端設定判斷 前端路由模式 使用
                //bool useHashRouter = true; //true = HashRouter, false = BrowserRouter

                //組成導向網址
                string baseUrl = "https://gobetgoal.vercel.app";
                string path = "/shop";
                //string query = $"?status={status}&orderNo={orderNo}&message={Uri.EscapeDataString(message)}";
                //string formattedMessage = $"{status}:{message}";
                //string redirectUrl;
                //string html;
                //redirectUrl =$"{baseUrl}";
                string redirectUrl;
                //string html;
                redirectUrl = $"{baseUrl}";

                //if (useHashRouter)
                //{
                //    redirectUrl = $"{baseUrl}/# {path}{query}";
                //}
                //else
                //{
                //    redirectUrl = $"{baseUrl}{path}{query}";
                //}

                if (status=="success" || PaymentStatus == "SUCCESS")
                {
                    //回傳 HTML + JS 導向 交易成功頁，並顯示提示訊息
                    return ReturnSuccessPage(orderNo, baseUrl, path);

//                if (status=="success")
//                {
//                    //回傳 HTML + JS 導向 交易成功頁，並顯示提示訊息
//                    html = $@"<html>
//                                    <head>
//                                        <meta charset='utf-8'/>
//                                        <title>付款結果導向</title>
//                                        <script>
//                                            window.location.href = '{redirectUrl}{path}?status=success';
//                                        </script>
//                                    </head>
//                                    <body>
//                                        <p>付款結果處理中，請稍候...</p>
//                                    </body>
//                                 </html>";

                }
                else 
                {
                    //回傳 HTML + JS 導向 交易失敗頁，並顯示提示訊息

                    return ReturnFailPage(orderNo,$"ReturnURL: Trade status not success. Status={status}, Message={message}",isError:false);

                     //html = $@"<html>
                     //               <head>
                     //                   <meta charset='utf-8'/>
                     //                   <title>付款結果導向</title>
                     //                   <script>
                     //                       window.location.href = '{redirectUrl}{path}?status=fail'; 
                     //                   </script>
                     //               </head>
                     //               <body>
                     //                   <p>付款結果處理中，請稍候...</p>
                     //               </body>
                     //            </html>";
                }
    
            }catch (Exception ex)
            {

                // 捕捉所有未預期錯誤，避免整個 API 500 
                return ReturnFailPage(orderNo,$"ReturnURL 未預期錯誤: {ex.Message}\n{ex.StackTrace}",isError:true);

                // 捕捉所有未預期錯誤，避免整個 API 500
                //Trace.TraceError($"ReturnURL 未預期錯誤: {ex.Message}\n{ex.StackTrace}");
                //WriteLog($"ReturnURL 未預期錯誤: {ex.Message}\n{ex.StackTrace}");
                //return BadRequest("Server error processing TradeInfo");

            }
        }



        // 藍新背景通知 (NotifyURL)
        [AllowAnonymous]
        [HttpPost]
        [Route("api/payments/result/{orderNo}")]
        public async Task<IHttpActionResult> NotifyURL(string orderNo)

        {

            try
            {
                // 1. 取得 NotifyURL 的 raw form 資料
                var form = HttpContext.Current.Request.Form;
                string tradeInfoRaw = form["TradeInfo"];
                string tradeSha = form["TradeSha"];
                string status = form["Status"];

                // 2. 驗證 TradeSha
                string checkSha = GetSHA256($"HashKey={HashKey}&{tradeInfoRaw}&HashIV={HashIV}");
                if (checkSha != tradeSha)
                {
                    WriteLog("TradeSha 驗證失敗，可能資料被竄改");
                    System.Diagnostics.Debug.WriteLine("TradeSha 驗證失敗，可能資料被竄改");
                    return Ok("1|OK");
                }

                // 3. 解密 TradeInfo
                TradeInfoResponseDto tradeInfo = null;
                try
                {
                    string decrypted = DecryptAES(tradeInfoRaw);
                    tradeInfo = JsonConvert.DeserializeObject<TradeInfoResponseDto>(decrypted);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"TradeInfo 解密失敗: {ex.Message}");
                    WriteLog($"TradeInfo 解密失敗: {ex.Message}");
                    //return Ok("1|OK");
                }

                // 4. 更新訂單狀態
                if (tradeInfo != null && tradeInfo.Result != null && !string.IsNullOrEmpty(tradeInfo.Result.MerchantOrderNo))
                {
                    // 解密成功 → 用 MerchantOrderNo 找訂單
                    await UpdateOrderStatus(tradeInfo.Result.MerchantOrderNo, status);
                }
                else
                {

                    // 解密失敗 → 用 OrderNo 直接更新
                    await UpdateOrderStatus(orderNo, status);

                    // 解密失敗 → fallback，用 created_at 最新一筆 pending 訂單
                    //var lastOrder = await FindLastOrderFromSupabase();
                    //if (lastOrder != null)
                    //{
                    //    await UpdateOrderStatus(lastOrder.order_no, status);
                    //}
                    //else
                    //{
                    //    System.Diagnostics.Debug.WriteLine("找不到任何 pending 訂單，無法更新狀態");
                    //    WriteLog("找不到任何 pending 訂單，無法更新狀態");
                    //}
                }
                return Ok("1|OK");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"NotifyURL 例外錯誤: {ex}");
                WriteLog($"NotifyURL 例外錯誤: {ex}");
                return Ok("1|OK");
            }

        }

  


        [AllowAnonymous]
        [HttpPost, Route("api/payments/result/debug")]
        public IHttpActionResult NotifyURLDebug()
        {
            try
            {
                // 1. 取得 POST 的 raw form data
                string rawForm = HttpContext.Current.Request.Form.ToString();
                WriteLog($"NotifyURLDebug: raw form data: {rawForm}");
                Trace.TraceInformation($"NotifyURLDebug: raw form data: {rawForm}");

                // 2. 解析 Query String
                var parsed = HttpUtility.ParseQueryString(rawForm);

                string status = parsed["Status"];
                string merchantID = parsed["MerchantID"];
                string version = parsed["Version"];
                string tradeInfoHex = parsed["TradeInfo"]; // 加密內容
                string tradeSha = parsed["TradeSha"];

                WriteLog($"NotifyURLDebug: TradeInfo (Hex) length={tradeInfoHex?.Length}");
                Trace.TraceInformation($"NotifyURLDebug: TradeInfo (Hex) length={tradeInfoHex?.Length}");

                if (string.IsNullOrEmpty(tradeInfoHex))
                {
                    WriteLog("NotifyURLDebug: TradeInfo is empty");
                    Trace.TraceWarning("NotifyURLDebug: TradeInfo is empty");
                    return Ok("1|OK");
                }

                // 3. 解密 TradeInfo Hex
                string decryptedJson = DecryptAES(tradeInfoHex);
                WriteLog($"NotifyURLDebug: Decrypted TradeInfo JSON: {decryptedJson}");
                Trace.TraceInformation($"NotifyURLDebug: Decrypted TradeInfo JSON: {decryptedJson}");

                // 4. 解析 JSON
                var tradeDetail = JsonConvert.DeserializeObject<TradeInfoResponseDetail>(decryptedJson);
                if (tradeDetail == null)
                {
                    WriteLog("NotifyURLDebug: Failed to parse decrypted TradeInfo JSON");
                    Trace.TraceWarning("NotifyURLDebug: Failed to parse decrypted TradeInfo JSON");
                }
                else
                {
                    WriteLog($"NotifyURLDebug: MerchantOrderNo={tradeDetail.MerchantOrderNo}, Amt={tradeDetail.Amt}, PaymentType={tradeDetail.PaymentType}");
                    Trace.TraceInformation($"NotifyURLDebug: MerchantOrderNo={tradeDetail.MerchantOrderNo}, Amt={tradeDetail.Amt}, PaymentType={tradeDetail.PaymentType}");
                }
            }
            catch (Exception ex)
            {
                WriteLog($"NotifyURLDebug Exception: {ex.Message}\n{ex.StackTrace}");
                Trace.TraceError($"NotifyURLDebug Exception: {ex.Message}\n{ex.StackTrace}");
            }

            return Ok("1|OK");
        }

     

        /// <summary>
        /// 產生交易失敗導向頁
        /// </summary>
        private IHttpActionResult ReturnFailPage(string orderNo, string logMessage, bool isError = false)
        {
            if (isError)
            {
                Trace.TraceError(logMessage);
            }
            else
            {
                Trace.TraceWarning(logMessage);
            }

            WriteLog(logMessage);

            string baseUrl = "https://gobetgoal.vercel.app";
            string path = "/shop";

            string html = $@"<html>
                       <head>
                           <meta charset='utf-8'/>
                           <title>付款結果導向</title>
                           <script>
                               window.location.href = '{baseUrl}{path}?status=fail&{orderNo}'; 
                           </script>
                       </head>
                       <body>
                           <p>付款結果處理中，請稍候...</p>
                       </body>
                    </html>";

            var response = new HttpResponseMessage
            {
                Content = new StringContent(html, Encoding.UTF8, "text/html")
            };

            return ResponseMessage(response);
        }

        /// <summary>
        /// 產生交易成功導向頁
        /// </summary>
        private IHttpActionResult ReturnSuccessPage(string orderNo, string baseUrl, string path)
        {
            string html = $@"<html>
                       <head>
                           <meta charset='utf-8'/>
                           <title>付款結果導向</title>
                           <script>
                               window.location.href = '{baseUrl}{path}?status=success&{orderNo}'; 
                           </script>
                       </head>
                       <body>
                           <p>付款結果處理中，請稍候...</p>
                       </body>
                    </html>";

            var response = new HttpResponseMessage
            {
                Content = new StringContent(html, Encoding.UTF8, "text/html")
            };

            return ResponseMessage(response);
        }

      
        //[AllowAnonymous]
        //[HttpPost, Route("api/payments/result/debug")]
        //public IHttpActionResult NotifyURLDebug()
        //{
        //    try
        //    {
        //        // 1. 取得 POST 的 raw form data
        //        string rawForm = HttpContext.Current.Request.Form.ToString();
        //        WriteLog($"NotifyURLDebug: raw form data: {rawForm}");
        //        Trace.TraceInformation($"NotifyURLDebug: raw form data: {rawForm}");

        //        // 2. 解析 Query String
        //        var parsed = HttpUtility.ParseQueryString(rawForm);

        //        string status = parsed["Status"];
        //        string merchantID = parsed["MerchantID"];
        //        string version = parsed["Version"];
        //        string tradeInfoHex = parsed["TradeInfo"]; // 加密內容
        //        string tradeSha = parsed["TradeSha"];

        //        WriteLog($"NotifyURLDebug: TradeInfo (Hex) length={tradeInfoHex?.Length}");
        //        Trace.TraceInformation($"NotifyURLDebug: TradeInfo (Hex) length={tradeInfoHex?.Length}");

        //        if (string.IsNullOrEmpty(tradeInfoHex))
        //        {
        //            WriteLog("NotifyURLDebug: TradeInfo is empty");
        //            Trace.TraceWarning("NotifyURLDebug: TradeInfo is empty");
        //            return Ok("1|OK");
        //        }

        //        // 3. 解密 TradeInfo Hex
        //        string decryptedJson = DecryptAES(tradeInfoHex);
        //        WriteLog($"NotifyURLDebug: Decrypted TradeInfo JSON: {decryptedJson}");
        //        Trace.TraceInformation($"NotifyURLDebug: Decrypted TradeInfo JSON: {decryptedJson}");

        //        // 4. 解析 JSON
        //        var tradeDetail = JsonConvert.DeserializeObject<TradeInfoResponseDetail>(decryptedJson);
        //        if (tradeDetail == null)
        //        {
        //            WriteLog("NotifyURLDebug: Failed to parse decrypted TradeInfo JSON");
        //            Trace.TraceWarning("NotifyURLDebug: Failed to parse decrypted TradeInfo JSON");
        //        }
        //        else
        //        {
        //            WriteLog($"NotifyURLDebug: MerchantOrderNo={tradeDetail.MerchantOrderNo}, Amt={tradeDetail.Amt}, PaymentType={tradeDetail.PaymentType}");
        //            Trace.TraceInformation($"NotifyURLDebug: MerchantOrderNo={tradeDetail.MerchantOrderNo}, Amt={tradeDetail.Amt}, PaymentType={tradeDetail.PaymentType}");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        WriteLog($"NotifyURLDebug Exception: {ex.Message}\n{ex.StackTrace}");
        //        Trace.TraceError($"NotifyURLDebug Exception: {ex.Message}\n{ex.StackTrace}");
        //    }

        //    return Ok("1|OK");
        //}

            //// 藍新會以 form-data/ x-www-form-urlencoded 回傳
            //string rawBody = new StreamReader(HttpContext.Current.Request.InputStream).ReadToEnd();
            //WriteLog($"Notify RawBody: {rawBody}");


            //// 優先從 Form 取值
            //string tradeInfo = HttpContext.Current.Request.Form["TradeInfo"];
            //string tradeSha = HttpContext.Current.Request.Form["TradeSha"];
            //string status = HttpContext.Current.Request.Form["Status"];


            //// 如果 Form 取不到，再從 rawBody 手動解析
            //if (string.IsNullOrEmpty(tradeInfo) && !string.IsNullOrEmpty(rawBody))
            //{
            //    var parsed = HttpUtility.ParseQueryString(rawBody);
            //    tradeInfo = parsed["TradeInfo"];
            //}

            //// 有些情況 TradeInfo 會再被 UrlEncode 一次 → 需要解一次碼
            //if (!string.IsNullOrEmpty(tradeInfo) && tradeInfo.Contains("%"))
            //{
            //    tradeInfo = HttpUtility.UrlDecode(tradeInfo);
            //}
            //WriteLog($"Notify TradeInfo(final): {tradeInfo}");


            ////驗證 TradeSha
            //string checkValue = $"HashKey={HashKey}&{tradeInfo}&HashIV={HashIV}";
            //string sha256Value = GetSHA256(checkValue).ToUpper();

            //if (sha256Value != tradeSha)
            //{
            //    WriteLog("success = false, message = TradeSha 驗證失敗");
            //    return Json(new { success = false, message = "TradeSha 驗證失敗" });
            //}




            ////if (string.IsNullOrEmpty(tradeInfo))
            ////{
            ////    Trace.TraceWarning("NotifyURL: No TradeInfo received");
            ////    WriteLog("NotifyURL: No TradeInfo received:string.IsNullOrEmpty(tradeInfo)");
            ////    return Ok("1|OK");

            ////}
            ////string rawTradeInfo = HttpUtility.UrlDecode(tradeInfo);
            ////WriteLog($"rawTradeInfo: {rawTradeInfo}");
            //var result = null;
            //string merchantOrderNo = null;
            //WriteLog($"Notify TradeInfo Length={tradeInfo.Length}, EndsWith={tradeInfo.Substring(tradeInfo.Length - 20)}");

            //try
            //{


            //    // 解析 JSON
            //    string decrypted = DecryptAES(tradeInfo);
            //    WriteLog($"Notify_Decrypted: {decrypted}");
            //    result = JsonConvert.DeserializeObject<Dictionary<string, object>>(decrypted);
            //    WriteLog($"Notify_result:{result}");
            //    // 解析 JSON (測試用，直接把 TradeInfo 當明文JSON)
            //    //result = JsonConvert.DeserializeObject<TradeInfoResponseDto>(tradeInfo);

            //    if (result == null || result.Result == null)
            //    {
            //        Trace.TraceWarning("NotifyURL: No TradeInfo received");
            //        WriteLog("NotifyURL: No TradeInfo received:result == null || result.Result == null");
            //        return Ok("1|OK");

            //    }

            //    if (result.ContainsKey("MerchantOrderNo"))
            //    {
            //        merchantOrderNo = result["MerchantOrderNo"].ToString();
            //        return Ok("1|OK");
            //    }

            //}
            //catch (Exception ex)
            //{
            //    Trace.TraceError($"NotifyURL 解密/解析失敗: {ex.Message}\n{ex.StackTrace}");
            //    WriteLog($"Notify解密/解析失敗: {ex.Message}\n{ex.StackTrace}");
            //    return Ok("1|OK");
            //}

            ////更新資料庫訂單狀態、交易成功更新貝果數邏輯
            ////string status = result.Status?.Trim().ToLower() ?? "fail";
            //if (status == "success")
            //{
            //    try
            //    {
            //        //更新 Supabase 資料
            //        using (var client = new HttpClient())
            //        {
            //            // Supabase 必要 Header
            //            if (!client.DefaultRequestHeaders.Contains("apikey"))
            //            {
            //                client.DefaultRequestHeaders.Add("apikey", ConfigurationManager.AppSettings["Supabase_ApiKey"]);
            //                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ConfigurationManager.AppSettings["Supabase_ApiKey"]);
            //            }

            //            // 更新deposit.status的欄位
            //            var newStatus = status;
            //            var payload = new { status = newStatus };
            //            var json = JsonConvert.SerializeObject(payload);
            //            var content = new StringContent(json, Encoding.UTF8, "application/json");


            //            // PATCH 請求更新status欄位
            //            var patchDeposit = new HttpRequestMessage(new HttpMethod("PATCH"), $"{SupabaseUrl}/rest/v1/deposit?order_no=eq.{result.Result.MerchantOrderNo}")
            //            {
            //                Content = content
            //            };

            //            var responseDeposit = client.SendAsync(patchDeposit).Result;
            //            responseDeposit.EnsureSuccessStatusCode();

            //            //交易成功才加 candy_count
            //            if (newStatus == "success")
            //            {
            //                // 查詢這筆訂單的 user_id 和 get_bagel
            //                var getDeposit = client.GetAsync($"{SupabaseUrl}/rest/v1/deposit?order_no=eq.{result.Result.MerchantOrderNo}&select=user_id,get_bagel").Result;

            //                var depositJson = getDeposit.Content.ReadAsStringAsync().Result;
            //                var depositArray = JArray.Parse(depositJson);

            //                if (depositArray != null && depositArray.Count > 0)
            //                {
            //                    var userId = depositArray[0]["user_id"]?.ToString();
            //                    var getBagel = depositArray[0]["get_bagel"]?.Value<int>() ?? 0;
            //                    WriteLog($"{userId}:{getBagel}");

            //                    if (!string.IsNullOrEmpty(userId) && getBagel > 0)
            //                    {
            //                        // 更新 user_info.candy_count = candy_count + get_bagel
            //                        // Supabase REST API 沒辦法直接做 += ，呼叫 RPC function 增加 candy_count值
            //                        var rpcPayload = new { the_user = userId, amount = getBagel };
            //                        var jsonRpc = JsonConvert.SerializeObject(rpcPayload);
            //                        var contentRpc = new StringContent(jsonRpc, Encoding.UTF8, "application/json");

            //                        var rpcRequest = new HttpRequestMessage(HttpMethod.Post, $"{SupabaseUrl}/rest/v1/rpc/increment_bagel")
            //                        {
            //                            Content = contentRpc
            //                        };


            //                        var rpcResponse = client.SendAsync(rpcRequest).Result;
            //                        rpcResponse.EnsureSuccessStatusCode();
            //                    }
            //                    else
            //                    {
            //                        Trace.TraceWarning($"NotifyURL: user_id 或 get_bagel 無效，userId={userId}, getBagel={getBagel}");
            //                        WriteLog($"NotifyURL: user_id 或 get_bagel 無效，userId={userId}, getBagel={getBagel}");
            //                        return Ok("1|OK");
            //                    }
            //                }
            //                else
            //                {
            //                    Trace.TraceWarning($"NotifyURL: 找不到 deposit 資料, order_no={result.Result.MerchantOrderNo}");
            //                    WriteLog($"NotifyURL: 找不到 deposit 資料, order_no={result.Result.MerchantOrderNo}");

            //                }
            //            }
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        Trace.TraceError($"NotifyURL 更新 Supabase 失敗: {ex.Message}\n{ex.StackTrace}");
            //        WriteLog($"NotifyURL 更新 Supabase 失敗: {ex.Message}\n{ex.StackTrace}");
            //        return Ok("1|OK");
            //    }
            //}

            //return Ok("1|OK"); // 必須回傳表示接收成功，否則藍新會重複通知

      


        private void WriteLog(string text)
        {
            try
            {
                string path = @"C:\temp\notify_log.txt";
                Directory.CreateDirectory(Path.GetDirectoryName(path));

                using (var sw = new StreamWriter(path, true, Encoding.UTF8))
                {
                    sw.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {text}");
                }
            }
            catch { /* 防止 log 寫檔失敗影響交易流程 */ }
        }

        #region Supabase API
        // 查詢最新一筆訂單 (created_at DESC)
        //private async Task<dynamic> FindLastOrderFromSupabase()
        //{
        //    using (var client = new HttpClient())
        //    {
        //        client.DefaultRequestHeaders.Add("apikey", ApiKey);
        //        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);

        //        var url = $"{SupabaseUrl}/rest/v1/deposit?order=created_at.desc&limit=1";

        //        var response = await client.GetAsync(url);
        //        if (!response.IsSuccessStatusCode) 
        //        { 
        //            return null; 
        //        }

        //        var json = await response.Content.ReadAsStringAsync();
        //        var orders = JsonConvert.DeserializeObject<List<SupabaseOrderDto>>(json);
        //        return orders.FirstOrDefault();
        //    }
        //}

        // 更新訂單狀態
        private async Task UpdateOrderStatus(string orderno, string status)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("apikey", ApiKey);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);

                var url = $"{SupabaseUrl}/rest/v1/deposit?order_no=eq.{orderno}";

                var update = new { status = status };
                var json = JsonConvert.SerializeObject(update);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var patchRequest = new HttpRequestMessage(new HttpMethod("PATCH"), url)
                {
                    Content = content
                };

                var response = await client.SendAsync(patchRequest);

                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"更新訂單狀態失敗: {err}");
                    WriteLog($"更新訂單狀態失敗: {err}");
                }
            }
        }
        #endregion

        #region AES / SHA 加解密方法

        private string EncryptAES(Dictionary<string, string> tradeData)
        {
            string query = string.Join("&", tradeData.Select(x => $"{x.Key}={x.Value}"));
            WriteLog($"query:{query}");

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
            if (string.IsNullOrWhiteSpace(encryptedHex))
            {
                WriteLog("TradeInfo 不能為空");
                throw new ArgumentException("TradeInfo 不能為空");
            }


            if (encryptedHex.Length % 2 != 0)
            {
                WriteLog("TradeInfo 長度不是偶數，Hex 格式錯誤");
                throw new Exception("TradeInfo 長度不是偶數，Hex 格式錯誤");
            }

            var bytes = HexStringToBytes(encryptedHex);
            //byte[] bytes = new byte[encryptedHex.Length / 2];
            //for (int i = 0; i < bytes.Length; i++)
            //    bytes[i] = Convert.ToByte(encryptedHex.Substring(i * 2, 2), 16);

            using (var aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(HashKey); 
                aes.IV = Encoding.UTF8.GetBytes(HashIV);
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (var decryptor = aes.CreateDecryptor())
                {
                    var result = decryptor.TransformFinalBlock(bytes, 0, bytes.Length);
                    return Encoding.UTF8.GetString(result);
                }
            }
        }


        private static byte[] HexStringToBytes(string hex)
        {
            int length = hex.Length / 2;
            byte[] result = new byte[length];
            for (int i = 0; i < length; i++)
                result[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            return result;
        }
        // 判斷字串是否為合法 Hex
        private bool IsHexString(string input)
        {
            if (string.IsNullOrEmpty(input)) return false;
            return input.All(c => Uri.IsHexDigit(c));
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
