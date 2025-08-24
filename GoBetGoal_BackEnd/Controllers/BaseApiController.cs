using GoBetGoal_BackEnd.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace GoBetGoal_BackEnd.Controllers
{
    // 讓這個基底類別繼承自 ApiController
    public abstract class BaseApiController : ApiController
    {
        /// <summary>
        /// 【核心邏輯】從請求屬性中解析 User ID，這個方法本身不拋出任何錯誤。
        /// 這是內部使用的方法。
        /// </summary>
        /// <returns>成功則回傳 Guid，失敗或找不到則回傳 null。</returns>
        private Guid? GetUserIdFromRequest()
        {
            // 檢查 "jwtPayload" 是否存在於請求的屬性中
            if (Request.Properties.TryGetValue("jwtPayload", out object payloadObject))
            {
                // 確認 payload 是我們預期的字典格式，且包含 "Id" 這個 key
                if (payloadObject is Dictionary<string, object> payload && payload.ContainsKey("Id"))
                {
                    // 嘗試將 Id 轉換成 Guid
                    if (Guid.TryParse(payload["Id"].ToString(), out Guid currentUserId))
                    {
                        // 一切順利，回傳解析出的 Guid
                        return currentUserId;
                    }
                }
            }

            // 上述任何一個環節失敗，都安全地回傳 null
            return null;
        }

        /// <summary>
        /// 【嚴格模式】「必須」取得當前登入者的 User ID。
        /// 這個方法供需要權限的 API (例如掛上 [Authorize] 的) 使用。
        /// 找不到有效的 ID 時，會拋出 401 Unauthorized 錯誤。
        /// </summary>
        /// <returns>當前使用者的 Guid</returns>
        protected Guid GetCurrentUserId()
        {
            // 呼叫核心邏輯
            Guid? userId = GetUserIdFromRequest();

            // 檢查是否有成功取得 ID
            if (userId.HasValue)
            {
                // 有，就回傳
                return userId.Value;
            }

            // 如果 userId 是 null，就建立並拋出我們自訂的 401 錯誤
            var error = new ErrorResponseDto
            {
                ErrorCode = "TOKEN_MISSING_OR_INVALID_FORMAT", // 您可以自訂錯誤碼
                Message = "連線階段無效或已過期，請重新登入。"
            };
            var response = Request.CreateResponse(HttpStatusCode.Unauthorized, error);
            throw new HttpResponseException(response);
        }

        /// <summary>
        /// 【溫和模式】「嘗試」取得當前登入者的 User ID。
        /// 這個方法供允許訪客的 API (例如掛上 [OptionalAuthorize] 的) 使用。
        /// 找不到有效的 ID 時，會安全地回傳 null，不會拋出任何錯誤。
        /// </summary>
        /// <returns>如果使用者已登入且 Token 有效，則回傳使用者的 Guid；否則回傳 null。</returns>
        protected Guid? TryGetCurrentUserId()
        {
            // 直接回傳核心邏輯的結果即可。
            // 因為 GetUserIdFromRequest() 永遠不會拋錯，所以這裡也不需要 try-catch。
            return GetUserIdFromRequest();
        }
    }
}