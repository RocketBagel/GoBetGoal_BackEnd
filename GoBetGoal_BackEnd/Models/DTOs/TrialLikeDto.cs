using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GoBetGoal_BackEnd.Models.DTOs
{
    public class TrialLikeDto
    {
        [JsonProperty("user_id")]
        public Guid UserId { get; set; }

        [JsonProperty("character_img_link")]
        public string CurrentAvatarUrl { get; set; }
    }
}