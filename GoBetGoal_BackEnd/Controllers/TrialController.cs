using GoBetGoal_BackEnd.Models;
using GoBetGoal_BackEnd.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Data.Entity;
using GoBetGoal_BackEnd.Enums;
using System.Net;

namespace GoBetGoal_BackEnd.Controllers
{
    public class TrialController : BaseApiController
    {
        private readonly Context _db = new Context();

        [HttpGet]
        [Route("api/trial/details/{id}")]
        [AllowAnonymous] // *** 標記為公開，允許訪客存取 ***
        public IHttpActionResult GetTrialDetails(int id)
        {
            // 1) 嘗試取得目前檢視者（登入者）的 UserId，未登入則為 null（可用於日後個人化資料）
            Guid? viewerId = TryGetCurrentUserId();

            // 2. 先查詢指定的試煉
            var trial = _db.Trials
                .Include(t => t.TrialTemplate) // 順便帶出 Template
                .FirstOrDefault(t => t.Id == id);

            if (trial == null)
            {
                var error = new ErrorResponseDto
                {
                    ErrorCode = "TRIAL_NOT_FOUND",
                    Message = "指定的試煉不存在。"
                };
                return Content(HttpStatusCode.NotFound, error);
            }

            // 3. 把這個試煉的所有 Stage (模板 + UserStages) 查出來
            var stages = _db.Stages
                .Include(s => s.UserStages)
                .Where(s => s.TrialTemplateId == trial.TrialTemplateId)
                .OrderBy(s => s.StageIndex)   // <-- 明確排序
                .ToList();

            // 4. 撈出所有參與者 (狀態 = Accepted)
            var participants = _db.TrialParticipants
                .Include(tp => tp.Invitee.UserStages)
                .Include(tp => tp.Invitee.UserAvatars.Select(a => a.Avatar))
                .Where(tp => tp.TrialId == id && tp.Status == Status.accepted)
                .ToList();

            //var users = _db.TrialParticipants.Where(t => t.TrialId == id && t.Status == (Status)7).Include(t => t.Invitee).Include(t=>t.Invitee.UserStages.Where(y=>y.TrialId==id)).Select(t => t.Invitee).ToList();

            // 5. 組 TrialDetailDto (試煉本身資訊)
            var trialDetailDto = new TrialDetailDto();

            trialDetailDto.Id = trial.Id;
            trialDetailDto.TrialName = trial.TrialName;
            trialDetailDto.StartAt = trial.StartTime;
            trialDetailDto.EndAt = trial.EndTime;
            trialDetailDto.TrialStatus = trial.TrialStatus.ToString();
            trialDetailDto.Deposit = trial.TrialDeposit;
            trialDetailDto.CreatorId = trial.UserId;

            var trialTemplateDto = new TrialTemplateInfoDto();
            trialTemplateDto.Id = trial.TrialTemplateId;
            trialTemplateDto.TrialTitle = trial.TrialTemplate.TrialTitle;
            trialTemplateDto.TrialDescription = trial.TrialTemplate.TrialDescription;
            trialTemplateDto.TrialFrequency = trial.TrialTemplate.TrialFrequency;
            trialTemplateDto.TrialCategory = trial.TrialTemplate.TrialCategory;
            trialTemplateDto.TrialSuitFor = trial.TrialTemplate.TrialSuitFor;
            trialTemplateDto.TrialNoSuitFor = trial.TrialTemplate.TrialNoSuitFor;
            trialTemplateDto.TrialRule = trial.TrialTemplate.TrialRule;
            trialTemplateDto.TrialCaution = trial.TrialTemplate.TrialCaution;
            trialTemplateDto.TrialEffect = trial.TrialTemplate.TrialEffect;
            trialTemplateDto.StageCount = trial.TrialTemplate.StageCount;
            trialTemplateDto.MaxUser = trial.TrialTemplate.MaxUser;
            trialTemplateDto.IsAi = trial.TrialTemplate.IsAi;
            trialTemplateDto.TrialTemplatePrice = trial.TrialTemplate.TrialTemplatePrice;
            trialTemplateDto.CardImagePath = trial.TrialTemplate.CardImagePath;
            trialTemplateDto.CardColor = trial.TrialTemplate.CardColor;

            trialDetailDto.TrialTemplateInfo = trialTemplateDto;

            // 6. 組 Participants DTO
            var trialParticipantDtos = new List<TrialParticipantDto>();

            foreach (var participant in participants)
            {
                var user = participant.Invitee;

                var userProfileDto = new UserProfileDto();

                userProfileDto.UserId = user.Id;
                userProfileDto.Email = user.Email;
                userProfileDto.PlayerId = user.PlayerId;
                userProfileDto.NickName = user.NickName;
                userProfileDto.BagelCount = user.BagelCount;
                userProfileDto.CheatBlanketCount = user.CheatBlanketCount;
                userProfileDto.TotalTrialCount = _db.TrialParticipants
        .Count(tp => tp.InviteeId == user.Id && tp.Status == Status.accepted);
                userProfileDto.LikedPostsCount = _db.PostLikes
        .Count(like => like.Post.UserId == user.Id);
                userProfileDto.FriendCount = _db.FriendsRelationships
        .Count(fr => (fr.UserId == user.Id || fr.InviteeId == user.Id)
                  && fr.Status == Status.accepted);
                userProfileDto.CreatedAt = user.CreatedAt;
                userProfileDto.CurrentAvatarId = user.UserAvatars.Where(u => u.IsCurrent).Select(u => u.AvatarId).FirstOrDefault();
                userProfileDto.CurrentAvatarUrl = user.UserAvatars.Where(u => u.IsCurrent).Select(u => u.Avatar.AvatarImagePath).FirstOrDefault();



                DateTime joinedAt;
                if (participant.ParticipantId == participant.InviteeId)
                {
                    joinedAt = participant.InviteAt;
                }
                else
                {
                    joinedAt = participant.Status == Status.accepted ? (participant.UpdatedAt ?? participant.InviteAt) : participant.InviteAt;
                }

                var userStages = _db.UserStages.Where(x => x.UserId == user.Id && x.TrialId == trial.Id);

                var passCount = userStages.Count(x => x.Status == Status.pass || x.Status == Status.cheat);
                var cheatBlanketCount = userStages.Count(x => x.Status == Status.cheat);
                var failCount = userStages.Count(x => x.Status == Status.fail);



                var trialStageProgressDtoList = new List<TrialStageProgressDto>();

                foreach (var stage in stages)
                {
                    var userStage = _db.UserStages.FirstOrDefault(us => us.UserId == user.Id && us.StageId == stage.Id);

                    var trialStageProgressDto = new TrialStageProgressDto();
                    trialStageProgressDto.StageIndex = stage.StageIndex;
                    trialStageProgressDto.StageDescription = stage.StageDescription;
                    trialStageProgressDto.StageSampleImagePath = stage.StageSampleImagePath;

                    trialStageProgressDto.StartTime = userStage?.StartTime;
                    trialStageProgressDto.EndTime = userStage?.EndTime;
                    trialStageProgressDto.Status = userStage?.Status.ToString();
                    trialStageProgressDto.UploadImagePath = userStage?.UploadImagePath;
                    trialStageProgressDto.UploadAt = userStage != null ? userStage.ImageUploadAt : null;
                    trialStageProgressDto.ChanceRemain = userStage != null ? (int?)userStage.ChanceRemain : null;

                    trialStageProgressDtoList.Add(trialStageProgressDto);

                }

                string friendState = null;
                if (viewerId.HasValue)
                {
                    var relation = _db.FriendsRelationships.FirstOrDefault(f => (f.UserId == viewerId.Value && f.InviteeId == user.Id) || (f.UserId == user.Id && f.InviteeId == viewerId.Value));

                    friendState = relation != null ? relation.Status.ToString() : null;
                }



                var trialParticipantDto = new TrialParticipantDto();
                trialParticipantDto.UserInfo = userProfileDto;
                trialParticipantDto.JoinedAt = joinedAt;
                trialParticipantDto.PassCount = passCount;
                trialParticipantDto.CheatBlanketCount = cheatBlanketCount;
                trialParticipantDto.FailCount = failCount;
                trialParticipantDto.Stages = trialStageProgressDtoList;
                trialParticipantDto.FriendState = friendState;

                trialParticipantDtos.Add(trialParticipantDto);

            }


            //    執行排序邏輯
            //    排序：創建者第一，其他人依 JoinAt
            var creator = trialParticipantDtos.FirstOrDefault(p => p.UserInfo.UserId == trial.UserId);
            var sortedParticipants = trialParticipantDtos.Where(p => p.UserInfo.UserId != trial.UserId).OrderBy(p => p.JoinedAt).ToList();
            if (creator != null)
            {
                sortedParticipants.Insert(0, creator);
            }

            trialDetailDto.Participants = sortedParticipants;

            //圍觀

            var trialLikes = _db.TrialLikes
    .Include(x => x.User.UserAvatars.Select(y => y.Avatar)) // 連同使用者和頭像一起抓
    .Where(x => x.TrialId == id)  // 篩選出指定試煉的喜歡
    .OrderBy(x => x.CreatedAt)    // 按時間排序
    .ToList();                     // 轉成 List，方便後續操作

            var trialLikeDtos = trialLikes.Select(x => new TrialLikeDto
            {
                UserId = x.UserId,
                Email = x.User.Email,
                PlayerId = x.User.PlayerId,
                NickName = x.User.NickName,
                BagelCount = x.User.BagelCount,
                CheatBlanketCount = x.User.CheatBlanketCount,

                // 取 IsCurrent = true 的頭像，如果沒有就 null
                CurrentAvatarUrl = x.User.UserAvatars
        .Where(a => a.IsCurrent)   // 篩選出目前使用的頭像
        .Select(a => a.Avatar.AvatarImagePath) // 取頭像的 Url
        .FirstOrDefault()          // 如果沒有就回傳 null
            }).ToList();

            trialDetailDto.TrialLikes = trialLikeDtos;


            return Ok(trialDetailDto);
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