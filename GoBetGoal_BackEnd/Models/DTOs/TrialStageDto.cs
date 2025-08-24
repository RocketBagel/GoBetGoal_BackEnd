using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GoBetGoal_BackEnd.Models.DTOs
{
    public class TrialStageDto
    {
        [JsonProperty("id")]
        public int Id { get; set; } // 假設 Stage ID 是 int

        [JsonProperty("stage_index")]
        public int StageIndex { get; set; }

        // 根據前端的定義，這兩個欄位都是陣列
        [JsonProperty("sample_image")]
        public List<string> SampleImage { get; set; }

        [JsonProperty("description")]
        public List<string> Description { get; set; }
    }
}