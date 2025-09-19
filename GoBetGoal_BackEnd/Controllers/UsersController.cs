using GoBetGoal_BackEnd.Enums;
using GoBetGoal_BackEnd.Models;
using GoBetGoal_BackEnd.Models.DTOs;
using GoBetGoal_BackEnd.Security;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Data.Entity;

namespace GoBetGoal_BackEnd.Controllers
{
    public class UsersController : BaseApiController
    {
        private readonly Context _db = new Context();
        /// <summary>
        /// 註冊第二步：初次設定個人資料
        /// </summary>

        [HttpPost]
        [Route("api/users/me/profile")]
        public IHttpActionResult CompleteUserProfile(RegisterStepTwoRequestDto model)
        {
            Guid currentUserId = GetCurrentUserId();

            //if (!ModelState.IsValid)
            //{
            //    return BadRequest(ModelState);
            //}

            bool IsNickNameTaken = _db.Users.Any(u => u.NickName == model.NickName);
            if (IsNickNameTaken)
            {
                var error = new ErrorResponseDto
                {
                    ErrorCode = "NICKNAME_ALREADY_EXISTS",
                    Message = "此暱稱已被使用，請嘗試使用其他暱稱註冊。"
                };

                return Content(HttpStatusCode.Conflict, error);

            }

            var selectedAvatar = _db.Avatars.FirstOrDefault(a => a.Id == model.AvatarId && a.IsActive);
            if (selectedAvatar == null)
            {
                var error = new ErrorResponseDto
                {
                    ErrorCode = "AVATAR_NOT_FOUND",
                    Message = "指定的頭像不存在。"
                };

                return Content(HttpStatusCode.BadRequest, error);
            }

            if (selectedAvatar.AvatarPrice > 0)
            {
                bool userOwnsThisAvatar = _db.UserAvatars.Any(a => a.UserId == currentUserId && a.AvatarId == model.AvatarId);

                if (!userOwnsThisAvatar)
                {
                    var error = new ErrorResponseDto
                    {
                        ErrorCode = "AVATAR_NOT_OWNED",
                        Message = "無法選擇尚未擁有的付費頭像。"
                    };

                    return Content(HttpStatusCode.Forbidden, error);

                }
            }

            var userToUpdate = _db.Users.Find(currentUserId);
            if (userToUpdate == null)
            {
                var error = new ErrorResponseDto
                {
                    ErrorCode = "USER_NOT_FOUND",
                    Message = "指定的使用者不存在。"
                };
                return Content(HttpStatusCode.NotFound, error);
            }

            userToUpdate.NickName = model.NickName;
            userToUpdate.UpdatedAt = DateTime.Now;
            userToUpdate.BagelCount += 10000;

            // 2. 更新 UserAvatar 表
            //    a. 先將使用者目前所有的頭像都設為非當前 (IsCurrent = false)
            var allUserAvatars = _db.UserAvatars.Where(ua => ua.UserId == currentUserId);
            foreach (var avatarEntry in allUserAvatars)
            {
                avatarEntry.IsCurrent = false;
            }

            //    b. 接著，從 allUserAvatars 中找到使用者「新選擇」的那一筆
            var chosenAvatarEntry = allUserAvatars.FirstOrDefault(ua => ua.AvatarId == model.AvatarId);

            //    c. 將它設為當前 (IsCurrent = true)
            //       (我們在前面的驗證已確保 chosenAvatarEntry 不會是 null，所以這裡可以直接設定)
            if (chosenAvatarEntry != null)
            {
                chosenAvatarEntry.IsCurrent = true;
            }
            else
            {
                // 這是一個保險措施，理論上前段驗證不會讓使用者選到他沒有的頭像
                // 但如果真的發生了，回傳一個錯誤
                var error = new ErrorResponseDto { ErrorCode = "AVATAR_NOT_OWNED", Message = "無法選擇尚未擁有的付費頭像。" };
                return Content(HttpStatusCode.BadRequest, error);
            }



            try
            {
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }

            var successResponse = new SuccessResponseDto { Message = "個人資料建立成功！恭喜您獲得 10,000 貝果獎勵！" };
            return Ok(successResponse);
        }


        [HttpPut]
        [Route("api/users/me/profile")]
        public IHttpActionResult UpdateUserProfile(UpdateProfileRequestDto model)
        {
            Guid currentUserId = GetCurrentUserId();

            var userToUpdate = _db.Users.Find(currentUserId);
            if (userToUpdate == null)
            {
                var error = new ErrorResponseDto
                {
                    ErrorCode = "USER_NOT_FOUND",
                    Message = "指定的使用者不存在。"
                };
                return Content(HttpStatusCode.NotFound, error);
            }

            bool IsNickNameTaken = _db.Users.Any(u => u.NickName == model.NickName && u.Id != currentUserId);
            if (IsNickNameTaken)
            {
                var error = new ErrorResponseDto
                {
                    ErrorCode = "NICKNAME_ALREADY_EXISTS",
                    Message = "此暱稱已被使用，請嘗試使用其他暱稱註冊。"
                };

                return Content(HttpStatusCode.Conflict, error);
            }

            userToUpdate.NickName = model.NickName;
            userToUpdate.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);
            userToUpdate.UpdatedAt = DateTime.Now;


            try
            {
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }

            var successResponse = new SuccessResponseDto { Message = "個人資料已成功更新！" };
            return Ok(successResponse);

        }

        [HttpPut]
        [Route("api/users/me/avatar")]
        public IHttpActionResult UpdateUserAvatar(UpdateAvatarRequestDto model)
        {
            Guid currentUserId = GetCurrentUserId();

            var userToUpdate = _db.Users.Find(currentUserId);
            if (userToUpdate == null)
            {
                var error = new ErrorResponseDto
                {
                    ErrorCode = "USER_NOT_FOUND",
                    Message = "指定的使用者不存在。"
                };
                return Content(HttpStatusCode.NotFound, error);
            }

            var selectedAvatar = _db.Avatars.FirstOrDefault(a => a.Id == model.AvatarId && a.IsActive);
            if (selectedAvatar == null)
            {
                var error = new ErrorResponseDto
                {
                    ErrorCode = "AVATAR_NOT_FOUND",
                    Message = "指定的頭像不存在。"
                };

                return Content(HttpStatusCode.BadRequest, error);
            }

            if (selectedAvatar.AvatarPrice > 0)
            {
                bool userOwnsThisAvatar = _db.UserAvatars.Any(a => a.UserId == currentUserId && a.AvatarId == model.AvatarId);

                if (!userOwnsThisAvatar)
                {
                    var error = new ErrorResponseDto
                    {
                        ErrorCode = "AVATAR_NOT_OWNED",
                        Message = "無法選擇尚未擁有的付費頭像。"
                    };

                    return Content(HttpStatusCode.Forbidden, error);

                }
            }

            // 找出使用者目前所有的頭像關聯紀錄
            var allUserAvatars = _db.UserAvatars.Where(ua => ua.UserId == currentUserId).ToList();

            // 將舊的「當前頭像」標記為 false
            var oldCurrentAvatar = allUserAvatars.FirstOrDefault(ua => ua.IsCurrent);
            if (oldCurrentAvatar != null)
            {
                oldCurrentAvatar.IsCurrent = false;
            }

            // 將新選擇的頭像標記為 true
            var newCurrentAvatar = allUserAvatars.FirstOrDefault(ua => ua.AvatarId == model.AvatarId);
            // 理論上 newCurrentAvatar 不會是 null，因為我們在步驟 1 已經檢查過了
            if (newCurrentAvatar != null)
            {
                newCurrentAvatar.IsCurrent = true;
            }

            // 儲存所有變更 (EF 會將這兩個 Update 操作包在一個交易中)
            try
            {
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }

            return Ok(new SuccessResponseDto { Message = "頭像已成功更新！" });

        }




        // 這支 API 放在我們之前建立的 UsersController.cs 中

        [HttpGet]
        [Route("api/users/me")]
        public IHttpActionResult GetMyProfile()
        {
            // 步驟 1：安全地取得當前登入者的 ID
            Guid currentUserId = GetCurrentUserId();

            // 步驟 2：從資料庫撈出使用者核心資料
            // 我們使用 .Include() 來確保相關的頭像資料也被一併取出
            var user = _db.Users
                  .Include(u => u.UserAvatars.Select(ua => ua.Avatar))
                  .FirstOrDefault(u => u.Id == currentUserId);

            // 處理極端情況：Token 有效，但資料庫中已找不到該使用者
            if (user == null)
            {
                var error = new ErrorResponseDto
                {
                    ErrorCode = "USER_NOT_FOUND",
                    Message = "指定的使用者不存在。"
                };
                return Content(HttpStatusCode.NotFound, error);
            }

            // 步驟 3：執行額外的計算查詢
            // 這樣將每個計算分開，查詢語句更簡單、更高效
            int totalTrialCount = _db.TrialParticipants.Count(tp => tp.InviteeId == currentUserId && tp.Status == Status.accepted);
            int likedPostsCount = _db.PostLikes.Count(like => like.Post.UserId == currentUserId);
            int friendCount = _db.FriendsRelationships.Count(fr => (fr.UserId == currentUserId || fr.InviteeId == currentUserId) && fr.Status == Status.accepted);
            var purchaseChallengeIds = _db.UserTrialTemplates.Where(x => x.UserId == currentUserId).Select(x => x.TrialTemplateId).ToList();
            var purchaseAvatarIds = _db.UserAvatars.Where(x => x.UserId == currentUserId).Select(x => x.AvatarId).ToList();

            // 步驟 4：在記憶體中組合最終的 DTO 物件
            var userProfile = new UserProfileDto
            {
                UserId = user.Id,
                NickName = user.NickName,
                BagelCount = user.BagelCount,
                CheatBlanketCount = user.CheatBlanketCount,
                CurrentAvatarUrl = user.UserAvatars.FirstOrDefault(ua => ua.IsCurrent)?.Avatar.AvatarImagePath,

                // 填入剛剛計算好的欄位
                TotalTrialCount = totalTrialCount,
                LikedPostsCount = likedPostsCount,
                FriendCount = friendCount
               

                // 注意：這裡不需要 FriendState，因為使用者看自己的個人檔案，這個欄位沒有意義
            };

            return Ok(userProfile);
        }


        [HttpGet]
        [Route("api/users/{userId}")] // api/users/{id}
        [AllowAnonymous] // 允許訪客和會員存取
        public IHttpActionResult GetUserProfile(Guid userId)
        {
            // 步驟 1：溫和地取得當前檢視者的 ID (訪客則為 null)
            Guid? viewerId = TryGetCurrentUserId();

            // 步驟 2：從資料庫撈出目標使用者，並同時載入相關的頭像資料
            // 我們使用最相容的字串路徑 .Include()
            var targetUser = _db.Users
                 .Include(u => u.UserAvatars.Select(ua => ua.Avatar))
                 .FirstOrDefault(u => u.Id == userId);


            // 步驟 3：如果找不到使用者，回傳 404 Not Found
            if (targetUser == null)
            {
                var error = new ErrorResponseDto
                {
                    ErrorCode = "USER_NOT_FOUND",
                    Message = "指定的使用者不存在。"
                };
                return Content(HttpStatusCode.NotFound, error);
            }

            // 步驟 4：呼叫輔助方法來建立並填充回傳給前端的 DTO
            var userProfileDto = CreateUserProfileDto(targetUser, viewerId);

            return Ok(userProfileDto);
        }

        // 檔案路徑: Controllers/UsersController.cs

        [HttpGet]
        [Route("api/users/all")] // 路由設定為 /api/users
        [AllowAnonymous] // 允許訪客和會員存取
        public IHttpActionResult GetAllUserProfiles()
        {
            // 步驟 1：溫和地取得當前檢視者的 ID (訪客則為 null)
            Guid? viewerId = TryGetCurrentUserId();

            // 步驟 2：從資料庫一次性撈出所有使用者，並包含他們各自的頭像資料
            var allUsers = _db.Users
                              .Include("UserAvatars.Avatar")
                              .ToList();

            // 步驟 3：【效能優化關鍵】
            // 如果是會員登入，我們先跑一趟資料庫，把「我」所有的好友關係一次性撈出來放到一個 List (清單) 中。
            List<FriendsRelationship> viewerFriendships = new List<FriendsRelationship>();
            if (viewerId.HasValue)
            {
                viewerFriendships = _db.FriendsRelationships
                                       .Where(f => f.UserId == viewerId.Value || f.InviteeId == viewerId.Value)
                                       .ToList();
            }

            // 步驟 4：遍歷所有使用者，並呼叫共用的輔助方法來建立 DTO
            // 我們將預先載入的好友關係清單 (viewerFriendships) 傳入輔助方法，
            // 這樣在計算 friend_state 時，就不用每次都去查詢資料庫了。
            var userProfiles = allUsers.Select(user => CreateUserProfileDto(user, viewerId, viewerFriendships)).ToList();

            // 步驟 5：回傳組合好的使用者列表
            return Ok(userProfiles);
        }

        /// <summary>
        /// 輔助方法：根據目標使用者和檢視者，建立一個 UserProfileDto
        /// </summary>
        private UserProfileDto CreateUserProfileDto(User targetUser, Guid? viewerId, List<FriendsRelationship> preloadedFriendships = null)
        {
            var dto = new UserProfileDto
            {
                UserId = targetUser.Id,
                NickName = targetUser.NickName,
                BagelCount = targetUser.BagelCount,
                CheatBlanketCount = targetUser.CheatBlanketCount,
                CurrentAvatarUrl = targetUser.UserAvatars.FirstOrDefault(ua => ua.IsCurrent)?.Avatar.AvatarImagePath,

                // 計算欄位 (分開查詢以確保效能和正確性)
                TotalTrialCount = _db.TrialParticipants.Count(tp => tp.InviteeId == targetUser.Id && tp.Status == Status.accepted),
                LikedPostsCount = _db.PostLikes.Count(like => like.Post.UserId == targetUser.Id),
                FriendCount = _db.FriendsRelationships.Count(fr => (fr.UserId == targetUser.Id || fr.InviteeId == targetUser.Id) && fr.Status == Status.accepted),              

                // 計算好友狀態
                FriendState = GetFriendState(viewerId, targetUser.Id, preloadedFriendships)
            };

            return dto;
        }

        /// <summary>
        /// 輔助方法：計算檢視者與目標使用者的好友關係
        /// </summary>
        private string GetFriendState(Guid? viewerId, Guid targetUserId, List<FriendsRelationship> preloadedFriendships)
        {
            if (!viewerId.HasValue)
            {
                return null; // 訪客模式，沒有好友狀態
            }

            if (viewerId.Value == targetUserId)
            {
                return "self"; // 是使用者本人
            }

            FriendsRelationship relation;

            if (preloadedFriendships != null)
            {
                // (供 GetAllUserProfiles 使用) 從預先載入的列表中尋找，效能較好
                relation = preloadedFriendships.FirstOrDefault(f =>
                    (f.UserId == viewerId.Value && f.InviteeId == targetUserId) ||
                    (f.UserId == targetUserId && f.InviteeId == viewerId.Value));
            }
            else
            {
                // (供 GetUserProfile 使用) 直接查詢資料庫
                relation = _db.FriendsRelationships.FirstOrDefault(f =>
                    (f.UserId == viewerId.Value && f.InviteeId == targetUserId) ||
                    (f.UserId == targetUserId && f.InviteeId == viewerId.Value));
            }

            if (relation != null)
            {
                return relation.Status.ToString().ToLower(); // "pending", "accepted", "rejected"
            }

            return "not_friends"; // 沒有任何關係紀錄
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