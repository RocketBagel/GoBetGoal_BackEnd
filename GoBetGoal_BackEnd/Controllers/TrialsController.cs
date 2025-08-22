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


        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _context.Dispose();

            base.Dispose(disposing);
        }
    }

}
