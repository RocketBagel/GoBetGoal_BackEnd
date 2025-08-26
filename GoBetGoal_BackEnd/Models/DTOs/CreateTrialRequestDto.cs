using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoBetGoal_BackEnd.Models.DTOs
{
    // <summary>
    // 建立試煉任務的請求 DTO
    // </summary>
    public class CreateTrialRequestDto
    {
        [Required]
        [JsonProperty("start_at")]
        public string StartAt { get; set; }    // 前端 start_at

        [Required]
        public int Deposit { get; set; }         // 前端 deposit

        [Required]
        [JsonProperty("challenge_id")]
        public string ChallengeId { get; set; }     // 前端 challenge_id -> TrialTemplateId

        [Required]
        [StringLength(100, ErrorMessage = "Tiral Title不得超過 100 個字元")]
        public string Title { get; set; }        // 前端 title -> TrialName

        [Required]
        [JsonProperty("create_by")]
        public string CreateBy { get; set; }       // 前端 create_by -> UserId
    }
}
