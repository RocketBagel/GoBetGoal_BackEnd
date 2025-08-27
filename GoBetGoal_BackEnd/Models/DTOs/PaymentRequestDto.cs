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
        public string UserEmail { get; set; }

        [Required]
        [JsonProperty("get_bagel")]
        public int BagelCount { get; set; }

        [Required]
        [JsonProperty("deposit_money")]
        public int Amount { get; set; }

        [Required]
        [JsonProperty("order_id")]
        public string OrderId { get; set; }
    }
}
