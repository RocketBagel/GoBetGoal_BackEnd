using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GoBetGoal_BackEnd.Models.DTOs
{
    public class UpdateAvatarRequestDto
    {
        [Required(ErrorMessage = "頭像 ID為必填欄位")]
        [Range(1, int.MaxValue, ErrorMessage = "無效的頭像 ID")]
        public int AvatarId { get; set; }

    }
}