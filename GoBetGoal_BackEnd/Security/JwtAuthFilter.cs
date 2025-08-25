using GoBetGoal_BackEnd.Models.DTOs;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace GoBetGoal_BackEnd.Security
{
    public class JwtAuthFilter : AuthorizeAttribute
    {
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            // 檢查方法上是否有 [AllowAnonymous] 標籤
            bool isAnonymousAllowed = actionContext.ActionDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().Any();

            var request = actionContext.Request;

            // 嘗試從 Header 取得 Token
            var token = (request.Headers.Authorization != null && request.Headers.Authorization.Scheme == "Bearer")
                ? request.Headers.Authorization.Parameter
                : null;

            // 如果 Token 為空
            if (string.IsNullOrEmpty(token))
            {
                // 如果這個方法允許匿名，就直接放行
                if (isAnonymousAllowed)
                {
                    return;
                }
                // 否則，回傳 401 錯誤
                HandleUnauthorizedRequest(actionContext);
                return;
            }

            // 如果 Token 存在，無論如何都嘗試解析
            try
            {
                var jwtAuthUtil = new JwtAuthUtility();
                var payload = jwtAuthUtil.GetPayload(token);

                // 檢查 Token 是否解碼成功且未過期
                if (payload != null && !jwtAuthUtil.IsTokenExpired(payload["Exp"].ToString()))
                {
                    // Token 有效，設定好 payload，讓 Controller 可以使用
                    actionContext.Request.Properties["jwtPayload"] = payload;
                    return; // 驗證成功，放行
                }

                // Token 無效或過期
                // 如果這個方法允許匿名，就當作訪客放行
                if (isAnonymousAllowed)
                {
                    return;
                }
                // 否則，回傳 401 錯誤
                HandleUnauthorizedRequest(actionContext);
                return;
            }
            catch (Exception)
            {
                // 解碼過程發生任何例外
                // 如果這個方法允許匿名，就當作訪客放行
                if (isAnonymousAllowed)
                {
                    return;
                }
                // 否則，回傳 401 錯誤
                HandleUnauthorizedRequest(actionContext);
                return;
            }
        }

        // 統一的 401 錯誤回應
        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            var error = new ErrorResponseDto { ErrorCode = "UNAUTHORIZED", Message = "連線階段無效或已過期，請重新登入。" };
            actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized, error);
        }
    }
}