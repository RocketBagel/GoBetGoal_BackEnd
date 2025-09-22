using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GoBetGoal_BackEnd.Models.Validations
{
    public class NicknameLengthAttribute:ValidationAttribute
    {
        private readonly int _maxUnits;

        public NicknameLengthAttribute(int maxUnits)
        {
            _maxUnits = maxUnits;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null) return new ValidationResult("暱稱為必填欄位");

            string nickname = value.ToString();

            int totalUnits = 0;
            foreach (char c in nickname)
            {
                // 判斷是否是中文（或全形字元）
                if (c > 127) // 簡單判斷，ASCII 之外當中文
                    totalUnits += 2;
                else
                    totalUnits += 1;

                if (totalUnits > _maxUnits)
                    return new ValidationResult($"暱稱長度超過限制 (最多 5 個中文字或 10 個英文字)");
            }

            return ValidationResult.Success;
        }
    }
}