using GoBetGoal_BackEnd.Enums;
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
    public class LeaderboardsController : BaseApiController
    {
        private readonly Context _db = new Context();

        [HttpGet]
        [Route("api/leaderboards")]
        [AllowAnonymous]
        public IHttpActionResult GetLeaderboard([FromUri] string type = "successfulTrials")
        {
            // 步驟 1: 查詢所有使用者的基礎統計數據 (不變)
            var userStats = _db.Users.Include(u => u.UserAvatars.Select(ua => ua.Avatar))
                .Select(u => new
                {
                    User = u,
                    TotalTrials = u.Invitees.Count(tp => tp.Status == Status.accepted),
                    SuccessfulTrials = u.Invitees.Count(tp => tp.Trial.TrialStatus == Status.pass || tp.Trial.TrialStatus == Status.perfect && tp.Status == Status.accepted),
                    LikedPosts = _db.PostLikes.Count(like => like.Post.UserId == u.Id),
                })
                .ToList();

            // --- *** 步驟 2: 根據 type 參數，決定「簡化後」的排序規則 *** ---
            List<LeaderboardUserDto> sortedLeaderboard;

            if (type.Equals("totalTrials", StringComparison.OrdinalIgnoreCase))
            {
                // 越戰越勇榜排序 (根據總參與數 >> 試煉成功數)
                sortedLeaderboard = userStats
                    .OrderByDescending(s => s.TotalTrials)
                    .ThenByDescending(s => s.SuccessfulTrials)
                    .Select(s => new LeaderboardUserDto {
                        UserInfo = new PublicUserProfileDto {

                            UserId = s.User.Id,
                            NickName = s.User.NickName,
                            CurrentAvatarUrl = s.User.UserAvatars.FirstOrDefault(x => x.IsCurrent).Avatar.AvatarImagePath

                        },
                        TotalTrialCount = s.TotalTrials,
                        SuccessfulTrialCount = s.SuccessfulTrials,
                        LikedPostsCount = s.LikedPosts
                    })
                    .ToList();
            }
            else // 預設為 successfulTrials
            {
                // 試煉高手榜排序 (根據成功試煉數 >> 參與試煉數)
                sortedLeaderboard = userStats
                    .OrderByDescending(s => s.SuccessfulTrials)
                    .ThenByDescending(s=>s.TotalTrials)
                    .Select(s => new LeaderboardUserDto
                    {
                        UserInfo = new PublicUserProfileDto
                        {
                            UserId = s.User.Id,
                            NickName = s.User.NickName,
                            CurrentAvatarUrl = s.User.UserAvatars.FirstOrDefault(ua => ua.IsCurrent)?.Avatar?.AvatarImagePath
                        },
                        TotalTrialCount = s.TotalTrials,
                        SuccessfulTrialCount = s.SuccessfulTrials,
                        LikedPostsCount = s.LikedPosts
                    })
                    .ToList();
            }

            // --- *** 步驟 3: 賦予名次 (現在只比對主要分數) *** ---
            int rank = 0;
            int lastScore = -1; // 用來儲存上一個人的分數

            for (int i = 0; i < sortedLeaderboard.Count; i++)
            {
                var currentEntry = sortedLeaderboard[i];

                // 根據不同榜單，決定要比對的分數
                int currentScore = (type.Equals("totalTrials", StringComparison.OrdinalIgnoreCase))
                    ? currentEntry.TotalTrialCount
                    : currentEntry.SuccessfulTrialCount;

                // 檢查當前這位參賽者的分數，是否和上一位不同
                if (currentScore != lastScore)
                {
                    rank = i + 1; // 如果分數不同，就更新名次為當前的位置
                }

                currentEntry.Rank = rank; // 將計算好的名次，賦予給當前的參賽者

                // 更新「上一位的分數」，為下一次迴圈做準備
                lastScore = currentScore;
            }

            // 步驟 4: 只取前 10 名 (不變)
            var top10 = sortedLeaderboard.Take(10).ToList();

            return Ok(top10);
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