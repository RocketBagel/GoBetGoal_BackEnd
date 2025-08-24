// 檔案: Models/DTOs/UserProfileDto.cs

// 記得在檔案頂端加入 using Newtonsoft.Json; 和 using System.Collections.Generic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace GoBetGoal_BackEnd.Models.DTOs
{
    /// <summary>
    /// 使用者的個人設定檔 (已根據前端 UserInfoSupa 需求調整)
    /// </summary>
    public class UserProfileDto
    {
        [JsonProperty("user_id")]
        public Guid UserId { get; set; }

        [JsonProperty("nick_name")]
        public string NickName { get; set; }

        [JsonProperty("character_img_link")]
        public string CurrentAvatarUrl { get; set; }

        [JsonProperty("bagel_count")]
        public int BagelCount { get; set; }

        [JsonProperty("cheat_blanket")]
        public int CheatBlanketCount { get; set; }

        //[JsonProperty("system_preference_color_mode")]
        //public string ColorMode { get; set; }

        [JsonProperty("total_trial_count")]
        public int TotalTrialCount { get; set; }

        [JsonProperty("liked_posts_count")]
        public int LikedPostsCount { get; set; }

        [JsonProperty("friend_count")]
        public int FriendCount { get; set; }

        // --- 新增前端需要的欄位 ---

        /// <summary>
        /// 使用者已購買/擁有的試煉模板 ID 列表
        /// </summary>
        [JsonProperty("purchase_challenge")]
        public List<int> PurchaseChallengeIds { get; set; }

        /// <summary>
        /// 使用者已購買/擁有的頭像 ID 列表
        /// </summary>
        [JsonProperty("purchase_avatar")]
        public List<int> PurchaseAvatarIds { get; set; }

        /// <summary>
        /// 當前檢視者與這位使用者的好友狀態
        /// (僅在檢視他人資料時有效)
        /// </summary>
        [JsonProperty("friend_state")]
        public string FriendState { get; set; }

    }
}