using GoBetGoal_BackEnd.Enums;
using GoBetGoal_BackEnd.Models;
using GoBetGoal_BackEnd.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;

namespace GoBetGoal_BackEnd.Controllers
{
    public class FriendsController : BaseApiController
    {
        private readonly Context _context = new Context();

        //讀取全部好友
        [HttpGet]
        [Route("api/friends")]
        public IHttpActionResult GetAllFriends()
        {
            //取得當前使用者 ID 
            Guid currentUserId = GetCurrentUserId();
            var user = _context.Users.Find(currentUserId);
            if (user == null)
            {
                var error = new ErrorResponseDto
                {
                    ErrorCode = "USER_NOT_FOUND",
                    Message = "指定的使用者不存在。"
                };
                return Content(HttpStatusCode.NotFound, error);
            }

            // 撈好友關係
            var friendRelationships = _context.FriendsRelationships.Where(f => f.Status == Status.accepted && (f.UserId == currentUserId || f.InviteeId == currentUserId)).ToList();

            if (!friendRelationships.Any())
            {
                var error = new ErrorResponseDto
                {
                    ErrorCode = "NO_FRIENDS",
                    Message = "尚無好友資訊"
                };
                return Content(HttpStatusCode.NotFound, error);
            }

            var friendDtos = new List<FriendInfoDto>();

            foreach (var relation in friendRelationships)
            {
                // 判斷當前使用者是邀請者還是受邀者
                var friend = relation.UserId == currentUserId ? relation.Invitee : relation.User;

                // 找出目前使用的頭像
                var currentAvatar = friend.UserAvatars.FirstOrDefault(ua => ua.IsCurrent);

                // 試煉參與數
                int trialCount = _context.TrialParticipants
                    .Count(tp => tp.ParticipantId == friend.Id || tp.InviteeId == friend.Id);

                // 發文數
                int postCount = _context.Posts
                    .Count(p => p.UserId == friend.Id);

                var friendInfoDto = new FriendInfoDto
                {
                    Id = friend.Id,
                    NickName = friend.NickName,
                    AvatarUrl = currentAvatar?.Avatar?.AvatarImagePath,
                    TrialCount = trialCount,
                    PostCount = postCount
                };

                friendDtos.Add(friendInfoDto);
            }

            return Ok(friendDtos);
        }


        protected override void Dispose(bool disposing)
        {

            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }
    
    }
}