using GoBetGoal_BackEnd.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace GoBetGoal_BackEnd.Services.Verifiers
{
    /// 定義所有審核器都必須遵守的合約（介面）
    public interface IVerifier
    {
        Task<ChallengeSubmissionResponse> VerifyAsync(List<string> imageUrls);

        // 文字+圖片審核用
        //Task<ChallengeSubmissionResponse> VerifyAsync(VerificationRequest request);

    }

    //public class AiVerifier : IVerifier
    //{
    //    public async Task<ChallengeSubmissionResponse> VerifyAsync(VerificationRequest request)
    //    {
    //        // 呼叫 AI API 做圖片 + 文字審核
    //        return new ChallengeSubmissionResponse
    //        {
    //            OverallResult = true,
    //            OverallMessage = 
    //        };
    //    }
    //}


    // 模擬人工審核：假設所有圖片都合規
    //public class ManualVerifier : IVerifier
    //{
    //    // 不需要非同步 → 移除 async
    //    public Task<ChallengeSubmissionResponse> VerifyAsync(List<string> imageUrls)
    //    {
    //        // 模擬人工審核：假設所有圖片都合規
    //        var imageResults = imageUrls.Select(url => new ImageResult
    //        {
    //            ImageUrl = url,
    //            IsSafe = true,
    //            IsCompliant = true,
    //            Reason = "人工審核通過"
    //        }).ToList();

    //        var response = new ChallengeSubmissionResponse
    //        {
    //            OverallResult = true,
    //            OverallMessage = "所有圖片皆通過人工審核",
    //            ImageResults = imageResults
    //        };

    //        return Task.FromResult(response);
    //    }
    //}
}