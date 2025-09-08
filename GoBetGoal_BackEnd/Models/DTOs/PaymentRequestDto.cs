using GoBetGoal_BackEnd.Models.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GoBetGoal_BackEnd.Models.DTOs
{
    public class PaymentRequestDto
    {
        [Required]
        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [Required]
        [JsonProperty("email")]
        [EmailAddress]
        public string UserEmail { get; set; }

        [Required]
        [JsonProperty("get_bagel")]
        [JsonConverter(typeof(StrictIntConverter))]
        [Range(1, int.MaxValue, ErrorMessage = "get_bagel 必須為正整數")]
        public dynamic BagelCount { get; set; }

        [Required]
        [JsonProperty("deposit_money")]
        [JsonConverter(typeof(StrictIntConverter))]
        [Range(1, int.MaxValue, ErrorMessage = "deposit_money 必須為正整數")]
        public dynamic Amount { get; set; }

    }
}
