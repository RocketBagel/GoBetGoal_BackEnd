using GoBetGoal_BackEnd.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace GoBetGoal_BackEnd.Filters
{
    public class ValidateModelAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            // ★★★ 核心修正 ★★★
            // 檢查請求的方法。如果是 GET 或 DELETE，我們就直接跳過所有驗證。
            var method = actionContext.Request.Method;
            if (method == HttpMethod.Get || method == HttpMethod.Delete)
            {
                base.OnActionExecuting(actionContext);
                return;
            }

            // 情況一：Body 為空或格式錯誤，導致 model 為 null
            if (actionContext.ActionArguments.Any(kv => kv.Value == null))
            {
                // 使用您定義好的 ErrorResponseDto
                var errorResponse = new ErrorResponseDto
                {
                    // 給開發者看的機器可讀代碼
                    ErrorCode = "INVALID_REQUEST_BODY",
                    // 給前端直接顯示給使用者的通用訊息
                    Message = "提供的資料格式有誤或為空，請重新操作。"
                };

                actionContext.Response = actionContext.Request.CreateResponse(
                    HttpStatusCode.BadRequest, errorResponse);

                return;
            }

            // 情況二：欄位資料不符合規則 (例如 [Required] 驗證失敗)
            if (actionContext.ModelState.IsValid == false)
            {
                // 取得詳細的欄位錯誤
                var errors = actionContext.ModelState
                    .Where(ms => ms.Value.Errors.Any())
                    .Select(ms => new
                    {
                        Field = ms.Key.Contains('.') ? ms.Key.Split('.').Last() : ms.Key,
                        Errors = ms.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    });

                // 建立一個包含詳細資訊的、結構一致的回應
                var responsePayload = new
                {
                    // 遵循一致的結構
                    ErrorCode = "VALIDATION_FAILED",
                    Message = "您輸入的資料有誤，請檢查後再試。",
                    // 額外提供給前端，用來顯示在各別欄位的詳細錯誤
                    Details = errors
                };

                actionContext.Response = actionContext.Request.CreateResponse(
                    HttpStatusCode.BadRequest, responsePayload);
            }

        }
    }
}