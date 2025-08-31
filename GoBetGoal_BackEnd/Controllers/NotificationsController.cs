using GoBetGoal_BackEnd.Models;
using GoBetGoal_BackEnd.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;

namespace GoBetGoal_BackEnd.Controllers
{
    public class NotificationsController : BaseApiController
    {
        private readonly Context _db = new Context();

        /// <summary>
        /// 將單筆通知標記為已讀
        /// </summary>
        /// <param name="id">要標記為已讀的通知 ID</param>
        [HttpPut]
        [Route("api/notifications/{notificationId}/read")]
        public IHttpActionResult MarkAsRead(int notificationId)
        {
            // 1. 取得當前使用者 ID (此 API 需要驗證)
            Guid currentUserId = GetCurrentUserId();

            // 2. 找出指定的通知
            var notification = _db.Notifications.Find(notificationId);

            // --- 3. 業務邏輯驗證 ---

            // a. 檢查通知是否存在
            if (notification == null)
            {
                return Content(HttpStatusCode.NotFound, new ErrorResponseDto { ErrorCode = "NOTIFICATION_NOT_FOUND", Message = "找不到指定的通知。" });
            }

            // b. (極其重要的安全檢查) 確認是「接收者」本人在操作
            //    這可以防止使用者 A 去標記使用者 B 的通知為已讀
            if (notification.ReceiverId != currentUserId)
            {
                return Content(HttpStatusCode.Forbidden, new ErrorResponseDto { ErrorCode = "ACCESS_DENIED", Message = "您沒有權限修改此通知。" });
            }

            // c. (可選) 檢查是否已經是已讀狀態，避免不必要的資料庫寫入
            if (notification.IsRead)
            {
                // 如果已經是已讀，直接回傳成功即可
                return Ok(new SuccessResponseDto { Message = "通知已為已讀狀態。" });
            }


            // --- 4. 執行核心操作 ---

            // 將 IsRead 狀態更新為 true
            notification.IsRead = true;
            notification.UpdatedAt = DateTime.Now; // 記錄更新時間

            try
            {
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex); // 交給 GlobalExceptionFilter 處理
            }

            // 5. 回傳成功的結果
            return Ok(new SuccessResponseDto { Message = "通知已成功標記為已讀。" });
        }

        // 釋放資料庫連線資源
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}