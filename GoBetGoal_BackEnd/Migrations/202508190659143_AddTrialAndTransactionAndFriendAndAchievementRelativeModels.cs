namespace GoBetGoal_BackEnd.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTrialAndTransactionAndFriendAndAchievementRelativeModels : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Achievements",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AchievementTitle = c.Int(nullable: false),
                        AchievementImagePath = c.String(nullable: false, maxLength: 200),
                        AchievementDescription = c.String(nullable: false, maxLength: 200),
                        SortOrder = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.UserAchievements",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Guid(nullable: false),
                        AchievementId = c.Int(nullable: false),
                        AcquiredAt = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Achievements", t => t.AchievementId)
                .ForeignKey("dbo.Users", t => t.UserId)
                .Index(t => t.UserId)
                .Index(t => t.AchievementId);
            
            CreateTable(
                "dbo.BagelTransactions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Guid(nullable: false),
                        TransactionType = c.Int(nullable: false),
                        ProductType = c.Int(nullable: false),
                        ReferenceId = c.Int(),
                        ItemName = c.String(nullable: false),
                        Price = c.Int(nullable: false),
                        Quantity = c.Int(nullable: false),
                        Amount = c.Int(nullable: false),
                        BalanceBefore = c.Int(nullable: false),
                        BalanceAfter = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.TrialParticipants",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TrialId = c.Int(nullable: false),
                        ParticipantId = c.Guid(nullable: false),
                        InviteeId = c.Guid(nullable: false),
                        Status = c.Int(nullable: false),
                        InviteAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.InviteeId)
                .ForeignKey("dbo.Users", t => t.ParticipantId)
                .ForeignKey("dbo.Trials", t => t.TrialId, cascadeDelete: true)
                .Index(t => t.TrialId)
                .Index(t => t.ParticipantId)
                .Index(t => t.InviteeId);
            
            CreateTable(
                "dbo.UserTrialTemplates",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Guid(nullable: false),
                        TrialTemplateId = c.Int(nullable: false),
                        AcquiredAt = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.TrialTemplates", t => t.TrialTemplateId)
                .ForeignKey("dbo.Users", t => t.UserId)
                .Index(t => t.UserId)
                .Index(t => t.TrialTemplateId);
            
            CreateTable(
                "dbo.PaymentTransactions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Guid(nullable: false),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Method = c.Int(nullable: false),
                        Status = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.FriendsRelationships",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Guid(nullable: false),
                        InviteeId = c.Guid(nullable: false),
                        Note = c.String(maxLength: 100),
                        Status = c.Int(nullable: false),
                        InviteAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.InviteeId)
                .ForeignKey("dbo.Users", t => t.UserId)
                .Index(t => t.UserId)
                .Index(t => t.InviteeId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserAchievements", "UserId", "dbo.Users");
            DropForeignKey("dbo.FriendsRelationships", "UserId", "dbo.Users");
            DropForeignKey("dbo.FriendsRelationships", "InviteeId", "dbo.Users");
            DropForeignKey("dbo.PaymentTransactions", "UserId", "dbo.Users");
            DropForeignKey("dbo.TrialParticipants", "TrialId", "dbo.Trials");
            DropForeignKey("dbo.UserTrialTemplates", "UserId", "dbo.Users");
            DropForeignKey("dbo.UserTrialTemplates", "TrialTemplateId", "dbo.TrialTemplates");
            DropForeignKey("dbo.TrialParticipants", "ParticipantId", "dbo.Users");
            DropForeignKey("dbo.TrialParticipants", "InviteeId", "dbo.Users");
            DropForeignKey("dbo.BagelTransactions", "UserId", "dbo.Users");
            DropForeignKey("dbo.UserAchievements", "AchievementId", "dbo.Achievements");
            DropIndex("dbo.FriendsRelationships", new[] { "InviteeId" });
            DropIndex("dbo.FriendsRelationships", new[] { "UserId" });
            DropIndex("dbo.PaymentTransactions", new[] { "UserId" });
            DropIndex("dbo.UserTrialTemplates", new[] { "TrialTemplateId" });
            DropIndex("dbo.UserTrialTemplates", new[] { "UserId" });
            DropIndex("dbo.TrialParticipants", new[] { "InviteeId" });
            DropIndex("dbo.TrialParticipants", new[] { "ParticipantId" });
            DropIndex("dbo.TrialParticipants", new[] { "TrialId" });
            DropIndex("dbo.BagelTransactions", new[] { "UserId" });
            DropIndex("dbo.UserAchievements", new[] { "AchievementId" });
            DropIndex("dbo.UserAchievements", new[] { "UserId" });
            DropTable("dbo.FriendsRelationships");
            DropTable("dbo.PaymentTransactions");
            DropTable("dbo.UserTrialTemplates");
            DropTable("dbo.TrialParticipants");
            DropTable("dbo.BagelTransactions");
            DropTable("dbo.UserAchievements");
            DropTable("dbo.Achievements");
        }
    }
}
