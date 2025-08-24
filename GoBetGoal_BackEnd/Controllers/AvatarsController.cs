using GoBetGoal_BackEnd.Models;
using GoBetGoal_BackEnd.Models.DTOs;
using GoBetGoal_BackEnd.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace GoBetGoal_BackEnd.Controllers
{
    public class AvatarsController : BaseApiController
    {
        private readonly Context _db = new Context();

        [HttpGet]
        [Route("api/avatars")]
        [JwtAuthFilter]
        public IHttpActionResult GetAllAvatars()
        {
            Guid currentUserId = GetCurrentUserId();

            // 為了提高效能，先一次性地查出這位使用者「已經擁有」的所有頭像的 ID 並存入 HashSet，這樣後續比對時速度會非常快。
            var userOwnedAvatarIds = new HashSet<int>(_db.UserAvatars.Where(a => a.UserId == currentUserId).Select(a => a.AvatarId));

            var avatarDtos = _db.Avatars
                // 1. 先篩選出所有「上架中」的頭像
                .Where(a => a.IsActive)
                // 2. 使用 .Select() 將 Avatar 物件轉換成我們設計好的 AvatarDto
                .OrderBy(a=>a.SortOrder)
                .Select(a => new AvatarDto
                {
                    AvatarId=a.Id,
                    SortOrder = a.SortOrder,
                    AvatarImagePath = a.AvatarImagePath,
                    AvatarPrice = a.AvatarPrice,
                    IsLocked = a.AvatarPrice > 0,
                    IsUnlocked=userOwnedAvatarIds.Contains(a.Id) || a.AvatarPrice==0

                }).ToList();

            return Ok(avatarDtos);
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