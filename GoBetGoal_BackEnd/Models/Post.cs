using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace GoBetGoal_BackEnd.Models
{
    public class Post
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string Content { get; set; }

        [Required]
        public Guid UserId { get; set; }
        public virtual User User { get; set; }

        [Required]
        public int TrialId { get; set; }
        public virtual Trial Trial { get; set; }

        [Required]
        public string ImageUrl { get; set; } // 如果和USERSTAGE的UPLOADIMAGE欄位一樣也可以直接抓USERSTAGE的UPLOADIMAGE欄位

        [Required]
        // 建立一個名為 "IX_UserStageId" 的索引，並設定為 IsUnique = true
        [Index("IX_UserStageId")]
        public int UserStageId { get; set; } // 詢問: 是否一關只會上船一次
        public virtual UserStage UserStage { get; set; }

        public virtual ICollection<PostLike> PostLikes { get; set; } = new List<PostLike>();



    }
}