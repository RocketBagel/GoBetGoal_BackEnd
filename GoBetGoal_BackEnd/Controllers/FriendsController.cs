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

        //取得當前使用者全部好友
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

                // 試煉參與總數
                int totalTrialCount = _context.TrialParticipants
                    .Count(ttc => ttc.ParticipantId == friend.Id || ttc.InviteeId == friend.Id);

                // 成功試煉總數
                int successTrialCount = _context.TrialParticipants.Count(stc => (stc.Trial.TrialStatus == Status.perfect||stc.Trial.TrialStatus == Status.pass) && (stc.ParticipantId == friend.Id || stc.InviteeId == friend.Id));

                // 發文總數
                int totalpostCount = _context.Posts
                    .Count(p => p.UserId == friend.Id);

                var friendInfoDto = new FriendInfoDto
                {
                    Id = friend.Id,
                    NickName = friend.NickName,
                    AvatarUrl = currentAvatar?.Avatar?.AvatarImagePath,
                    TotalTrialCount = totalTrialCount,
                    SuccessTrialCount = successTrialCount,
                    TotalPostCount = totalpostCount
                };

                friendDtos.Add(friendInfoDto);
            }

            return Ok(friendDtos);
        }


        //新增好友邀請
        [HttpPost]
        [Route("api/friends/invite")]
        public IHttpActionResult InviteFriend([FromBody] InviteFriendDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

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

            //被邀請人是否存在
            if (!Guid.TryParse(request.InviteeId, out Guid inviteeGuid))
            {
                return BadRequest("InviteeId 格式錯誤。");
            }

            var invitee = _context.Users.Find(inviteeGuid);
            if (invitee == null)
            {
                return Content(HttpStatusCode.NotFound, new ErrorResponseDto
                {
                    ErrorCode = "INVITEE_NOT_FOUND",
                    Message = "被邀請者不存在。"
                });
            }

            //是否重複邀請 (或已經是好友)
            bool alreadyExists = _context.FriendsRelationships.Any(fr =>
                (fr.UserId == currentUserId && fr.InviteeId == inviteeGuid) ||
                (fr.UserId == inviteeGuid && fr.InviteeId == currentUserId));

            if (alreadyExists) // 409
            {
                return Content(HttpStatusCode.Conflict, new ErrorResponseDto
                {
                    ErrorCode = "FRIEND_INVITE_ALREADY_EXISTS",
                    //Message = "好友邀請已存在或已經是好友。"
                }); 
            }

            var newFriendInvitation = new FriendsRelationship
            {
                UserId = currentUserId,
                InviteeId = inviteeGuid,
                Note = request.Note,
                Status = Status.pending,
                InviteAt = DateTime.Now,

            };

            _context.FriendsRelationships.Add(newFriendInvitation);
            _context.SaveChanges();


            return Ok("已發出邀請");

        }


        //同意好友邀請(會員中心)
        [HttpPatch]
        [Route("api/friends/invitation/{inviteId}")]
        public IHttpActionResult AgreeFriendInvitation(int inviteId)
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

            var invitation = _context.FriendsRelationships.Find(inviteId);
            if (invitation == null)
            {
                return Content(HttpStatusCode.NotFound, new ErrorResponseDto
                {
                    ErrorCode = "INVITATION_NOT_FOUND",
                    Message = "邀請不存在。"
                });
            }

            // 是否為受邀者
            if (invitation.InviteeId != currentUserId)
            {
                return Content(HttpStatusCode.Forbidden, new ErrorResponseDto
                {
                    ErrorCode = "NOT_INVITEE",
                    Message = "非受邀者"
                });
            }

            // 檢查狀態
            if (invitation.Status != Status.pending)
            {
                return Content(HttpStatusCode.BadRequest, new ErrorResponseDto
                {
                    ErrorCode = "INVALID_STATUS",
                    Message = "邀請已處理，無法重複同意"
                });
            }
            
            // 更新邀請狀態
            invitation.Status = Status.accepted;
            invitation.UpdatedAt = DateTime.UtcNow;


            // 建立通知資料(發送邀請者)
            var notifySender = new Notification
            {
                ReceiverId = invitation.UserId,   // 發送邀請者
                SenderId = currentUserId,         // 來源是受邀者
                NotificationType = NotificationType.friend_request_accept,
                Content = $"{user.NickName} 已接受你的好友邀請。",
                ReferenceId_Int = invitation.Id,  // 關聯到邀請紀錄
                CreatedAt = DateTime.Now
            };

            // 建立通知資料(受邀者)
            var notifyInvitee = new Notification
            {
                ReceiverId = invitation.InviteeId,    // 受邀者
                SenderId = invitation.UserId,         // 來源是發送邀請者
                NotificationType = NotificationType.friend_request_accept,
                Content = $"你已成功與 {invitation.User.NickName} 成為好友。",
                ReferenceId_Int = invitation.Id,
                CreatedAt = DateTime.Now
            };
            
            _context.Notifications.Add(notifySender);
            _context.Notifications.Add(notifyInvitee);
            _context.SaveChanges();




            return Ok("Agree");
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