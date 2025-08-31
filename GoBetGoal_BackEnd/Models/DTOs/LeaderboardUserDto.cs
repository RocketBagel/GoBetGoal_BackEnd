using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GoBetGoal_BackEnd.Models.DTOs
{
    public class LeaderboardUserDto
    {
        [JsonProperty("rank")]
        public int Rank { get; set; }

        [JsonProperty("user_info")]
        public PublicUserProfileDto UserInfo { get; set; } // 我們可以重複使用之前建立的公開使用者資訊 DTO

        [JsonProperty("successful_trial_count")]
        public int SuccessfulTrialCount { get; set; }

        [JsonProperty("total_trial_count")]
        public int TotalTrialCount { get; set; }

        [JsonProperty("liked_posts_count")]
        public int LikedPostsCount { get; set; }

        // 為了排序用的內部欄位，不需要回傳給前端
        [JsonIgnore]
        public DateTime LastActivityDate { get; set; }
    }
}