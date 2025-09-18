using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GoBetGoal_BackEnd.Models.DTOs
{
    public class RegisterStepTwoRequestDto
    {
        [Required(ErrorMessage = "暱稱為必填欄位")]
        [StringLength(5, ErrorMessage = "暱稱長度不可超過 5 個字元")]
        public string NickName { get; set; }

        [Required(ErrorMessage = "頭像Id為必填欄位")]
        public int AvatarId { get; set; }
    }
}