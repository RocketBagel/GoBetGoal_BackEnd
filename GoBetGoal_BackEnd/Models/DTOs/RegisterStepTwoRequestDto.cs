using GoBetGoal_BackEnd.Models.Validations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GoBetGoal_BackEnd.Models.DTOs
{
    public class RegisterStepTwoRequestDto
    {
        //[Required(ErrorMessage = "暱稱為必填欄位")]
        [NicknameLength(10)]
        public string NickName { get; set; }

        [Required(ErrorMessage = "頭像Id為必填欄位")]
        [RegularExpression("^[0-9]+$", ErrorMessage = "無效的頭像 ID")]
        public string AvatarId { get; set; }
    }
}