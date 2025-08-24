using System;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace GoBetGoal_BackEnd.Security // 請確認您的命名空間
{
    /// <summary>
    /// 提供「可選式」的 JWT 驗證。
    /// - 如果請求未提供 Token，則視為匿名訪客。
    /// - 如果請求提供了有效的 Token，則驗證使用者身分。
    /// - 如果請求提供了無效/過期的 Token，則仍視為匿名訪客，且不回傳 401 錯誤。
    /// </summary>
    public class OptionalAuthorizeAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            var request = actionContext.Request;

            // 步驟 1：檢查 Header 中是否存在 Token
            if (request.Headers.Authorization == null || request.Headers.Authorization.Scheme != "Bearer" || string.IsNullOrEmpty(request.Headers.Authorization.Parameter))
            {
                // 【沒有 Token】：這是合法的訪客請求，我們什麼都不做，直接讓請求通過。
                return;
            }

            // 步驟 2：如果找到了 Token，就嘗試去驗證它
            try
            {
                var jwtAuthUtil = new JwtAuthUtility();
                var token = request.Headers.Authorization.Parameter;
                var payload = jwtAuthUtil.GetPayload(token);

                // 檢查 Token 是否解碼成功且未過期
                if (payload != null && !jwtAuthUtil.IsTokenExpired(payload["Exp"].ToString()))
                {
                    // 【Token 有效】：驗證成功！將 payload 存入 Properties，讓使用者以會員身分繼續。
                    actionContext.Request.Properties["jwtPayload"] = payload;
                }

                // 【Token 無效或過期】：如果 payload 是 null 或已過期，我們「故意什麼都不做」。
                // 請求會繼續往下走，但因為沒有設定 jwtPayload，使用者會被當成訪客。
            }
            catch (Exception)
            {
                // 【Token 解碼異常】：如果 Jose.JWT.Decode 拋出任何例外（例如 Token 格式嚴重錯誤），
                // 我們也「故意什麼都不做」，並將使用者視為訪客。
            }

            // 注意：這個方法自始至終都沒有呼叫 HandleUnauthorizedRequest，
            // 所以它永遠不會產生 401 錯誤。
        }
    }
}