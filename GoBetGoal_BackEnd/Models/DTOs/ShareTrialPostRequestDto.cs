using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GoBetGoal_BackEnd.Models.DTOs
{
    public class ShareTrialPostRequestDto
    {
        /// <summary>
        /// 使用者填寫的心得內容
        /// </summary>
        //[Required(ErrorMessage = "心得內容為必填")]
        [StringLength(50, ErrorMessage = "內容不可超過 50 個字元")]
        public string Content { get; set; }

        /// <summary>
        /// (可選) 使用者額外上傳的封面照片 URL
        /// </summary>
        public string CoverImageUrl { get; set; }
    }
}