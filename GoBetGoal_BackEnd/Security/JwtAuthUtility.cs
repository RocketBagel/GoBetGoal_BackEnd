using GoBetGoal_BackEnd.Models;
using Jose;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Configuration;

namespace GoBetGoal_BackEnd.Security
{
    /// <summary>
    /// JwtToken 生成功能
    /// </summary>
    public class JwtAuthUtility
    {
        private static readonly string secretKey = WebConfigurationManager.AppSettings["TokenKey"];

        //private readonly ApplicationDbContext db = new ApplicationDbContext(); // DB 連線 //<驗證帳號密碼成功後就不開db>

        /// <summary>
        /// 生成 JwtToken
        /// </summary>
        /// <param name="id">會員id</param>
        /// <returns>JwtToken</returns>
        public string GenerateToken(User user, int? expiresInMinutes=null)
        {
            // 自訂字串，驗證用，用來加密送出的 key (放在 Web.config 的 appSettings)
            //string secretKey = WebConfigurationManager.AppSettings["TokenKey"]; // 從 appSettings 取出
            //string secretKey = "ILoveCoding";
            //var user = db.User.Find(id); // 進 DB 取出想要夾帶的基本資料 //<驗證帳號密碼成功後就不開db>

            DateTime expirationTime;

            if (expiresInMinutes.HasValue)
            {
                // 如果有提供分鐘數（例如，用於密碼重設），就使用該時效
                expirationTime = DateTime.Now.AddMinutes(expiresInMinutes.Value);
            }
            else
            {
                // 如果未提供，就使用預設的長時效（例如 3 個月）
                expirationTime = DateTime.Now.AddMonths(3);
            }

            // payload作法1 需透過 token 傳遞的資料 (可夾帶常用且不重要的資料)
            var payload = new Dictionary<string, object>
            {
                // --- 必備的核心金鑰 ---
                { "Id", user.Id },
                { "Exp", expirationTime.ToString() }, // JwtToken 時效設定 
                // --- 建議加入的標準金鑰 ---
                { "iss", "GoBetGoalApi" }, // 發行者
                //{ "aud", "GoBetGoalApi" }, // 接收者
                { "jti", Guid.NewGuid().ToString() }, // 讓每個 Token 本身都獨一無二

                // --- 方便使用的自訂金鑰 ---
                { "Email", user.Email}
            };

            ////payload作法2
            //Dictionary<string, Object> claim = new Dictionary<string, Object>();//payload 需透過token傳遞的資料
            //claim.Add("Id", user.Id);
            //claim.Add("Account", user.Account);
            //claim.Add("Exp", DateTime.Now.AddSeconds(Convert.ToInt32("100")).ToString());//Token 時效設定100秒
            //var payload = claim;

            // 產生 JwtToken
            var token = JWT.Encode(payload, Encoding.UTF8.GetBytes(secretKey), JwsAlgorithm.HS512);
            return token;
        }

        /// <summary>
        /// 生成只刷新效期的 JwtToken
        /// </summary>
        /// <returns>JwtToken</returns>
        public string ExpRefreshToken(Dictionary<string, object> tokenData)
        {
            //string secretKey = WebConfigurationManager.AppSettings["TokenKey"];
            // payload 從原本 token 傳遞的資料沿用，並刷新效期
            var payload = new Dictionary<string, object>
            {
                // --- 核心資料從舊 Token 沿用 ---
                // 注意：從 Dictionary 取出的 Guid 和 Email 需要做型別轉換和檢查
                { "Id", Guid.Parse(tokenData["Id"].ToString()) },
                { "Email", tokenData["Account"].ToString() },

                // --- 標準金鑰需要重新產生或設定 ---
                { "iss", "GoBetGoalApi" }, // 維持發行者
                { "jti", Guid.NewGuid().ToString() }, // *** 產生一個全新的 JTI，代表這是一個新 Token ***
                { "Exp", DateTime.Now.AddMonths(3).ToString() } // *** 設定一個全新的過期時間 ***
            };

            //產生刷新時效的 JwtToken
            var token = JWT.Encode(payload, Encoding.UTF8.GetBytes(secretKey), JwsAlgorithm.HS512);
            return token;
        }

        /// <summary>
        /// 生成無效 JwtToken
        /// </summary>
        /// <returns>JwtToken</returns>
        public string RevokeToken()
        {
            string secretKeyNew = "RevokeToken"; // 故意用不同的 key 生成 // 不跟全域依樣
            var payload = new Dictionary<string, object>
            {
                // 欄位結構與 GenerateToken 保持一致，但給予無意義或作廢的數值
                { "Id", Guid.Empty },
                { "Exp", DateTime.Now.AddDays(-1).ToString() }, // 將過期時間設定在昨天
                { "iss", "GoBetGoalApi" },
                { "jti", Guid.NewGuid().ToString() },
                { "Email", "revoked@example.com" }
            };

            // 產生失效的 JwtToken
            var token = JWT.Encode(payload, Encoding.UTF8.GetBytes(secretKeyNew), JwsAlgorithm.HS512);
            return token;
        }

        /// <summary>
        /// 將 Token 解密取得夾帶的資料
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public Dictionary<string, object> GetPayload(string token)
        {
            try
            {
                return JWT.Decode<Dictionary<string, object>>(token, Encoding.UTF8.GetBytes(secretKey), JwsAlgorithm.HS512);

            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 【新增至此處】驗證 token 時效
        /// </summary>
        /// <param name="dateTimeString">來自 token payload 的 Exp 欄位字串</param>
        /// <returns>是否已過期</returns>
        public bool IsTokenExpired(string dateTimeString)
        {
            // 使用 TryParse 更安全，避免因格式錯誤導致伺服器崩潰
            if (DateTime.TryParse(dateTimeString, out DateTime expirationTime))
            {
                // 如果成功轉換，就比較時間
                return expirationTime < DateTime.Now;
            }

            // 如果連日期格式都解析不了，直接視為無效/過期
            return true;
        }
    }
}