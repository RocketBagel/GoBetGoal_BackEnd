using GoBetGoal_BackEnd.Models.DTOs;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace GoBetGoal_BackEnd.Security
{
    /// <summary>
    /// 處理 JWT 授權的核心過濾器，繼承自 AuthorizeAttribute 以融入 Web API 標準管線。
    /// </summary>
    public class JwtAuthFilter : AuthorizeAttribute
    {
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            // 檢查是否有 [AllowAnonymous] 標籤，如果有就直接放行
            bool isAnonymousAllowed = actionContext.ActionDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().Any() ||
                                      actionContext.ControllerContext.ControllerDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().Any();

            if (isAnonymousAllowed)
            {
                return;
            }

            var request = actionContext.Request;

            // 檢查 Token 是否存在且格式正確
            if (request.Headers.Authorization == null || request.Headers.Authorization.Scheme != "Bearer" || string.IsNullOrEmpty(request.Headers.Authorization.Parameter))
            {
                // 失敗，呼叫統一的處理方法
                HandleUnauthorizedRequest(actionContext);
                return;
            }

            try
            {
                var jwtAuthUtil = new JwtAuthUtility();
                var payload = jwtAuthUtil.GetPayload(request.Headers.Authorization.Parameter);

                // 檢查 Token 是否解碼成功且未過期
                if (payload == null || jwtAuthUtil.IsTokenExpired(payload["Exp"].ToString()))
                {
                    // 失敗，呼叫統一的處理方法
                    HandleUnauthorizedRequest(actionContext);
                    return;
                }

                // 驗證成功，將 payload 存入 Properties 供 Controller 使用
                actionContext.Request.Properties["jwtPayload"] = payload;
            }
            catch (Exception)
            {
                // 發生任何未預期的解碼錯誤，也視為授權失敗
                HandleUnauthorizedRequest(actionContext);
                return;
            }
        }

        /// <summary>
        /// 處理所有授權失敗情況的統一方法。
        /// </summary>
        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            var error = new ErrorResponseDto
            {
                // 對外只提供一個通用的錯誤碼
                ErrorCode = "UNAUTHORIZED",
                // 對外只提供一個統一的、引導使用者操作的訊息
                Message = "連線階段無效或已過期，請重新登入。"
            };
            actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized, error);
        }
    }
}