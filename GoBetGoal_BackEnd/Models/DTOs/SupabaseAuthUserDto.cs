using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GoBetGoal_BackEnd.Models.DTOs
{
    public class SupabaseAuthUserDto
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("email")]
        [EmailAddress]
        public string Email { get; set; }
    }
}