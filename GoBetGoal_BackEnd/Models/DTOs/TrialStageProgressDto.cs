// 檔案: Models/DTOs/TrialStageProgressDto.cs
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace GoBetGoal_BackEnd.Models.DTOs
{
    public class TrialStageProgressDto
    {
        [JsonProperty("challenge_stage_id")] // 對應前端 challenge_stage_id
        public int StageId { get; set; }

        [JsonProperty("stage_index")]
        public int StageIndex { get; set; }

        // 根據前端的定義，這兩個欄位都是陣列
        [JsonProperty("sample_image")]
        public List<string> StageSampleImagePath { get; set; }

        [JsonProperty("description")]
        public List<string> StageDescription { get; set; }

        [JsonProperty("start_at")]
        public DateTime? StartTime { get; set; }

        [JsonProperty("end_at")]
        public DateTime? EndTime { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("upload_image")]
        public List<string> UploadImagePaths { get; set; } 

        [JsonProperty("upload_at")]
        public DateTime? UploadAt { get; set; }

        [JsonProperty("chance_remain")]
        public int? ChanceRemain { get; set; }
    }
}