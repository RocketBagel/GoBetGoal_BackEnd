using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace GoBetGoal_BackEnd.Models
{
    public class PostLike
    {

        [Key]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required]
        public int PostId { get; set; }
        public virtual Post Post { get; set; }

        [Required]
        public Guid UserId { get; set; }
        public virtual User User { get; set; }
    }
}