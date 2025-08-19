using GoBetGoal_BackEnd.Models;
using GoBetGoal_BackEnd.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Data.Entity;

namespace GoBetGoal_BackEnd.Controllers
{
    public class TrialController : BaseApiController
    {
        private readonly Context _db = new Context();

        [HttpGet]
        [Route("api/trials/{id}")]
        [AllowAnonymous] // *** 標記為公開，允許訪客存取 ***
        public IHttpActionResult GetTrialDetails(Guid id)
        {
            // 1. 嘗試取得當前「檢視者」的 ID。如果是訪客，viewerId 會是 null
            Guid? viewerId = TryGetCurrentUserId();

            // 2. 進行一次複雜的資料庫查詢，撈取所有需要的資料
            //    使用 .Include() 來避免 N+1 查詢問題，提升效能
            var trial = _db.Trials
                .Include(t => t.TrialTemplate.Stages) // 載入試煉模板及其所有關卡
                .Include(t => t.TrialParticipants.Select(p => p.User.UserAvatars.Select(ua => ua.Avatar))) // 載入參與者的使用者、頭像關聯、頭像本身
                .Include(t => t.TrialParticipants.Select(p => p.User.UserStages)) // 載入參與者的所有關卡進度
                .FirstOrDefault(t => t.Id == id);

            if (trial == null)
            {
                return NotFound();
            }

            // 3. 將從資料庫撈出的 Entity 物件，手動轉換為 DTO
            var trialDetailDto = new TrialDetailDto
            {
                Id = trial.Id,
                Title = trial.Title,
                StartAt = trial.StartAt,
                EndAt = trial.EndAt,
                TrialStatus = trial.TrialStatus, // 不確定
                Deposit = trial.Deposit,
                CreatorId = trial.CreateBy,
                TrialTemplateInfo = new TrialTemplateInfoDto
                {
                    Id=trial.TrialTemplate.Id,
                    TrialTitle = trial.TrialTemplate.Title,
                    TrialDescription=trial.TrialTemplate.Description,
                    TrialFrequency= trial.TrialTemplate.Frequency,
                    TrialCategory=trial.TrialTemplate.Category,
                    TrialSuitFor=trial.TrialTemplate.SuitFor,
                    TrialNoSuitFor= trial.TrialTemplate.NoSuitFor,
                    TrialRule= trial.TrialTemplate.Rule,
                    TrialCaution= trial.TrialTemplate.Caution,
                    TrialEffect= trial.TrialTemplate.Effect,
                    StageCount= trial.TrialTemplate.StageCount,
                    MaxUser= trial.TrialTemplate.MaxUser,
                    IsAi= trial.TrialTemplate.IsAi,
                    TrialTemplatePrice= trial.TrialTemplate.Price,
                    CardImagePath= trial.TrialTemplate.ImagePath,
                    CardColor= trial.TrialTemplate.Color
                },
                Participants = trial.TrialParticipants.Select(p => new TrialParticipantDto
                {
                    UserInfo = new PublicUserProfileDto
                    {
                        UserId = p.UserId,
                        Email= p.Email,
                        PlayerId= p.PlayerId,
                        NickName = p.User.NickName,
                        CurrentAvatarId= p.User.CurrentAvatarId,
                        CurrentAvatarUrl= p.User.CurrentAvatarUrl
                    },
                    JoinedAt = p.JoinedAt,
                    Stages = p.User.UserStages
                        .Where(us => us.TrialId == trial.Id) // 只篩選出屬於本次試煉的關卡進度
                        .Select(us => new TrialStageProgressDto
                        {
                            StageIndex=us.UserStage.StageIndex,
                            Status=us.UserStage.Status,
                            UploadImageUrls=us.UserStage.UploadImageUrls,
                            UploadAt=us.UserStage.UploadAt,
                            ChanceRemain=us.UserStage.ChanceRemain, // 目前預設三次 是否要改成允許空值
                        }).ToList(),

                    // --- 個人化邏輯 ---
                    // 如果 viewerId 有值 (使用者已登入)，才去計算好友狀態
                    //FriendState = viewerId.HasValue
                    //    ? _db.Friends.FirstOrDefault(f => (f.User1Id == viewerId.Value && f.User2Id == p.UserId) || (f.User1Id == p.UserId && f.User2Id == viewerId.Value))?.Status
                    //    : null
                }).ToList()
            };

            // 4. 執行排序邏輯
            //    將創建者排到第一位
            var creator = trialDetailDto.Participants.FirstOrDefault(p => p.UserInfo.UserId == trial.CreateBy);
            if (creator != null)
            {
                trialDetailDto.Participants.Remove(creator);
                trialDetailDto.Participants.Insert(0, creator);
            }
            //    (這裡可以再接著寫其他參與者的排序邏輯)

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