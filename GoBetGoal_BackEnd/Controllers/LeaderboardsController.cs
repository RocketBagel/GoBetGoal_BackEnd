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

        [HttpGet]
        [Route("api/leaderboards/social-experts")]
        [AllowAnonymous]
        public IHttpActionResult GetSocialExpertsLeaderboard()
        {
            // 步驟 1: 預先載入需要的關聯資料，並將所有使用者撈到記憶體中
            var allUsers = _db.Users
                .Include(u => u.UserAvatars.Select(ua => ua.Avatar))
                .ToList();

            // 步驟 2: 建立一個列表，用來存放每位使用者的計算結果
            var userStats = new List<SocialExpertDto>();

            // 步驟 3: 使用 foreach 迴圈，一個一個地為使用者計算分數
            foreach (var user in allUsers)
            {
                // a. 計算這位使用者所有貼文被按讚的總數
                int totalLikes = _db.PostLikes.Count(like => like.Post.UserId == user.Id);

                // b. 計算這位使用者成功完成的試煉總數
                int successfulTrials = _db.TrialParticipants.Count(tp =>
                    tp.InviteeId == user.Id &&
                    tp.Trial.TrialStatus == Status.pass || tp.Trial.TrialStatus == Status.perfect); 

                // c. 將計算結果和使用者資訊，存入一個新的 DTO 物件
                userStats.Add(new SocialExpertDto
                {
                    LikedPostsCount = totalLikes,
                    SuccessfulTrialCount = successfulTrials,
                    UserInfo = new PublicUserProfileDto
                    {
                        UserId = user.Id,
                        NickName = user.NickName,
                        CurrentAvatarUrl = user.UserAvatars.FirstOrDefault(ua => ua.IsCurrent)?.Avatar?.AvatarImagePath
                    }
                });
            }

            // 步驟 4: 對計算完畢的列表進行排序
            var sortedLeaderboard = userStats
                .OrderByDescending(s => s.LikedPostsCount) // 主要規則：按讚數越多的排前面
                .ThenByDescending(s=>s.SuccessfulTrialCount)
                .ToList();

            // 步驟 5: 賦予名次 (處理同名次)
            int rank = 0;
            int lastScore = -1;

            for (int i = 0; i < sortedLeaderboard.Count; i++)
            {
                var currentEntry = sortedLeaderboard[i];

                if (currentEntry.LikedPostsCount != lastScore)
                {
                    rank = i + 1;
                }
                currentEntry.Rank = rank;
                lastScore = currentEntry.LikedPostsCount;
            }

            // 步驟 6: 只取前 3 名
            var top3 = sortedLeaderboard.Take(3).ToList();

            return Ok(top3);
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