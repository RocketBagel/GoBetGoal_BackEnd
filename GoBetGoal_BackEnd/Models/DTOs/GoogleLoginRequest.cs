using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GoBetGoal_BackEnd.Models.DTOs
{
    public class GoogleLoginRequest
    {
        [Required(ErrorMessage = "未提供 Google Token。")]
        public string Token { get; set; }

    }
}