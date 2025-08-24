// 檔案: Models/DTOs/TrialTemplateInfoDto.cs
using Newtonsoft.Json;
using System.Collections.Generic;

namespace GoBetGoal_BackEnd.Models.DTOs
{
    /// <summary>
    /// 試煉模板的詳細資訊 (對應前端 ChallengeSupa)
    /// </summary>
    public class TrialTemplateInfoDto
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("title")]
        public string TrialTitle { get; set; }

        [JsonProperty("description")]
        public string TrialDescription { get; set; }

        [JsonProperty("frequency")]
        public int TrialFrequency { get; set; }

        [JsonProperty("category")]
        public List<string> TrialCategory { get; set; }

        [JsonProperty("suit_for")]
        public List<string> TrialSuitFor { get; set; }

        [JsonProperty("no_suit_for")]
        public List<string> TrialNoSuitFor { get; set; }

        [JsonProperty("rule")]
        public List<string> TrialRule { get; set; }

        [JsonProperty("caution")]
        public List<string> TrialCaution { get; set; }

        [JsonProperty("effect")]
        public List<string> TrialEffect { get; set; }

        [JsonProperty("img")]
        public string CardImagePath { get; set; }

        [JsonProperty("stage_count")]
        public int StageCount { get; set; }

        [JsonProperty("price")]
        public int TrialTemplatePrice { get; set; }

        [JsonProperty("color")]
        public string CardColor { get; set; }

        [JsonProperty("check_by_ai")]
        public bool IsAi { get; set; }

        [JsonProperty("max_user")]
        public int MaxUser { get; set; }

        /// <summary>
        /// 此試煉模板包含的所有關卡
        /// </summary>
        [JsonProperty("challenge_stage")]
        public List<TrialStageDto> ChallengeStages { get; set; }
    }
}