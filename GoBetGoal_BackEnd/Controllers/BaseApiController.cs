using GoBetGoal_BackEnd.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace GoBetGoal_BackEnd.Controllers
{
    // 讓這個基底類別繼承自 ApiController
    public abstract class BaseApiController : ApiController
    {
        /// <summary>
        /// 從 JWT Payload 中取得當前登入者的 User ID。
        /// 如果取得失敗，會直接拋出一個 401 Unauthorized 錯誤。
        /// </summary>
        /// <returns>當前使用者的 Guid</returns>
        protected Guid GetCurrentUserId()
        {
            // 將我們之前寫的、繁瑣但安全的邏輯，全部搬到這裡
            if (Request.Properties.TryGetValue("jwtPayload", out object payloadObject))
            {
                var payload = payloadObject as Dictionary<string, object>;
                if (payload != null && payload.ContainsKey("Id"))
                {
                    string userIdString = payload["Id"].ToString();
                    if (Guid.TryParse(userIdString, out Guid currentUserId))
                    {
                        // 成功取得，回傳 Guid
                        return currentUserId;
                    }
                }
            }

            // 1. 建立一個標準的錯誤回應 DTO
            var error = new ErrorResponseDto
            {
                ErrorCode = "TOKEN_INVALID",
                Message = "連線階段無效或已過期，請重新登入。"
            };

            // 2. 建立一個包含此 DTO 的 HttpResponseMessage 401
            var response = Request.CreateResponse(HttpStatusCode.Unauthorized, error);

            // 3. 拋出包含此詳細回應的例外
            throw new HttpResponseException(response);
        }

        /// <summary>
        /// 【溫和模式】「嘗試」取得當前登入者的 User ID。
        /// 這個方法適用於允許「訪客」和「已登入會員」同時存取的 API。
        /// </summary>
        /// <returns>如果使用者已登入且 Token 有效，則回傳使用者的 Guid；否則回傳 null。</returns>
        protected Guid? TryGetCurrentUserId()
        {
            try
            {
                // 直接呼叫上面那個「嚴格模式」的方法
                return GetCurrentUserId();
            }
            catch (HttpResponseException)
            {
                // 如果 GetCurrentUserId() 因為 Token 無效或不存在而拋出 401 例外，
                // 我們就捕捉這個例外，並安靜地回傳 null。
                // 這代表「好的，我知道這個使用者是訪客」。
                return null;
            }
        }
    }
}