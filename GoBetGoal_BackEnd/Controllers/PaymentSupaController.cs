using GoBetGoal_BackEnd.Models;
using GoBetGoal_BackEnd.Models.DTOs;
using Newtonsoft.Json;
using Swashbuckle.Swagger;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
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

        /// <summary>
        /// 建立交易並回傳給前端需要的資料
        /// </summary>
        [HttpPost]
        [Route("api/payments/create")]
        public IHttpActionResult CreatePayment(PaymentRequestDto request)
        {
            // 產生訂單編號
            // 先移除 GUID 的 "-" 符號
            string cleanGuid = request.OrderId.Replace("-", "");

            // 取前 12 碼作為訂單編號的一部分
            string shortGuid = cleanGuid.Length >= 12 ? cleanGuid.Substring(0, 12) : cleanGuid;

            // 使用年月日時分 (12 碼)
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmm");

            // 組成訂單編號 (ORD + shortGuid + timestamp = 3 + 12 + 12 = 27 碼)
            string orderNo = $"ORD{shortGuid}{timestamp}";

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
            var response = new PaymentOrderFormDto
            {
                MerchantID = MerchantID,
                MerchantOrderNo = orderNo,
                TradeInfo = tradeInfo,
                TradeSha = tradeSha,
                Version = "2.0",
                PayGateWay = PayGateWay,
            };

            return Ok(response);
        }

        /// <summary>
        /// 前景通知 (ReturnURL)
        /// 使用者付款流程結束後會 redirect 到這裡
        /// </summary>
        [HttpPost]
        [Route("api/payments/return")]
        public IHttpActionResult ReturnURL()
        {
            string tradeInfo = null;

            if (HttpContext.Current.Request.HttpMethod == "POST")
            {
                // POST 取 form-data
                tradeInfo = HttpContext.Current.Request.Form["TradeInfo"];
            }
            else
            {
                // GET 取 query string
                tradeInfo = HttpContext.Current.Request.QueryString["TradeInfo"];
            }

            if (string.IsNullOrEmpty(tradeInfo))
            {
                return BadRequest("No TradeInfo received.");
            }

            var decrypted = DecryptAES(tradeInfo);
            var result = JsonConvert.DeserializeObject<PaymentResponseDto>(decrypted);

            // 導回前端頁面
            string frontendUrl = $"https://your-frontend.com/payment/result" +
                                 $"?status={result.Status}" +
                                 $"&orderNo={result.Result.MerchantOrderNo}" +
                                 $"&amount={result.Result.Amt}";
            return Redirect(frontendUrl);
        }


        /// <summary>
        /// 藍新背景通知 (NotifyURL)
        /// </summary>
        [HttpPost, Route("api/payments/result")]
        public IHttpActionResult NotifyURL()
        {

            // 藍新會以 form-data 回傳
            string tradeInfo = HttpContext.Current.Request.Form["TradeInfo"];

            var decrypted = DecryptAES(tradeInfo);

            // 解析 JSON
            var result = JsonConvert.DeserializeObject<PaymentResponseDto>(decrypted);

            // 更新資料庫訂單狀態邏輯
            // 將交易結果轉發給 Supabase Edge Function
            using (var client = new HttpClient())
            {
                var json = JsonConvert.SerializeObject(result);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = client.PostAsync("https://GoBetGoal.functions.supabase.co/payment-callback", content).Result;
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
