using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GoBetGoal_BackEnd.Models.DTOs
{
    public class SocialExpertDto
    {
        [JsonProperty("rank")]
        public int Rank { get; set; }

        // 我們可以重複使用 PublicUserProfileDto 來顯示使用者基本資訊
        [JsonProperty("user_info")]
        public PublicUserProfileDto UserInfo { get; set; }

        /// <summary>
        /// 該使用者成功完成的試煉總數
        /// </summary>
        [JsonProperty("successful_trial_count")]
        public int SuccessfulTrialCount { get; set; }
        /// <summary>
        /// 該使用者所有貼文被按讚的總數 (排序依據)
        /// </summary>
        [JsonProperty("total_liked_posts_count")]
        public int LikedPostsCount { get; set; }
    }
}