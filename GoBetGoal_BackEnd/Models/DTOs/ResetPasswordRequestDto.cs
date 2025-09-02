using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GoBetGoal_BackEnd.Models.DTOs
{
    public class ResetPasswordRequestDto
    {
        [Required(ErrorMessage = "必須提供重設權杖")]
        public string Token { get; set; }

        [Required(ErrorMessage = "新密碼為必填欄位")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "新密碼長度必須介於 6 到 100 個字元之間")]
        public string NewPassword { get; set; }
    }
}