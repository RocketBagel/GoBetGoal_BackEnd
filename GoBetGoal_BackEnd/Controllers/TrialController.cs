using CloudinaryDotNet.Actions;
using GoBetGoal_BackEnd.Enums;
using GoBetGoal_BackEnd.Models;
using GoBetGoal_BackEnd.Models.DTOs;
using GoBetGoal_BackEnd.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace GoBetGoal_BackEnd.Controllers
{
    public class TrialController : BaseApiController
    {
        private readonly Context _db = new Context();

        [HttpGet]
        [Route("api/trial/details/{trialIdInput}")]
        [AllowAnonymous] // *** 標記為公開，允許訪客存取 ***
        public IHttpActionResult GetTrialDetails(string trialIdInput)
        {
            int trialId;
            if (!int.TryParse(trialIdInput, out trialId))
            {
                var error = new ErrorResponseDto
                {
                    ErrorCode = "TRIAL_NOT_FOUND",
                    Message = "指定的試煉不存在。"
                };
                return Content(HttpStatusCode.BadRequest, error);
            }
            // 1) 嘗試取得目前檢視者（登入者）的 UserId，未登入則為 null（可用於日後個人化資料）
            Guid? viewerId = TryGetCurrentUserId();

            // 2. 先查詢指定的試煉
            var trial = _db.Trials
                .Include(t => t.TrialTemplate) // 順便帶出 Template
                .FirstOrDefault(t => t.Id == trialId);

            if (trial == null)
            {
                var error = new ErrorResponseDto
                {
                    ErrorCode = "TRIAL_NOT_FOUND",
                    Message = "指定的試煉不存在。"
                };
                return Content(HttpStatusCode.BadRequest, error);
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
                .Where(tp => tp.TrialId == trialId && tp.Status == Status.accepted)
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
            trialDetailDto.CreatedAt = trial.CreatedAt;

            var trialTemplateDto = new TrialTemplateInfoDto();
            trialTemplateDto.Id = trial.TrialTemplateId;
            trialTemplateDto.TrialTitle = trial.TrialTemplate.TrialTitle;
            trialTemplateDto.TrialDescription = trial.TrialTemplate.TrialDescription;
            trialTemplateDto.TrialFrequency = trial.TrialTemplate.TrialFrequency;
            trialTemplateDto.TrialCategory = JsonConvert.DeserializeObject<List<string>>(trial.TrialTemplate.TrialCategory ?? "[]");
            trialTemplateDto.TrialSuitFor = JsonConvert.DeserializeObject<List<string>>(trial.TrialTemplate.TrialSuitFor ?? "[]");
            trialTemplateDto.TrialNoSuitFor = JsonConvert.DeserializeObject<List<string>>(trial.TrialTemplate.TrialNoSuitFor ?? "[]");
            trialTemplateDto.TrialRule = JsonConvert.DeserializeObject<List<string>>(trial.TrialTemplate.TrialRule ?? "[]");
            trialTemplateDto.TrialCaution = JsonConvert.DeserializeObject<List<string>>(trial.TrialTemplate.TrialCaution ?? "[]");
            trialTemplateDto.TrialEffect = JsonConvert.DeserializeObject<List<string>>(trial.TrialTemplate.TrialEffect ?? "[]");
            trialTemplateDto.StageCount = trial.TrialTemplate.StageCount;
            trialTemplateDto.MaxUser = trial.TrialTemplate.MaxUser;
            trialTemplateDto.IsAi = trial.TrialTemplate.IsAi;
            trialTemplateDto.TrialTemplatePrice = trial.TrialTemplate.TrialTemplatePrice;
            trialTemplateDto.CardImagePath = trial.TrialTemplate.CardImagePath;
            trialTemplateDto.CardColor = trial.TrialTemplate.CardColor;
            trialTemplateDto.AiType = trial.TrialTemplate.AiType;

            var trialStageDto = new List<TrialStageDto>();

            foreach (var stage in stages)
            {
                var newDto = new TrialStageDto();

                newDto.Id = stage.Id;
                newDto.StageIndex = stage.StageIndex;
                newDto.Description = JsonConvert.DeserializeObject<List<string>>(stage.StageDescription ?? "[]");
                newDto.SampleImage = JsonConvert.DeserializeObject<List<string>>(stage.StageSampleImagePath ?? "[]");

                trialStageDto.Add(newDto);
            }

            trialTemplateDto.ChallengeStages = trialStageDto;


            trialDetailDto.TrialTemplateInfo = trialTemplateDto;



            // 6. 組 Participants DTO
            var trialParticipantDtos = new List<TrialParticipantDto>();

            foreach (var participant in participants)
            {
                var user = participant.Invitee;

                // 1. 先計算出 friendState 的值
                string calculatedFriendState = null; // 預設為 null (訪客)

                if (viewerId.HasValue) // 先判斷是否有登入的檢視者
                {
                    if (viewerId.Value == user.Id) // 再判斷是不是在看自己
                    {
                        calculatedFriendState = "self";
                    }
                    else
                    {
                        // 是登入者，且在看別人，才去查好友關係
                        var relation = _db.FriendsRelationships.FirstOrDefault(
                            f => (f.UserId == viewerId.Value && f.InviteeId == user.Id) || (f.UserId == user.Id && f.InviteeId == viewerId.Value)
                        );

                        if (relation != null)
                        {
                            // 如果有關係紀錄，就用資料庫的狀態
                            calculatedFriendState = relation.Status.ToString().ToLower(); // 例如 "accepted", "pending"
                        }
                        else
                        {
                            // 如果沒有任何關係紀錄，狀態為 "not_friends"
                            calculatedFriendState = "not_friends";
                        }
                    }
                }

                var userProfileDto = new PublicUserProfileDtoV2();

                userProfileDto.UserId = user.Id;
                userProfileDto.NickName = user.NickName;
                userProfileDto.TotalTrialCount = _db.TrialParticipants
        .Count(tp => tp.InviteeId == user.Id && tp.Status == Status.accepted);
                userProfileDto.LikedPostsCount = _db.PostLikes
        .Count(like => like.Post.UserId == user.Id);
                userProfileDto.FriendCount = _db.FriendsRelationships
        .Count(fr => (fr.UserId == user.Id || fr.InviteeId == user.Id)
                  && fr.Status == Status.accepted);
                userProfileDto.CurrentAvatarUrl = user.UserAvatars.Where(u => u.IsCurrent).Select(u => u.Avatar.AvatarImagePath).FirstOrDefault();
                userProfileDto.FriendState = calculatedFriendState;



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

                    trialStageProgressDto.StageId = stage.Id;
                    trialStageProgressDto.StageIndex = stage.StageIndex;
                    trialStageProgressDto.StageDescription = JsonConvert.DeserializeObject<List<string>>(stage.StageDescription ?? "[]"); ;
                    trialStageProgressDto.StageSampleImagePath = JsonConvert.DeserializeObject<List<string>>(stage.StageSampleImagePath ?? "[]");

                    trialStageProgressDto.StartTime = userStage?.StartTime;
                    trialStageProgressDto.EndTime = userStage?.EndTime;
                    trialStageProgressDto.Status = userStage?.Status.ToString();
                    trialStageProgressDto.UploadImagePaths = !string.IsNullOrEmpty(userStage?.UploadImagePath)
            ? JsonConvert.DeserializeObject<List<string>>(userStage.UploadImagePath)
            : new List<string>(); // 如果是 null 或空字串，就給一個空的 List

                    trialStageProgressDto.UploadAt = userStage != null ? userStage.ImageUploadAt : null;
                    trialStageProgressDto.ChanceRemain = userStage != null ? (int?)userStage.ChanceRemain : null;

                    trialStageProgressDtoList.Add(trialStageProgressDto);

                }


                var trialParticipantDto = new TrialParticipantDto();
                trialParticipantDto.UserInfo = userProfileDto;
                trialParticipantDto.JoinedAt = joinedAt;
                trialParticipantDto.PassCount = passCount;
                trialParticipantDto.CheatBlanketCount = cheatBlanketCount;
                trialParticipantDto.FailCount = failCount;
                trialParticipantDto.Stages = trialStageProgressDtoList;

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
    .Where(x => x.TrialId == trialId)  // 篩選出指定試煉的喜歡
    .OrderBy(x => x.CreatedAt)    // 按時間排序
    .ToList();                     // 轉成 List，方便後續操作

            var trialLikeDtos = trialLikes.Select(x => new PublicUserProfileDto
            {
                UserId = x.UserId,
                NickName = x.User.NickName,
                // 取 IsCurrent = true 的頭像，如果沒有就 null
                CurrentAvatarUrl = x.User.UserAvatars
        .Where(a => a.IsCurrent)   // 篩選出目前使用的頭像
        .Select(a => a.Avatar.AvatarImagePath) // 取頭像的 Url
        .FirstOrDefault()          // 如果沒有就回傳 null
            }).ToList();

            trialDetailDto.TrialLikes = trialLikeDtos;


            return Ok(trialDetailDto);
        }

        [HttpPost]
        [Route("api/trial/{trialIdInput}/stage/{stageIdInput}/use-cheat-blanket")]
        public IHttpActionResult UseCheatBlanket(string trialIdInput, string stageIdInput)
        {
            int trialId;
            if (!int.TryParse(trialIdInput, out trialId))
            {
                var error = new ErrorResponseDto
                {
                    ErrorCode = "TRIAL_NOT_FOUND",
                    Message = "指定的試煉不存在。"
                };
                return Content(HttpStatusCode.BadRequest, error);
            }

            int stageId;
            if (!int.TryParse(stageIdInput, out stageId))
            {
                var error = new ErrorResponseDto
                {
                    ErrorCode = "STAGE_NOT_FOUND",
                    Message = "指定的關卡不存在。"
                };
                return Content(HttpStatusCode.BadRequest, error);
            }

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

            if (user.CheatBlanketCount <= 0)
            {
                var error = new ErrorResponseDto
                {
                    ErrorCode = "NO_CHEAT_BLANKET_AVAILABLE",
                    Message = "您的遮羞布數量不足。"
                };
                return Content(HttpStatusCode.NotFound, error);
            }

            // b. (可選但建議) 檢查使用者是否真的是這個試煉的參與者
            bool isParticipant = _db.TrialParticipants.Any(p => p.TrialId == trialId && p.InviteeId == currentUserId && p.Status == Status.accepted);
            if (!isParticipant)
            {
                var error = new ErrorResponseDto
                {
                    ErrorCode = "NOT_A_PARTICIPANT",
                    Message = "您並非此試煉的參與者。"
                };
                // 403 Forbidden 代表「你已登入，但無權對此資源操作」
                return Content(HttpStatusCode.Forbidden, error);
            }

            // b. 找出使用者在這個試煉、這個關卡的「進度紀錄 (UserStage)」
            var userStageToUpdate = _db.UserStages.FirstOrDefault(us =>
                us.TrialId == trialId &&
                us.UserId == currentUserId &&
                us.StageId == stageId
            );

            // c. 檢查這筆進度紀錄是否存在
            if (userStageToUpdate == null)
            {
                var error = new ErrorResponseDto
                {
                    ErrorCode = "STAGE_PROGRESS_NOT_FOUND",
                    Message = "找不到您在此關卡的進度紀錄。"
                };
                return Content(HttpStatusCode.NotFound, error);
            }

            // d. (可選但建議) 檢查這個關卡是否已經通關了，避免重複使用
            if (userStageToUpdate.Status == Status.pass || userStageToUpdate.Status == Status.cheat)
            {
                var error = new ErrorResponseDto
                {
                    ErrorCode = "STAGE_ALREADY_COMPLETED",
                    Message = "此關卡已通關，無法使用遮羞布。"
                };
                return Content(HttpStatusCode.BadRequest, error);
            }

            user.CheatBlanketCount--;


            // b. 更新 UserStage 的狀態
            userStageToUpdate.Status = Status.cheat;

            try
            {
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }

            // 5. 準備成功的回應 (不變)
            var successResponse = new UseCheatBlanketResponseDto
            {
                Message = "成功使用遮羞布！",
                RemainingCheatBlanketCount = user.CheatBlanketCount
            };

            return Ok(successResponse);
        }

        /// <summary>
        /// 加入一個試煉的圍觀列表 (喜歡這個試煉)
        /// </summary>
        /// <param name="id">要加入圍觀的試煉 ID</param>
        [HttpPost]
        [Route("api/trial/{trialIdInput}/toggle-like")]
        public IHttpActionResult ToggleLikeTrial(string trialIdInput)
        {
            int trialId;
            if (!int.TryParse(trialIdInput, out trialId))
            {
                var error = new ErrorResponseDto
                {
                    ErrorCode = "TRIAL_NOT_FOUND",
                    Message = "指定的試煉不存在。"
                };
                return Content(HttpStatusCode.BadRequest, error);
            }

            // 1. 取得當前使用者 ID (此 API 需要驗證)
            Guid currentUserId = GetCurrentUserId();

            // --- 2. 業務邏輯驗證 ---

            // a. 檢查試煉是否存在
            var trial = _db.Trials.Find(trialId);
            if (trial == null)
            {
                return Content(HttpStatusCode.BadRequest, new ErrorResponseDto { ErrorCode = "TRIAL_NOT_FOUND", Message = "指定的試煉不存在。" });
            }

            // b. 檢查使用者是否已經圍觀過此試煉，避免重複加入
            //bool alreadyLiked = _db.TrialLikes.Any(tl => tl.TrialId == trialId && tl.UserId == currentUserId);
            //if (alreadyLiked)
            //{
            //    // 使用 409 Conflict 表示這個操作與現有狀態衝突
            //    return Content(HttpStatusCode.Conflict, new ErrorResponseDto { ErrorCode = "ALREADY_LIKED", Message = "您已經在圍觀列表中了。" });
            //}

            // c. (可選但建議) 檢查使用者是否為參與者，參與者可能不能同時是圍觀者
            //bool isParticipant = _db.TrialParticipants.Any(p => p.TrialId == trialId && p.InviteeId == currentUserId && p.Status == Status.accepted);
            //if (isParticipant)
            //{
            //    return Content(HttpStatusCode.BadRequest, new ErrorResponseDto { ErrorCode = "PARTICIPANT_CANNOT_LIKE", Message = "您已是此試煉的參與者，無法加入圍觀。" });
            //}

            // 3. 尋找使用者是否已經「喜歡」過此試煉
            var existingLike = _db.TrialLikes.FirstOrDefault(tl => tl.TrialId == trialId && tl.UserId == currentUserId);

            bool isCurrentlyLiked;

            if (existingLike != null)
            {
                // 如果找到了紀錄，代表使用者要「取消喜歡」
                _db.TrialLikes.Remove(existingLike);
                isCurrentlyLiked = false; // 操作後的狀態是「不喜歡」
            }
            else
            {
                // 如果沒找到紀錄，代表使用者要「加入喜歡」
                var newLike = new TrialLike
                {
                    UserId = currentUserId,
                    TrialId = trialId,
                };
                _db.TrialLikes.Add(newLike);
                isCurrentlyLiked = true; // 操作後的狀態是「喜歡」
            }


            try
            {
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }

            // 4. 準備成功的回應
            //    在儲存後，重新計算一次總數，確保資料最新
            var newLikeCount = _db.TrialLikes.Count(tl => tl.TrialId == trialId);

            var successResponse = new ToggleLikeResponseDto
            {
                Message = isCurrentlyLiked ? "成功加入圍觀！" : "已取消圍觀。",
                IsLiked = isCurrentlyLiked, // 回傳最終狀態
                NewLikeCount = newLikeCount
            };

            return Ok(successResponse);
        }

        /// <summary>
        /// 為指定的試煉新增一筆社群分享貼文
        /// </summary>
        /// <param name="id">要分享的試煉 ID</param>
        /// <param name="model">包含心得和可選封面圖的資料</param>
        [HttpPost]
        [Route("api/trial/{trialIdInput}/share")]
        public IHttpActionResult ShareTrialPost(string trialIdInput, ShareTrialPostRequestDto model)
        {
            //// 1. 驗證 DTO 格式
            //if (!ModelState.IsValid)
            //{
            //    return BadRequest(ModelState);
            //}

            int trialId;
            if (!int.TryParse(trialIdInput, out trialId))
            {
                var error = new ErrorResponseDto
                {
                    ErrorCode = "TRIAL_NOT_FOUND",
                    Message = "指定的試煉不存在。"
                };
                return Content(HttpStatusCode.BadRequest, error);
            }


            Guid currentUserId = GetCurrentUserId();

            // --- 2. 業務邏輯驗證 ---

            var trial = _db.Trials.FirstOrDefault(t => t.Id == trialId);

            if (trial == null) { return Content(HttpStatusCode.BadRequest, new ErrorResponseDto { ErrorCode = "TRIAL_NOT_FOUND", Message = "指定的試煉不存在。" }); }


            // a. 檢查使用者是否為此試煉的參與者
            bool isParticipant = _db.TrialParticipants.Any(p => p.TrialId == trialId && p.InviteeId == currentUserId && p.Status == Status.accepted);
            if (!isParticipant)
            {
                var error = new ErrorResponseDto { ErrorCode = "NOT_A_PARTICIPANT", Message = "您並非此試煉的參與者，無法分享心得。" };
                return Content(HttpStatusCode.Forbidden, error);
            }

            // b. (可選) 檢查使用者是否已為此試煉分享過 (避免重複發文)
            //bool alreadyPosted = _db.Posts.Any(p => p.TrialId == id && p.UserId == currentUserId);
            //if (alreadyPosted)
            //{
            //    var error = new ErrorResponseDto { ErrorCode = "POST_ALREADY_EXISTS", Message = "您已經為此試煉分享過心得了。" };
            //    return Content(HttpStatusCode.Conflict, error);
            //}

            // --- 3. 核心操作：聚合所有圖片 ---

            // a. 建立一個列表來存放所有圖片 URL
            var allImageUrls = new List<string>();

            // b. 如果有提供封面圖，將它放在第一個
            if (!string.IsNullOrEmpty(model.CoverImageUrl))
            {
                allImageUrls.Add(model.CoverImageUrl);
            }

            // c. 找出使用者在此試煉所有關卡的進度紀錄
            var userStagesInTrial = _db.UserStages
                .Where(us => us.TrialId == trialId && us.UserId == currentUserId && us.UploadImagePath != null)
                .OrderBy(us => us.Stage.StageIndex) // 確保關卡圖片順序
                .ToList();

            // d. 遍歷所有關卡紀錄，將其中的圖片加入列表
            foreach (var userStage in userStagesInTrial)
            {
                // 將儲存的 JSON 字串反序列化成 List<string>
                var stageImages = JsonConvert.DeserializeObject<List<string>>(userStage.UploadImagePath);
                if (stageImages != null && stageImages.Any())
                {
                    // 將這個關卡的所有圖片，全部加入到我們的總列表中
                    allImageUrls.AddRange(stageImages);
                }
            }

            // --- 4. 建立新的 Post 物件 ---
            var newPost = new Post
            {
                Content = model.Content,
                UserId = currentUserId,
                TrialId = trialId,
                // 將聚合好的圖片列表，序列化成 JSON 字串存入資料庫
                ImageUrl = JsonConvert.SerializeObject(allImageUrls),
                //CreatedAt = DateTime.UtcNow
                // UserStageId 可以是 null，因為這篇貼文是關於整個試煉的總結
            };

            _db.Posts.Add(newPost);

            try
            {
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }

            return Ok(new SuccessResponseDto { Message = "已成功分享貼文至大平台！" });
        }

        /// <summary>
        /// 參加者在試煉開始前退出
        /// </summary>
        /// <param name="id">要退出的試煉 ID</param>
        [HttpDelete]
        [Route("api/trial/{trialIdInput}/participation")]
        public IHttpActionResult LeaveTrial(string trialIdInput)
        {

            int trialId;
            if (!int.TryParse(trialIdInput, out trialId))
            {
                var error = new ErrorResponseDto
                {
                    ErrorCode = "TRIAL_NOT_FOUND",
                    Message = "指定的試煉不存在。"
                };
                return Content(HttpStatusCode.BadRequest, error);
            }

            // 1. 取得當前使用者 ID (此 API 需要驗證)
            Guid currentUserId = GetCurrentUserId();

            // 2. 找出使用者在此試煉中的「參與紀錄」
            var participation = _db.TrialParticipants.FirstOrDefault(p => p.TrialId == trialId && p.InviteeId == currentUserId);

            // --- 3. 業務邏輯驗證 (Guard Clauses) ---

            // a. 檢查參與紀錄是否存在
            if (participation == null)
            {
                return Content(HttpStatusCode.NotFound, new ErrorResponseDto { ErrorCode = "PARTICIPATION_NOT_FOUND", Message = "您並未參加此試煉。" });
            }

            // b. 檢查試煉是否已開始 (最關鍵的業務規則)
            //    我們需要從 participation 關聯的 Trial 物件取得開始時間
            var trial = _db.Trials.Find(trialId);
            if (trial == null) { return Content(HttpStatusCode.BadRequest, new ErrorResponseDto { ErrorCode = "TRIAL_NOT_FOUND", Message = "指定的試煉不存在。" }); } // 理論上不會發生，但做個保險

            bool isCreatedToday = trial.CreatedAt.Date == DateTime.Now.Date;
            DateTime cutOffTime;

            if (isCreatedToday)
            {
                cutOffTime = trial.CreatedAt.Date.AddDays(1).AddSeconds(-1);
            }
            else
            {
                cutOffTime = trial.StartTime.Date.AddSeconds(-1);
            }


            if (DateTime.Now > cutOffTime)
            {
                return Content(HttpStatusCode.Forbidden, new ErrorResponseDto { ErrorCode = "TRIAL_ALREADY_STARTED", Message = "試煉已開始，無法退出。" });
            }

            // c. 找出使用者物件，準備退還押金
            var user = _db.Users.Find(currentUserId);
            if (user == null) { return Content(HttpStatusCode.NotFound, new ErrorResponseDto { ErrorCode = "USER_NOT_FOUND", Message = "指定的使用者不存在。" }); }


            // --- 4. 執行核心操作 (Transaction) ---

            // a. 記錄交易前的餘額
            int balanceBefore = user.BagelCount;

            // b. 將試煉押金退還給使用者
            user.BagelCount += trial.TrialDeposit;

            // c. 記錄交易後的餘額
            int balanceAfter = user.BagelCount;

            // d. 建立一筆 BagelTransaction 交易紀錄來「記帳」
            var transaction = new BagelTransaction
            {
                UserId = currentUserId,
                TransactionType = TransactionType.試煉押金,
                ProductType = ProductType.Bagel,   // 商品類型：試煉押金
                ReferenceId = trial.Id,                   // 關聯的試煉 ID
                ItemName = "退出試煉退還押金",
                Price = trial.TrialDeposit,               // 退款單價
                Quantity = 1,
                Amount = trial.TrialDeposit,              // 交易金額 (收入為正數)
                BalanceBefore = balanceBefore,
                BalanceAfter = balanceAfter
                //CreatedAt = DateTime.UtcNow
            };
            _db.BagelTransactions.Add(transaction);

            // e. 從 `TrialParticipants` 表中，移除這筆參與紀錄
            _db.TrialParticipants.Remove(participation);

            try
            {
                // f. 將所有變更 (更新 User、新增 BagelTransaction、刪除 TrialParticipant) 一次性存入資料庫
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }

            // 5. 回傳成功的結果
            var successResponse = new LeaveTrialResponseDto
            {
                Message = "成功退出試煉，貝果已退還。",
                NewBagelCount = user.BagelCount // 回傳更新後的餘額
            };

            return Ok(successResponse);
        }



        [HttpGet]
        [Route("api/trial/{inputTrialId}/results")]
        public IHttpActionResult GetMyTrialResults(string inputTrialId)
        {
            // --- *** 步驟二：在方法開頭，手動進行型別轉換與驗證 *** ---
            int trialId;
            if (!int.TryParse(inputTrialId, out trialId))
            {
                // 如果傳入的 string 無法被成功轉換為 int
                // 就回傳一個我們自訂的 400 Bad Request 錯誤
                var error = new ErrorResponseDto
                {
                    ErrorCode = "TRIAL_NOT_FOUND",
                    Message = "指定的試煉不存在。"
                };
                return Content(HttpStatusCode.BadRequest, error);
            }

            // 1. 取得並驗證使用者身份
            Guid currentUserId = GetCurrentUserId();

            // --- 步驟 A: 一次性從資料庫撈取所有需要的資料 ---

            // a. 查詢試煉本身，並預先載入模板和所有參與者
            var trial = _db.Trials
                .Include(t => t.TrialTemplate)
                .Include(t => t.TrialParticipants.Select(p => p.Invitee))
                .FirstOrDefault(t => t.Id == trialId);

            if (trial == null) { return Content(HttpStatusCode.BadRequest, new ErrorResponseDto { ErrorCode = "TRIAL_NOT_FOUND", Message = "指定的試煉不存在。" }); }

            // b. (安全檢查) 確認當前使用者是參與者之一
            var participants = trial.TrialParticipants.Where(p => p.Status == Status.accepted).ToList();
            if (!participants.Any(p => p.InviteeId == currentUserId))
            {
                return Content(HttpStatusCode.Forbidden, new ErrorResponseDto { ErrorCode = "NOT_A_PARTICIPANT", Message = "您並非此試煉的參與者。" });
            }

            // c. 撈出這個試煉所有的關卡進度 (UserStages)
            var allUserStagesInTrial = _db.UserStages
                .Where(us => us.TrialId == trialId)
                .ToList();

            // d. 撈出當前使用者在此試煉中獲得的成就
            var myAchievements = _db.UserAchievements
                .Include(ua => ua.Achievement)
                .Where(ua => ua.TrialId == trialId && ua.UserId == currentUserId)
                .Select(ua => ua.Achievement)
                .ToList();

            // --- 步驟 B: 在記憶體中進行計算與組裝 ---

            // a. 組合「排行榜」資料
            var leaderboardEntries = new List<LeaderboardEntryDto>();
            foreach (var p in participants)
            {
                var participantStages = allUserStagesInTrial.Where(us => us.UserId == p.InviteeId).ToList();
                var failCount = participantStages.Count(us => us.Status == Status.fail);
                var cheatCount = participantStages.Count(us => us.Status == Status.cheat);
                var completeCount = participantStages.Count(us => us.Status == Status.pass || us.Status == Status.cheat);

                leaderboardEntries.Add(new LeaderboardEntryDto
                {
                    UserInfo = new PublicUserProfileDto { UserId = p.InviteeId, NickName = p.Invitee.NickName, CurrentAvatarUrl = p.Invitee.UserAvatars.Where(a => a.IsCurrent == true).Select(a => a.Avatar.AvatarImagePath).FirstOrDefault() },
                    CompleteStageCount = completeCount,
                    CheatBlanketUsedCount = cheatCount,
                });
            }

            // b. 進行排序並賦予名次
            // b. 進行排序
            var sortedLeaderboard = leaderboardEntries
                .OrderByDescending(e => e.CompleteStageCount) // 規則一：失敗次數少的排前面
                .ToList();

            // --- *** 這是處理同名次的核心邏輯 *** ---

            int rank = 0;
            int lastCompleteStageCount = -1; // 儲存上一個人的失敗次數，-1 確保第一個人一定會被賦予名次

            for (int i = 0; i < sortedLeaderboard.Count; i++)
            {
                var currentEntry = sortedLeaderboard[i];

                // 檢查當前這位參賽者的分數，是否和上一位不同
                if (currentEntry.CompleteStageCount != lastCompleteStageCount)
                {
                    // 如果分數不同，就更新名次為當前的位置
                    rank = i + 1;
                }

                // 將計算好的名次，賦予給當前的參賽者
                currentEntry.Rank = rank;

                // 更新「上一位的分數」，為下一次迴圈做準備
                lastCompleteStageCount = currentEntry.CompleteStageCount;
            }
            // --- *** 邏輯結束 *** ---

            // c. 組合「我的個人結果」
            var myStages = allUserStagesInTrial.Where(us => us.UserId == currentUserId).ToList();
            var myCompleteCount = myStages.Count(us => us.Status == Status.pass || us.Status == Status.cheat);

            var myResult = new TrialResultDto
            {
                TrialInfo = new TrialResultInfoDto { TrialId = trial.Id, TrialCategory = JsonConvert.DeserializeObject<List<string>>(trial.TrialTemplate.TrialCategory), TrialName = trial.TrialName, TrialTitle = trial.TrialTemplate.TrialTitle, TrialDescription = trial.TrialTemplate.TrialDescription, TrialFrequency = trial.TrialTemplate.TrialFrequency, StageCount = trial.TrialTemplate.StageCount, TotalDays = trial.TrialTemplate.TrialFrequency * trial.TrialTemplate.StageCount, TotalParticipants = trial.TrialParticipants.Count(x => x.Status == Status.accepted), TrialStatus = trial.TrialStatus, EndTime = trial.EndTime },
                Leaderboard = sortedLeaderboard,
                MyResult = new MyPersonalResultDto
                {
                    MyUserInfo = _db.Users.Where(u => u.Id == currentUserId).Select(u => new PublicUserProfileDto { UserId = u.Id, NickName = u.NickName, CurrentAvatarUrl = u.UserAvatars.Where(a => a.IsCurrent == true).Select(a => a.Avatar.AvatarImagePath).FirstOrDefault() }).FirstOrDefault(), // 重新查詢以取得完整的 DTO
                    MyCompleteStageCount = myCompleteCount,
                    MyCheatBlanketUsedCount = myStages.Count(us => us.Status == Status.cheat),
                    MyAchievements = myAchievements.OrderBy(a => a.SortOrder).Select(a => new AchievementDto { Title = a.AchievementTitle, ImagePath = a.AchievementImagePath, SortOrder = a.SortOrder }).ToList(),
                    MyAllApprovedPhotos = myStages
                    .Where(us => us.UploadImagePath != null && (us.Status == Status.pass))
                    .SelectMany(us => JsonConvert.DeserializeObject<List<string>>(us.UploadImagePath))
                    .ToList(),
                    RewardAmount = _db.BagelTransactions.Where(x => x.UserId == currentUserId && x.TransactionType == TransactionType.試煉結算 && x.ProductType == ProductType.Bagel && x.ReferenceId == trial.Id).Select(x => x.Amount).FirstOrDefault()
                }
            };

            return Ok(myResult);
        }


        // 建立一個 CloudinaryService 的實體
        private readonly CloudinaryService _cloudinaryService = new CloudinaryService();
        // 建立一個 AiVerificationService 的實體
        private readonly AiVerificationService _aiService = new AiVerificationService();

        [HttpPost]
        [Route("api/trial/{trialIdInput}/stage/{stageIdInput}/submit-images")]
        public async Task<IHttpActionResult> SubmitStageImages(string trialIdInput, string stageIdInput)
        {
            int trialId;
            if (!int.TryParse(trialIdInput, out trialId))
            {
                var error = new ErrorResponseDto
                {
                    ErrorCode = "TRIAL_NOT_FOUND",
                    Message = "指定的試煉不存在。"
                };
                return Content(HttpStatusCode.BadRequest, error);
            }

            int stageId;
            if (!int.TryParse(stageIdInput, out stageId))
            {
                var error = new ErrorResponseDto
                {
                    ErrorCode = "STAGE_NOT_FOUND",
                    Message = "指定的關卡不存在。"
                };
                return Content(HttpStatusCode.BadRequest, error);
            }


            // --- 步驟 1: 驗證使用者身份 ---
            Guid currentUserId = GetCurrentUserId();

            // --- 步驟 1: 找出對應的 UserStage 紀錄 ---
            // 我們將這段邏輯提前，因為後續所有操作都需要它
            var userStage = _db.UserStages.FirstOrDefault(us =>
                us.TrialId == trialId &&
                us.UserId == currentUserId &&
                us.StageId == stageId);

            // 如果這是使用者第一次對此關卡進行操作，先建立一筆紀錄
            if (userStage == null)
            {
                var error = new ErrorResponseDto
                {
                    ErrorCode = "USERSTAGE_NOT_FOUND",
                    Message = "指定的使用者關卡不存在。"
                };
                return Content(HttpStatusCode.BadRequest, error);
            }

            // --- *** 步驟 2: 檢查剩餘次數 (核心修改點) *** ---
            if (userStage.ChanceRemain <= 0)
            {
                return Content(HttpStatusCode.Forbidden, new ErrorResponseDto
                {
                    ErrorCode = "NO_CHANCES_REMAINING",
                    Message = " AI 審核次數已用完。"
                });
            }

            // --- 步驟 2: 檢查請求的基本格式 ---
            // --- 【最佳實踐建議】新增一個 try-catch-finally 區塊來處理所有後續流程 ---
            // 這可以捕捉到 Cloudinary 或 AI 服務的突發錯誤，並確保資料庫操作的完整性
            List<string> allPublicIds = new List<string>(); // 將 publicIds 宣告在 try 區塊外，以便 catch 中也能存取
           
                // 檢查請求的 Content-Type 是否為 multipart/form-data，如果不是，代表前端傳送的格式根本不對

                if (!Request.Content.IsMimeMultipartContent())
            {
                // 回傳 415 Unsupported Media Type 錯誤，並使用我們自訂的 DTO
                // *** 修正 #1：使用 ErrorResponseDto ***
                var error = new ErrorResponseDto
                {
                    ErrorCode = "UNSUPPORTED_MEDIA_TYPE",
                    Message = "請求的內容格式不正確。"
                };
                return Content(HttpStatusCode.UnsupportedMediaType, error);
            }

            // --- 步驟 3: 接收前端傳來的所有檔案 ---

            // 建立一個提供者，它會將上傳的檔案暫存在伺服器的記憶體中
            var provider = new MultipartMemoryStreamProvider();
            // `await` 會在此處等待，直到所有檔案都接收完畢
            await Request.Content.ReadAsMultipartAsync(provider);

            // 接收完畢後，檢查是否有包含任何檔案內容
            if (!provider.Contents.Any())
            {
                // 如果一個檔案都沒有，回傳 400 Bad Request 錯誤
                var error = new ErrorResponseDto
                {
                    ErrorCode = "NO_FILES_UPLOADED",
                    Message = "請求中未包含任何檔案。"
                };
                return Content(HttpStatusCode.BadRequest, error);
            }

                var stage = _db.Stages.Find(stageId);

                var stageDescriptions = JsonConvert.DeserializeObject<List<string>>(stage.StageDescription ?? "[]");

                // b. (防呆) 檢查收到的圖片數量是否符合關卡要求
                if (provider.Contents.Count != stageDescriptions.Count)
                {
                    var error = new ErrorResponseDto
                    {
                        ErrorCode = "IMAGE_COUNT_MISMATCH",
                        Message = "圖片數量不符。"
                    };
                    return Content(HttpStatusCode.BadRequest, error);
                }

                // --- 步驟 4: 將檔案上傳到 Cloudinary 並保持順序 ---

                // 建立一個非同步任務列表，用來存放每一個「上傳到 Cloudinary」的任務
                var uploadTasks = provider.Contents
                // 核心排序邏輯：根據前端傳送時設定的 form-data `name` ("image_0", "image_1"...) 進行排序
                .OrderBy(c => c.Headers.ContentDisposition.Name)
                .Select(async file =>
                {
                    // 從檔案內容中讀取位元組資料
                    var fileBytes = await file.ReadAsByteArrayAsync();
                    // 取得原始檔名
                    var fileName = file.Headers.ContentDisposition.FileName.Trim('\"');

                    // 定義在 Cloudinary 上的儲存路徑，確保每個使用者的檔案都分開
                    var folderPath = $"trials/{trialId}/stages/{stageId}/{currentUserId}";

                    // 呼叫我們的 Cloudinary 服務來上傳，這是一個非同步操作
                    var (secureUrl, publicId) = await _cloudinaryService.UploadImageAsync(fileBytes, fileName, folderPath);

                    // 可以在這裡加上更完整的錯誤處理，例如拋出例外
                    if (string.IsNullOrEmpty(secureUrl))
                    {
                        throw new Exception($"File '{fileName}' failed to upload.");
                    }

                    return new { SecureUrl = secureUrl, PublicId = publicId };


                });

            // 1. 使用 Task.WhenAll 等待所有上傳任務完成，並取得結果陣列
            //    uploadResults 將會是一個陣列，裡面每個元素都是 { SecureUrl = "...", PublicId = "..." }
            var uploadResults = await Task.WhenAll(uploadTasks);
            // 2. 從結果陣列中，分別取出 SecureUrl 和 PublicId 到各自的 List
            var allUploadedUrls = uploadResults.Select(r => r.SecureUrl).ToList();
            allPublicIds = uploadResults.Select(r => r.PublicId).ToList();


            // --- 步驟 5: 執行 AI 驗證 (使用您之前的邏輯) ---

            // a. 從資料庫取得此關卡的驗證規則
            var trial = _db.Trials.FirstOrDefault(t => t.Id == trialId);

            if (stage.TrialTemplateId != trial.TrialTemplateId)
            {
                return Content(HttpStatusCode.BadRequest, new ErrorResponseDto
                {
                    ErrorCode = "STAGE_TRIAL_MISMATCH",
                    Message = "關卡與試煉不匹配。"
                });
            }

           

            var generalRules = JsonConvert.DeserializeObject<List<string>>(trial.TrialTemplate.TrialRule ?? "[]");
            var aiType = trial.TrialTemplate.AiType;

            // c. 呼叫 AI 服務進行驗證
            List<ImageResult> verificationResults = await _aiService.VerifyImagesAsync(allUploadedUrls, stageDescriptions, generalRules, aiType);

            // --- 步驟 D: 處理 AI 結果並更新資料庫 ---
            var response = new AiVerificationResponseV2(); // 準備回傳給前端的物件
                                                           // d. 檢查是否有任何一張圖片驗證失敗
            bool isOverallPassed = verificationResults.All(r => r.IsCompliant && r.IsSafe);
            response.OverallResult = isOverallPassed;
            response.OverallMessage = isOverallPassed ? "恭喜！此關卡所有照片均審核通過！" : "很遺憾，此關卡有照片未通過審核。";
            response.ImageResults = verificationResults;



            // --- 步驟 6: 根據 AI 結果，更新資料庫 ---
            userStage.ChanceRemain--;
            response.ChanceRemain = userStage.ChanceRemain;

            if (isOverallPassed)
            {
                // 如果驗證成功，儲存圖片 URL 列表
                userStage.Status = Status.pass;
                userStage.UploadImagePath = JsonConvert.SerializeObject(allUploadedUrls);
                userStage.ImageUploadAt = DateTime.Now;
            }
            else
            {
                // 如果驗證失敗，更新狀態，但不儲存 URL
                userStage.Status = Status.fail;
                userStage.UploadImagePath = null;
                // **核心**：呼叫刪除 API
                var deleteTasks = allPublicIds.Select(id => _cloudinaryService.DeleteImageAsync(id));
                await Task.WhenAll(deleteTasks); // 平行刪除
            }

                await _db.SaveChangesAsync(); // 使用非同步儲存

                // --- 步驟 7: 回傳最終的審核結果給前端 ---
                return Ok(response);
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