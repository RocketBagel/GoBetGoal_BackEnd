using GoBetGoal_BackEnd.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoBetGoal_BackEnd.Models.DTOs
{
    public class PurchaseRequestDto
    {
        [Required]
        [JsonProperty("user_id")]
        public Guid UserId { get; set; }

        [Required]
        [JsonProperty("item_type")]
        public ProductType ItemType { get; set; }  // 'cheat_blanket' → ProductType.CheatBlanket

        
        [JsonProperty("item_id")]
        public string ItemId { get; set; }            // CheatBlanket 不需要，Avatar/TrialTemplate 會用到

        [Required]
        [JsonProperty("item_name")]
        public string ItemName { get; set; }

        [Required]
        public int Price { get; set; }

       
    }
}
