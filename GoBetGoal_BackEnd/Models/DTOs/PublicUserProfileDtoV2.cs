using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GoBetGoal_BackEnd.Models.DTOs
{
    public class PublicUserProfileDtoV2
    {
        [JsonProperty("user_id")]
        public Guid UserId { get; set; }

        [JsonProperty("nick_name")]
        public string NickName { get; set; }

        [JsonProperty("character_img_link")]
        public string CurrentAvatarUrl { get; set; }

        [JsonProperty("total_trial_count")]
        public int TotalTrialCount { get; set; }

        [JsonProperty("liked_posts_count")]
        public int LikedPostsCount { get; set; }

        [JsonProperty("friend_count")]
        public int FriendCount { get; set; }

        [JsonProperty("friend_state")]
        public string FriendState { get; set; }
    }
}