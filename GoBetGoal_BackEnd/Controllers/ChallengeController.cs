// 1. 確保檔案頂部有這些 using 指示詞
using GoBetGoal_BackEnd.Controllers;
using GoBetGoal_BackEnd.Models;
using GoBetGoal_BackEnd.Models.DTOs;
using GoBetGoal_BackEnd.Security;
using GoBetGoal_BackEnd.Services;
using GoBetGoal_BackEnd.Services.Verifiers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

// 3. 主要的 Controller 類別
public class ChallengeController : BaseApiController
{
    //private readonly ChallengeDbService _dbService = new ChallengeDbService();


    //[HttpPost]
    //[Route("api/challenge/submit")]
    //public async Task<IHttpActionResult> SubmitChallengeStage([FromBody] ChallengeSubmissionRequest request)
    //{
    //    Guid currentUserId = GetCurrentUserId();

    //    if (request == null || request.ImageUrls == null || !request.ImageUrls.Any())
    //        return BadRequest("請求格式錯誤或未提供圖片。");

    //    using (var _db = new Context())
    //    {
    //        var userStage = await _db.UserStages
    //.Include("Trial.TrialTemplate")
    //.Include("Stage.TrialTemplate")
    //.FirstOrDefaultAsync(us =>
    //    us.UserId == currentUserId &&
    //    us.TrialId == request.TrialId &&
    //    us.Stage.StageIndex == request.StageIndex);


    //        if (userStage == null)
    //        {
    //            return NotFound();
    //        }

    //        if (userStage.ChanceRemain <= 0)
    //        {
    //            return BadRequest("您今日的 AI 審核次數已用完。");
    //        }

    //        var stageRules = userStage.Stage.StageDescription;
    //        var trialRules = userStage.Stage.TrialTemplate.TrialRule;

    //        if (stageRules == null || trialRules == null)
    //        {
    //            return InternalServerError(new Exception("關卡規則資料不完整。"));
    //        }

    //        var stageRulesJson = JsonConvert.DeserializeObject<List<string>>(stageRules);
    //        var trialRulesJson = JsonConvert.DeserializeObject<List<string>>(trialRules);

    //        IVerifier verifier = (userStage.Stage.VerificationMode == "Collective")
    //            ? (IVerifier)new CollectiveVerifier(userStage.Stage)
    //            : new PerImageVerifier(userStage.Stage);

    //        // --- D. 執行審核 ---
    //        // 將審核任務委派給選擇好的審核器去執行。
    //        var result = await verifier.VerifyAsync(request.ImageUrls);

    //        // --- 7. 處理審核結果 (更新 userStage 物件) ---
    //        userStage.ChanceRemain--; // 無論成敗，都扣除一次機會

    //        if (result.OverallResult)
    //        {
    //            userStage.UploadImagePath = JsonConvert.SerializeObject(request.ImageUrls);
    //            userStage.ImageUploadAt = DateTime.Now;
    //            userStage.Status = (GoBetGoal_BackEnd.Enums.Status)2;
    //        }
    //        //else
    //        //{
    //        //    // (可選) 如果失敗，也可以更新狀態
    //        //    // userStage.Status = Enums.Status.fail;
    //        //}

    //        _db.Entry(userStage).State = EntityState.Modified;
    //        await _db.SaveChangesAsync();

    //        result.ChanceRemain = userStage.ChanceRemain;

    //        return Ok(result);
    //    }
    //    // --- E. 如果所有圖片都失敗，嘗試備用審核策略 ---
    //    //if (!result.OverallResult && result.ImageResults.All(r => !r.IsSafe))
    //    //{
    //    //    var fallbackResult = await TryFallbackVerification(request.ImageUrls, stage);
    //    //    if (fallbackResult.OverallResult)
    //    //    {
    //    //        result = fallbackResult;
    //    //        result.OverallMessage += " (使用備用審核通過)";
    //    //    }
    //    //}
    //}
}





