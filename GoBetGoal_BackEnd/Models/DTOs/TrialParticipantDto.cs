// 檔案: Models/DTOs/TrialParticipantDto.cs
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace GoBetGoal_BackEnd.Models.DTOs
{
    public class TrialParticipantDto
    {
        [JsonProperty("user_info")]
        public UserProfileDto UserInfo { get; set; } // 注意：UserProfileDto 也需要修改

        [JsonProperty("joined_at")]
        public DateTime JoinedAt { get; set; }

        [JsonProperty("pass_count")] // 假設欄位名稱
        public int PassCount { get; set; }

        [JsonProperty("cheat_blanket_count")] // 假設欄位名稱
        public int CheatBlanketCount { get; set; }

        [JsonProperty("fail_count")] // 假設欄位名稱
        public int FailCount { get; set; }

        [JsonProperty("stages")]
        public List<TrialStageProgressDto> Stages { get; set; }

    }
}