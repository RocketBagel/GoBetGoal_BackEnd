using GoBetGoal_BackEnd.Enums;
using GoBetGoal_BackEnd.Models;
using GoBetGoal_BackEnd.Models.DTOs;
using GoBetGoal_BackEnd.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;

namespace GoBetGoal_BackEnd.Controllers
{
    public class AvatarsController : BaseApiController
    {
        private readonly Context _db = new Context();

        [HttpGet]
        [Route("api/avatars")]
        public IHttpActionResult GetAllAvatars()
        {
            Guid currentUserId = GetCurrentUserId();

            // 為了提高效能，先一次性地查出這位使用者「已經擁有」的所有頭像的 ID 並存入 HashSet，這樣後續比對時速度會非常快。
            var userOwnedAvatarIds = new HashSet<int>(_db.UserAvatars.Where(a => a.UserId == currentUserId).Select(a => a.AvatarId));

            var avatarDtos = _db.Avatars
                // 1. 先篩選出所有「上架中」的頭像
                .Where(a => a.IsActive)
                // 2. 使用 .Select() 將 Avatar 物件轉換成我們設計好的 AvatarDto
                .OrderBy(a => a.SortOrder)
                .Select(a => new AvatarDto
                {
                    AvatarId = a.Id,
                    SortOrder = a.SortOrder,
                    AvatarImagePath = a.AvatarImagePath,
                    AvatarPrice = a.AvatarPrice,
                    IsLocked = a.AvatarPrice > 0,
                    IsUnlocked = userOwnedAvatarIds.Contains(a.Id) || a.AvatarPrice == 0

                }).ToList();

            return Ok(avatarDtos);
        }

        /// <summary>
        /// 購買指定的頭像
        /// </summary>
        /// <param name="id">要購買的頭像 ID</param>
        [HttpPost]
        [Route("api/avatars/{avatarIdInput}/purchase")]
        public IHttpActionResult PurchaseAvatar(string avatarIdInput)
        {
            int avatarId;
            if (!int.TryParse(avatarIdInput, out avatarId))
            {
                // 如果傳入的 string 無法被成功轉換為 int
                // 就回傳一個我們自訂的 400 Bad Request 錯誤
                var error = new ErrorResponseDto
                {
                    ErrorCode = "AVATAR_NOT_FOUND",
                    Message = "指定的頭像不存在。"
                };
                return Content(HttpStatusCode.BadRequest, error);
            }

            // 步驟 1: 取得當前使用者 ID 並找出使用者
            Guid currentUserId = GetCurrentUserId();
            var user = _db.Users.Find(currentUserId);
            if (user == null)
            {
                var error = new ErrorResponseDto
                {
                    ErrorCode = "USER_NOT_FOUND",
                    Message = "指定的使用者不存在。"
                };
                return Content(HttpStatusCode.NotFound, error);
            }

            // 步驟 2: 找出使用者想購買的頭像
            var avatarToPurchase = _db.Avatars.FirstOrDefault(a => a.Id == avatarId && a.IsActive);
            if (avatarToPurchase == null)
            {
                return Content(HttpStatusCode.NotFound, new ErrorResponseDto
                {
                    ErrorCode = "AVATAR_NOT_FOUND",
                    Message = "指定的頭像不存在。"
                });
            }

            // --- 步驟 3: 執行業務邏輯驗證 ---

            // a. 檢查頭像是否為免費
            //if (avatarToPurchase.AvatarPrice <= 0)
            //{
            //    return Content(HttpStatusCode.BadRequest, new ErrorResponseDto { ErrorCode = "AVATAR_IS_FREE", Message = "此頭像為免費，無需購買。" });
            //}

            // b. 檢查使用者是否已經擁有此頭像
            bool alreadyOwned = _db.UserAvatars.Any(ua => ua.UserId == currentUserId && ua.AvatarId == avatarId);
            if (alreadyOwned)
            {
                return Content(HttpStatusCode.Conflict, new ErrorResponseDto { ErrorCode = "AVATAR_ALREADY_OWNED", Message = "您已經擁有此頭像。" });
            }

            // c. 檢查使用者貝果餘額是否足夠
            if (user.BagelCount < avatarToPurchase.AvatarPrice)
            {
                return Content(HttpStatusCode.BadRequest, new ErrorResponseDto { ErrorCode = "INSUFFICIENT_BAGELS", Message = "您的貝果數量不足。" });
            }

            // --- 步驟 4: 執行核心操作 (Transaction) ---

            // a.記錄交易前的餘額
            int balanceBefore = user.BagelCount;

            // b. 扣除使用者的貝果
            user.BagelCount -= avatarToPurchase.AvatarPrice;

            // c. 記錄交易後的餘額
            int balanceAfter = user.BagelCount;


            // b. 在 UserAvatar 表中新增一筆擁有權紀錄
            var newOwnership = new UserAvatar
            {
                UserId = currentUserId,
                AvatarId = avatarId,
                IsCurrent = false /// 購買後預設不是「當前使用」
                //AcquiredAt = DateTime.UtcNow
            };
            _db.UserAvatars.Add(newOwnership);

            // e. *** 新增：建立 BagelTransaction 交易紀錄 ***
            var transaction = new BagelTransaction
            {
                UserId = currentUserId,
                TransactionType = TransactionType.花費解鎖, // 交易類型：購買
                ProductType = ProductType.Avatar,         // 商品類型：頭像
                ReferenceId = avatarToPurchase.Id,          // 關聯的商品 ID (頭像 ID)
                ItemName = $"頭像-{avatarToPurchase.Id}", // 交易項目名稱
                Price = avatarToPurchase.AvatarPrice,       // 商品單價
                Quantity = 1,                               // 購買數量
                Amount = -avatarToPurchase.AvatarPrice,     // 交易金額 (支出為負數)
                BalanceBefore = balanceBefore,              // 交易前餘額
                BalanceAfter = balanceAfter                // 交易後餘額
               /* CreatedAt = DateTime.UtcNow  */               // 交易時間
            };
            _db.BagelTransactions.Add(transaction); // 將交易紀錄加入 DbContext

            try
            {
                // c. 將對 User 和 UserAvatar 的變更，一次性存入資料庫
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }

            // 步驟 5: 回傳成功的結果
            var successResponse = new PurchaseSuccessDto
            {
                Message = $"已成功購買頭像-{avatarToPurchase.Id}！",
                RemainingBagelCount = user.BagelCount // 回傳更新後的餘額
            };

            return Ok(successResponse);
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