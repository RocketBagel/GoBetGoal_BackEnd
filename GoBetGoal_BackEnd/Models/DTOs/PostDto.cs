// 檔案路徑: Models/DTOs/PostDto.cs

using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace GoBetGoal_BackEnd.Models.DTOs
{
    public class PostDto
    {
        // 為了讓 C# 的屬性名稱 (PostLike) 能對應到 JSON 的 (post_like)，
        // 我們使用 JsonProperty 來做明確的轉換。

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("trial_history_id")]
        public int? UserStageId { get; set; } // 對應 UserStageId

        [JsonProperty("image_url")]
        public List<string> ImageUrl { get; set; }

        [JsonProperty("publish_by")]
        public Guid UserId { get; set; }

        [JsonProperty("trial_id")]
        public int TrialId { get; set; }

        [JsonProperty("trial")]
        public TrialInfoDto Trial { get; set; }

        [JsonProperty("user_info")]
        public UserInfoDto UserInfo { get; set; }

        [JsonProperty("post_like")]
        public List<PostLikeInfoDto> PostLike { get; set; }

        [JsonProperty("is_liked_by_viewer")]
        public bool IsLikedByViewer { get; set; }
    }

    // --- 以下是 PostDto 內部的巢狀 Class ---

    public class TrialInfoDto
    {
        [JsonProperty("title")]
        public string Title { get; set; } // 這是 Trial 的自訂名稱

        [JsonProperty("challenge")]
        public ChallengeInfoDto Challenge { get; set; }
    }

    public class ChallengeInfoDto
    {
        [JsonProperty("title")]
        public string Title { get; set; } // 這是 TrialTemplate 的標題

        [JsonProperty("category")]
        public List<string> Category { get; set; }
    }

    public class UserInfoDto
    {
        [JsonProperty("nick_name")]
        public string NickName { get; set; }

        [JsonProperty("character_img_link")]
        public string CharacterImgLink { get; set; }
    }

    public class PostLikeInfoDto
    {
        [JsonProperty("like_by")]
        public Guid LikeBy { get; set; }
    }
}