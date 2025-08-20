using GoBetGoal_BackEnd.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace GoBetGoal_BackEnd.Models
{
    public class TrialParticipant
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TrialId { get; set; }  //對應的試煉場次Id
        public virtual Trial Trial { get; set; }

        [Required]
        public Guid ParticipantId { get; set; }  //發送試煉邀請者的UserId
        public virtual User Participant { get; set; }

        [Required]
        public Guid InviteeId { get; set; }   //受邀者的UserId
        public virtual User Invitee { get; set; }

        [Required]
        public Status Status { get; set; }  //狀態(Pending,accepted)

        [Required]
        [Column(TypeName = "datetime2")]
        public DateTime InviteAt { get; set; } = DateTime.Now;  //建立時間(發送邀請時間)

        [Column(TypeName = "datetime2")]
        public DateTime? UpdatedAt { get; set; }    //狀態更新時間
    }
}