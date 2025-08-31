using GoBetGoal_BackEnd.Enums;
using GoBetGoal_BackEnd.Models;
using GoBetGoal_BackEnd.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Data.Entity;

namespace GoBetGoal_BackEnd.Controllers
{
    public class NotificationsController : BaseApiController
    {
        private readonly Context _db = new Context();

        /// <summary>
        /// 獲取通知中心的訊息列表
        /// </summary>
        [HttpGet]
        [Route("api/notifications")]
        public IHttpActionResult GetNotifications()
        {
            // 1. 取得當前使用者 ID (此 API 需要驗證)
            Guid currentUserId = GetCurrentUserId();

            // 2. 一次性從資料庫撈出這位使用者所有的通知
            //    使用 .Include() 預先載入發送者(Sender)的資料，避免 N+1 查詢
            var allNotifications = _db.Notifications
                .Include(n => n.Sender.UserAvatars.Select(ua => ua.Avatar))
                .Where(n => n.ReceiverId == currentUserId)
                .OrderByDescending(n => n.CreatedAt) // 統一先做排序
                .ToList();

            // 3. 在記憶體中，將通知分類並轉換成 DTO
            var result = new NotificationCenterDto
            {
                // a. 篩選出「公告」類型的通知，並取前 10 筆
                Announcements = allNotifications
                    .Where(n => n.NotificationType == NotificationType.announcement)
                    .Take(10)
                    .Select(n => MapToDto(n)) // 使用輔助方法進行轉換
                    .ToList(),

                // b. 篩選出「非公告」且「未讀」的通知，並取前 10 筆
                Unread = allNotifications
                    .Where(n => n.NotificationType != NotificationType.announcement && !n.IsRead)
                    .Take(10)
                    .Select(n => MapToDto(n))
                    .ToList(),

                // c. 篩選出「非公告」且「已讀」的通知，並取前 10 筆
                Read = allNotifications
                    .Where(n => n.NotificationType != NotificationType.announcement && n.IsRead)
                    .Take(10)
                    .Select(n => MapToDto(n))
                    .ToList()
            };

            return Ok(result);
        }

        // --- 建立一個私有的輔助方法，專門用來將 Notification Model 轉換為 DTO ---
        private NotificationDto MapToDto(Notification notification)
        {
            return new NotificationDto
            {
                Id = notification.Id,
                // 如果 Sender 是 null (例如系統公告)，SenderInfo 也會是 null
                SenderInfo = notification.Sender == null ? null : new PublicUserProfileDto
                {
                    UserId = notification.Sender.Id,
                    NickName = notification.Sender.NickName,
                    CurrentAvatarUrl = notification.Sender.UserAvatars.FirstOrDefault(ua => ua.IsCurrent)?.Avatar?.AvatarImagePath
                },
                NotificationType = notification.NotificationType,
                Content = notification.Content,
                IsRead = notification.IsRead,
                ReferenceId_Int = notification.ReferenceId_Int,
                ReferenceId_Guid = notification.ReferenceId_Guid,
                CreatedAt = notification.CreatedAt
            };
        }


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