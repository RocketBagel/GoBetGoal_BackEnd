using GoBetGoal_BackEnd.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace GoBetGoal_BackEnd.Models
{
    public class UserStage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public Guid UserId { get; set; }
        public virtual User User { get; set; }

        [Required]
        public int TrialId { get; set; }
        public virtual Trial Trial { get; set; }

        [Required]
        public int StageId { get; set; }
        public virtual Stage Stage { get; set; }

        [StringLength(500)]
        public string UploadImagePath { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? ImageUploadAt { get; set; }

        // 無須審核的關卡怎麼顯示
        [Required]
        public int ChanceRemain { get; set; } = 3;

        [Required]
        [Column(TypeName = "datetime2")]
        public DateTime StartTime { get; set; }

        [Required]
        [Column(TypeName = "datetime2")]
        public DateTime EndTime { get; set; }

        // 看畫面需不需要判斷各關卡是否使用遮羞布
        [Required]
        public bool IsCheat { get; set; } = false;

        [Required]
        public Status Status { get; set; } = (Status)0; //預設是pending

        //supabase有此欄位, 需要確認產生時間及是否需要此欄位
        [Column(TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual Post Post { get; set; } //一對一

        // (可選，如果你需要在 UserStage.cs 中查詢歷史紀錄)
        public virtual ICollection<CheatBlanketHistory> CheatBlanketHistories { get; set; } = new List<CheatBlanketHistory>();

    }
}