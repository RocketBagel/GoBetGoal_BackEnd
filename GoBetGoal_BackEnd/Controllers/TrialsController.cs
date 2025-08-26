using GoBetGoal_BackEnd.Enums;
using GoBetGoal_BackEnd.Models;
using GoBetGoal_BackEnd.Models.DTOs;
using GoBetGoal_BackEnd.Services;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;

namespace GoBetGoal_BackEnd.Controllers
{
    public class TrialsController : BaseApiController
    {
        private readonly Context _context = new Context();

        [AllowAnonymous]
        [HttpGet]
        [Route("api/trials/templates")]
        public IHttpActionResult GetAllTrialTemplates()
        {
            // 先把資料庫資料撈出來 (包含關卡 Stages)
            var templates = _context.TrialTemplates.Include(t => t.Stages).ToList();

            // 建立一個 DTO 清單
            var result = new List<TrialTemplateListDto>();


            foreach (var template in templates)
            {
                var allTrialTemplates = new TrialTemplateListDto
                {
                    Id = template.Id,
                    TrialTitle = template.TrialTitle,
                    TrialDescription = template.TrialDescription,
                    TrialFrequency = template.TrialFrequency,
                    TrialCategory = template.TrialCategory,
                    TrialSuitFor = template.TrialSuitFor,
                    TrialNoSuitFor = template.TrialNoSuitFor,
                    TrialRule = template.TrialRule,
                    TrialCaution = template.TrialCaution,
                    TrialEffect = template.TrialEffect,
                    StageCount = template.Stages.Count(),
                    MaxUser = template.MaxUser,
                    IsAi = template.IsAi,
                    TrialTemplatePrice = template.TrialTemplatePrice,
                    CardImagePath = template.CardImagePath,
                    CardColor = template.CardColor,
                    Stages = new List<StageInfoDto>()

                };

                // 用 foreach 把 Stage Entity 轉成 StageInfoDto
                foreach (var stage in template.Stages)
                {
                    var stageDto = new StageInfoDto
                    {
                        StageIndex = stage.StageIndex,
                        StageDescription = stage.StageDescription,
                        StageSampleImagePath = stage.StageSampleImagePath
                    };

                    allTrialTemplates.Stages.Add(stageDto);
                }

                result.Add(allTrialTemplates);
            }

            // 回傳 DTO 清單
            return Ok(result);
        }

  
        [HttpPost]
        [Route("api/trials")]
        public IHttpActionResult CreateTrial([FromBody] CreateTrialRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 轉換 StartAt
            if (!DateTime.TryParse(request.StartAt, out DateTime startTime) || startTime < DateTime.Now)
            { 
                return BadRequest("StartAt 必須是有效日期格式"); 
            }

            // 轉換 ChallengeId
            if (!int.TryParse(request.ChallengeId, out int templateId))
            {
                return BadRequest("ChallengeId 必須是整數");
            }

            // 轉換 CreateBy
            if (!Guid.TryParse(request.CreateBy, out Guid userId))
            {
                return BadRequest("CreateBy 必須是合法 Guid");
            }


            try
            {
                var service = new CreateTrialsService(_context);
                var trial = service.CreateTrialRelatedLogic(
                    userId,
                    templateId,
                    request.Title,
                    startTime,
                    request.Deposit
                );

                return Ok(new
                {
                    trialId = trial.Id
                    //trial.TrialName,
                    //StartTime = trial.StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    //EndTime = trial.EndTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    //trial.TrialDeposit,
                    //trial.TrialTemplateId,
                    //trial.TrialStatus
                });
            }
            catch (CreateTrialsService.NotFoundException ex)
            { 
                var error = new { errorMessage = ex.Message };
                return Content(HttpStatusCode.NotFound, error); // 404
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message); // 400
            }
            catch (Exception ex)
            {
                return InternalServerError(ex); // 500
            }
        }


        //public IHttpActionResult CreateTrial([FromBody] CreateTrialRequestDto request)
        //{
        //    //if (!ModelState.IsValid)
        //    //    return BadRequest(ModelState);

        //    // 轉換 StartAt
        //    if (!DateTime.TryParse(request.StartAt, out DateTime startTime))
        //    {
        //        return BadRequest("StartAt 必須是有效日期格式");
        //    }

        //    if (startTime < DateTime.Now)
        //    {
        //        return BadRequest("非有效日期，請重新確認輸入！");
        //    }

        //    // 轉換 ChallengeId
        //    if (!int.TryParse(request.ChallengeId, out int templateId))
        //    {
        //        return BadRequest("ChallengeId 必須是整數");
        //    }

        //    // 轉換 CreateBy
        //    if (!Guid.TryParse(request.CreateBy, out Guid userId))
        //    {
        //        return BadRequest("CreateBy 必須是合法 Guid");
        //    }

        //    // 1. 取得 TrialTemplate
        //    var template = _context.TrialTemplates.FirstOrDefault(t => t.Id == templateId);

        //    if (template == null)
        //    {
        //        return NotFound();
        //    }

        //    // 檢查使用者是否已解鎖該模板
        //    var hasTemplate = _context.UserTrialTemplates
        //        .Any(ut => ut.UserId == userId && ut.TrialTemplateId == templateId);

        //    if (!hasTemplate)
        //        return Content(System.Net.HttpStatusCode.BadRequest, "該模板尚未購買解鎖");

        //    // 2. 計算 EndTime
        //    // EndTime = StartTime + (StageCount * TrialFrequency)天
        //    var totalDays = template.StageCount * template.TrialFrequency;
        //    var endTime = startTime.Date.AddDays(totalDays);

        //    // 3. 建立 Trial
        //    var trial = new Trial
        //    {
        //        UserId = userId,
        //        TrialTemplateId = templateId,
        //        TrialName = request.Title,
        //        TrialDeposit = request.Deposit,
        //        StartTime = startTime.Date,
        //        EndTime = endTime,
        //        TrialStatus = Status.pending,
        //        CreatedAt = DateTime.Now
        //    };

        //    // 4. 建立 TrialParticipant
        //    using (var transaction = _context.Database.BeginTransaction())
        //    {
        //        try
        //        {
        //            // 先新增 Trial
        //            _context.Trials.Add(trial);


        //            // 新增 TrialParticipant
        //            var participant = new TrialParticipant
        //            {
        //                TrialId = trial.Id,
        //                ParticipantId = userId,
        //                InviteeId = userId,
        //                InviteAt = DateTime.Now
        //            };
        //            participant.Status = (participant.ParticipantId == participant.InviteeId)
        //                ? Status.accepted
        //                : Status.pending;

        //            _context.TrialParticipants.Add(participant);

        //            // 最後一次 SaveChange 包成一個 Transaction
        //            _context.SaveChanges();

        //            // 全部成功才提交
        //            transaction.Commit();

        //            // 回傳新增結果
        //            return Ok(new
        //            {
        //                trial.Id,
        //                trial.TrialName,
        //                trial.StartTime,
        //                trial.EndTime,
        //                trial.TrialDeposit,
        //                trial.TrialTemplateId,
        //                trial.TrialStatus
        //            });
        //        }
        //        catch (Exception ex)
        //        {
        //            // 其中一個失敗就回滾
        //            transaction.Rollback();
        //            return InternalServerError(ex);
        //        }
        //    }




        //}

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _context.Dispose();

            base.Dispose(disposing);
        }
    }

}
