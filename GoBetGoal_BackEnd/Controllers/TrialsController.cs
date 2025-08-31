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



        [HttpPost]
        [Route("api/trials/{templateId}/purchase")]
        public IHttpActionResult PurchaseTrialTemplate(int templateId)
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

            //找出要購買解鎖的試煉模板
            var theTrialTemplate = _context.TrialTemplates.FirstOrDefault(t => t.Id == templateId);
            if (theTrialTemplate == null)
            {
                return Content(HttpStatusCode.NotFound, new ErrorResponseDto
                {
                    ErrorCode = "TRIALTEMPLATE_NOT_FOUND",
                    Message = "指定的試煉模板不存在。"
                });
            }

            //檢查使用者是否已經擁有
            bool userOwned = _context.UserTrialTemplates.Any(ut => ut.UserId == currentUserId && ut.TrialTemplateId == templateId);
            if (userOwned)
            {
                return Content(HttpStatusCode.Conflict, new ErrorResponseDto 
                {
                    ErrorCode = "TRIALTEMPLATE_ALREADY_OWNED", 
                    Message = "您已經擁有此模板。" 
                });
            }

            //確認貝果餘額是否足夠
            if (user.BagelCount < theTrialTemplate.TrialTemplatePrice)
            {
                return Content(HttpStatusCode.BadRequest, new ErrorResponseDto 
                {
                    ErrorCode = "INSUFFICIENT_BAGELS",
                    Message = "貝果數量不足。" 
                });
            }

            var balanceBefore = user.BagelCount;
            var balanceAfter = balanceBefore - theTrialTemplate.TrialTemplatePrice;

            // 建立 BagelTransaction 資料
            var transaction = new BagelTransaction
            {
                UserId = user.Id,
                TransactionType = TransactionType.花費解鎖,
                ProductType = ProductType.TrialTemplate,
                ReferenceId = templateId,
                ItemName = theTrialTemplate.TrialTitle,
                Price = theTrialTemplate.TrialTemplatePrice,
                Quantity = 1,
                Amount = theTrialTemplate.TrialTemplatePrice,
                BalanceBefore = balanceBefore,
                BalanceAfter = balanceAfter,
                CreatedAt = DateTime.Now
            };

            // 建立 UserTrialTemplate 資料
            var newOwned = new UserTrialTemplate
            {
                UserId = user.Id,
                TrialTemplateId = templateId,
                AcquiredAt = DateTime.Now
            };

            using (var dbTransaction = _context.Database.BeginTransaction())
            {
                try
                {
                    _context.BagelTransactions.Add(transaction);
                    user.BagelCount = balanceAfter;
                    _context.UserTrialTemplates.Add(newOwned);

                    _context.SaveChanges(); 

                    dbTransaction.Commit(); 
                }
                catch (Exception ex)
                {
                    dbTransaction.Rollback(); // 失敗就回滾
                    return InternalServerError(ex);
                }
            }

            // 回傳成功的結果
            var successResponse = new PurchaseSuccessDto
            {
                Message = $"已成功解鎖試煉模板-{theTrialTemplate.Id}！",
                RemainingBagelCount = user.BagelCount // 回傳更新後的餘額
            };

            return Ok(successResponse);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _context.Dispose();

            base.Dispose(disposing);
        }
    }

}
