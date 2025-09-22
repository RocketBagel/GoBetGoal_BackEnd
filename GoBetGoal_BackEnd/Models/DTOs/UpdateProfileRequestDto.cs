using GoBetGoal_BackEnd.Models.Validations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace GoBetGoal_BackEnd.Models.DTOs
{
    public class UpdateProfileRequestDto
    {
        [Required(ErrorMessage = "暱稱為必填欄位")]
        [NicknameLength(10)]
        public string NickName { get; set; }

        [Required(ErrorMessage = "密碼為必填欄位")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "密碼長度必須介於 6 到 100 個字元之間")]
        public string Password { get; set; }
    }
}