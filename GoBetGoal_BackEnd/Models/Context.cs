using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace GoBetGoal_BackEnd.Models
{
    public class Context : DbContext
    {
        public Context() : base("name=Context")
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Avatar> Avatars { get; set; }
        public DbSet<UserAvatar> UserAvatars { get; set; }
        public DbSet<TrialTemplate> TrialTemplates { get; set; }
        public DbSet<Stage> Stages { get; set; }
        public DbSet<Trial> Trials { get; set; }
        public DbSet<UserStage> UserStages { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<PostLike> PostLikes { get; set; }
        public DbSet<TrialLike> TrialLikes { get; set; }
        public DbSet<UserTrialTemplate> UserTrialTemplates { get; set; }
        public DbSet<TrialParticipant> TrialParticipants { get; set; }

        public DbSet<BagelTransaction> BagelTransactions { get; set; }
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
        public DbSet<FriendsRelationship> FriendsRelationships { get; set; }
        public DbSet<Achievement> Achievements { get; set; }
        public DbSet<UserAchievement> UserAchievements { get; set; }



        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- User 相關關聯 ---
            // User (一) -> UserAvatar (多) (Join Table)
            modelBuilder.Entity<UserAvatar>()
                .HasRequired(ua => ua.User)
                .WithMany(u => u.UserAvatars) // <== 明確指定 User.cs 中的 UserAvatars 集合
                .HasForeignKey(ua => ua.UserId)
                .WillCascadeOnDelete(false);

            // User (一) -> Trial (多)
            modelBuilder.Entity<Trial>()
                .HasRequired(t => t.User)
                .WithMany(u => u.Trials) // <== 明確指定 User.cs 中的 Trials 集合
                .HasForeignKey(t => t.UserId)
                .WillCascadeOnDelete(false);

            // User (一) -> UserStage (多)
            modelBuilder.Entity<UserStage>()
                .HasRequired(us => us.User)
                .WithMany(u => u.UserStages) // <== 明確指定 User.cs 中的 UserStages 集合
                .HasForeignKey(us => us.UserId)
                .WillCascadeOnDelete(false);

            // User (一) -> Post (多)
            modelBuilder.Entity<Post>()
                .HasRequired(p => p.User)
                .WithMany(u => u.Posts) // <== 明確指定 User.cs 中的 Posts 集合
                .HasForeignKey(p => p.UserId)
                .WillCascadeOnDelete(false);

            // User (一) -> PostLike (多)
            modelBuilder.Entity<PostLike>()
                .HasRequired(pl => pl.User)
                .WithMany(u => u.PostLikes) // <== 明確指定 User.cs 中的 PostLikes 集合
                .HasForeignKey(pl => pl.UserId)
                .WillCascadeOnDelete(false); // 通常使用者不刪，即使刪了按讚紀錄也不應觸發連鎖刪除

            // User (一) -> TrialLike (多)
            modelBuilder.Entity<TrialLike>()
                .HasRequired(tl => tl.User)
                .WithMany(u => u.TrialLikes) // <== 明確指定 User.cs 中的 TrialLikes 集合
                .HasForeignKey(tl => tl.UserId)
                .WillCascadeOnDelete(false);

            // --- Avatar 相關關聯 ---
            // Avatar (一) -> UserAvatar (多) (Join Table)
            modelBuilder.Entity<UserAvatar>()
                .HasRequired(ua => ua.Avatar)
                .WithMany(a => a.UserAvatars) // <== 明確指定 Avatar.cs 中的 UserAvatars 集合
                .HasForeignKey(ua => ua.AvatarId)
                .WillCascadeOnDelete(false);

            // --- TrialTemplate 相關關聯 ---
            // TrialTemplate (一) -> Stage (多)
            modelBuilder.Entity<Stage>()
                .HasRequired(s => s.TrialTemplate)
                .WithMany(tt => tt.Stages) // <== 明確指定 TrialTemplate.cs 中的 Stages 集合
                .HasForeignKey(s => s.TrialTemplateId)
                .WillCascadeOnDelete(false);

            // TrialTemplate (一) -> Trial (多)
            modelBuilder.Entity<Trial>()
                .HasRequired(t => t.TrialTemplate)
                .WithMany(tt => tt.Trials) // <== 明確指定 TrialTemplate.cs 中的 Trials 集合
                .HasForeignKey(t => t.TrialTemplateId)
                .WillCascadeOnDelete(false);

            // --- Stage 相關關聯 ---
            // Stage (一) -> UserStage (多)
            modelBuilder.Entity<UserStage>()
                .HasRequired(us => us.Stage)
                .WithMany(s => s.UserStages) // <== 明確指定 Stage.cs 中的 UserStages 集合
                .HasForeignKey(us => us.StageId)
                .WillCascadeOnDelete(false);

            // --- Trial 相關關聯 ---
            // Trial (一) -> UserStage (多)
            modelBuilder.Entity<UserStage>()
                .HasRequired(us => us.Trial)
                .WithMany(t => t.UserStages) // <== 明確指定 Trial.cs 中的 UserStages 集合
                .HasForeignKey(us => us.TrialId)
                .WillCascadeOnDelete(false);

            // Trial (一) -> Post (多)
            modelBuilder.Entity<Post>()
                .HasRequired(p => p.Trial)
                .WithMany(t => t.Posts) // <== 明確指定 Trial.cs 中的 Posts 集合
                .HasForeignKey(p => p.TrialId)
                .WillCascadeOnDelete(false);

            // Trial (一) -> TrialLike (多)
            modelBuilder.Entity<TrialLike>()
                .HasRequired(tl => tl.Trial)
                .WithMany(t => t.TrialLikes) // <== 明確指定 Trial.cs 中的 TrialLikes 集合
                .HasForeignKey(tl => tl.TrialId)
                .WillCascadeOnDelete(true); // Trial 刪除時，相關的按讚紀錄應一起刪除

            // --- UserStage 相關關聯 ---
            // UserStage (Principal, 1) -> Post (Dependent, 0..1)
            modelBuilder.Entity<Post>()
                .HasRequired(post => post.UserStage) // 一個 Post 必須有一個 UserStage
                .WithOptional(userStage => userStage.Post); // 一個 UserStage 可以沒有 Post (也可以有一個)

            // --- Post 相關關聯 ---
            // Post (一) -> PostLike (多)
            modelBuilder.Entity<PostLike>()
                .HasRequired(pl => pl.Post)
                .WithMany(p => p.PostLikes) // <== 明確指定 Post.cs 中的 PostLikes 集合
                .HasForeignKey(pl => pl.PostId)
                .WillCascadeOnDelete(true); // Post 刪除時，相關的按讚紀錄應一起刪除

            // --- Achievement & UserAchievement (多對多) ---
            modelBuilder.Entity<UserAchievement>()
                .HasRequired(ua => ua.User)
                .WithMany(u => u.UserAchievements)
                .HasForeignKey(ua => ua.UserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<UserAchievement>()
                .HasRequired(ua => ua.Achievement)
                .WithMany(a => a.UserAchievements)
                .HasForeignKey(ua => ua.AchievementId)
                .WillCascadeOnDelete(false);

            // --- User & UserTrialTemplate (多對多) ---
            modelBuilder.Entity<UserTrialTemplate>()
                .HasRequired(utt => utt.User)
                .WithMany(u => u.UserTrialTemplates)
                .HasForeignKey(utt => utt.UserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<UserTrialTemplate>()
                .HasRequired(utt => utt.TrialTemplate)
                .WithMany(tt => tt.UserTrialTemplates)
                .HasForeignKey(utt => utt.TrialTemplateId)
                .WillCascadeOnDelete(false);

            // --- Transaction Tables ---
            modelBuilder.Entity<BagelTransaction>()
                .HasRequired(bt => bt.User)
                .WithMany(u => u.BagelTransactions)
                .HasForeignKey(bt => bt.UserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PaymentTransaction>()
                .HasRequired(pt => pt.User)
                .WithMany(u => u.PaymentTransactions)
                .HasForeignKey(pt => pt.UserId)
                .WillCascadeOnDelete(false);

            // --- FriendsRelationship (兩個指向 User 的 FK) ---
            // 1. 發送邀請的使用者
            modelBuilder.Entity<FriendsRelationship>()
                .HasRequired(fr => fr.User)
                .WithMany(u => u.SendFriends)
                .HasForeignKey(fr => fr.UserId)
                .WillCascadeOnDelete(false);

            // 2. 被邀請的使用者
            modelBuilder.Entity<FriendsRelationship>()
                .HasRequired(fr => fr.Invitee)
                .WithMany(u => u.ReceivedFriends)
                .HasForeignKey(fr => fr.InviteeId)
                .WillCascadeOnDelete(false);

            // --- TrialParticipant (兩個指向 User 的 FK) ---
            // 1. 發送邀請的參與者
            modelBuilder.Entity<TrialParticipant>()
                .HasRequired(tp => tp.Participant)
                .WithMany(u => u.SendInviters)
                .HasForeignKey(tp => tp.ParticipantId)
                .WillCascadeOnDelete(false);

            // 2. 被邀請者
            modelBuilder.Entity<TrialParticipant>()
                .HasRequired(tp => tp.Invitee)
                .WithMany(u => u.Invitees)
                .HasForeignKey(tp => tp.InviteeId)
                .WillCascadeOnDelete(false);

            // Trial (一) -> TrialParticipant (多)
            modelBuilder.Entity<TrialParticipant>()
                .HasRequired(tp => tp.Trial)
                .WithMany(t => t.TrialParticipants)
                .HasForeignKey(tp => tp.TrialId)
                .WillCascadeOnDelete(true); // 試煉刪除時，參與者紀錄應一起刪除
        }
    }
}