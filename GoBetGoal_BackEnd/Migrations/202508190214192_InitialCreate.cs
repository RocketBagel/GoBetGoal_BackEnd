namespace GoBetGoal_BackEnd.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Avatars",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AvatarImagePath = c.String(nullable: false, maxLength: 200),
                        AvatarPrice = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        SortOrder = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.UserAvatars",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Guid(nullable: false),
                        AvatarId = c.Int(nullable: false),
                        IsCurrent = c.Boolean(nullable: false),
                        AcquiredAt = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Avatars", t => t.AvatarId)
                .ForeignKey("dbo.Users", t => t.UserId)
                .Index(t => t.UserId)
                .Index(t => t.AvatarId);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Email = c.String(nullable: false, maxLength: 100),
                        PasswordHash = c.String(nullable: false, maxLength: 100),
                        PlayerId = c.String(nullable: false, maxLength: 100),
                        NickName = c.String(maxLength: 50),
                        BagelCount = c.Int(nullable: false),
                        CheatBlanketCount = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        UpdatedAt = c.DateTime(precision: 7, storeType: "datetime2"),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Email, unique: true, name: "IX_UserEmail")
                .Index(t => t.NickName, unique: true, name: "IX_UserNickName");
            
            CreateTable(
                "dbo.PostLikes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CreatedAt = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        PostId = c.Int(nullable: false),
                        UserId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Posts", t => t.PostId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserId)
                .Index(t => t.PostId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Posts",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CreatedAt = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        Content = c.String(),
                        UserId = c.Guid(nullable: false),
                        TrialId = c.Int(nullable: false),
                        ImageUrl = c.String(nullable: false),
                        UserStageId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Trials", t => t.TrialId)
                .ForeignKey("dbo.Users", t => t.UserId)
                .ForeignKey("dbo.UserStages", t => t.Id)
                .Index(t => t.Id)
                .Index(t => t.UserId)
                .Index(t => t.TrialId)
                .Index(t => t.UserStageId, unique: true);
            
            CreateTable(
                "dbo.Trials",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Guid(nullable: false),
                        TrialTemplateId = c.Int(nullable: false),
                        StartTime = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        EndTime = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        TrialDeposit = c.Int(nullable: false),
                        TrialName = c.String(nullable: false, maxLength: 100),
                        TrialStatus = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.TrialTemplates", t => t.TrialTemplateId)
                .ForeignKey("dbo.Users", t => t.UserId)
                .Index(t => t.UserId)
                .Index(t => t.TrialTemplateId);
            
            CreateTable(
                "dbo.TrialLikes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CreatedAt = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        TrialId = c.Int(nullable: false),
                        UserId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Trials", t => t.TrialId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserId)
                .Index(t => t.TrialId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.TrialTemplates",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TrialTitle = c.String(nullable: false, maxLength: 100),
                        TrialDescription = c.String(nullable: false, maxLength: 200),
                        TrialFrequency = c.Int(nullable: false),
                        TrialCategory = c.String(nullable: false, maxLength: 50),
                        TrialSuitFor = c.String(nullable: false, maxLength: 100),
                        TrialNoSuitFor = c.String(nullable: false, maxLength: 100),
                        TrialRule = c.String(nullable: false, maxLength: 200),
                        TrialCaution = c.String(nullable: false, maxLength: 200),
                        TrialEffect = c.String(nullable: false, maxLength: 200),
                        StageCount = c.Int(nullable: false),
                        MaxUser = c.Int(nullable: false),
                        IsAi = c.Boolean(nullable: false),
                        TrialTemplatePrice = c.Int(nullable: false),
                        CardImagePath = c.String(nullable: false, maxLength: 200),
                        CardColor = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Stages",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StageIndex = c.Int(nullable: false),
                        VerificationMode = c.String(maxLength: 50),
                        ChallengeType = c.String(maxLength: 50),
                        StageSampleImagePath = c.String(maxLength: 500),
                        StageDescription = c.String(nullable: false, maxLength: 300),
                        TrialTemplateId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.TrialTemplates", t => t.TrialTemplateId)
                .Index(t => t.TrialTemplateId);
            
            CreateTable(
                "dbo.UserStages",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Guid(nullable: false),
                        TrialId = c.Int(nullable: false),
                        StageId = c.Int(nullable: false),
                        UploadImagePath = c.String(maxLength: 500),
                        ImageUploadAt = c.DateTime(precision: 7, storeType: "datetime2"),
                        ChanceRemain = c.Int(nullable: false),
                        StartTime = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        EndTime = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        IsCheat = c.Boolean(nullable: false),
                        Status = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Stages", t => t.StageId)
                .ForeignKey("dbo.Trials", t => t.TrialId)
                .ForeignKey("dbo.Users", t => t.UserId)
                .Index(t => t.UserId)
                .Index(t => t.TrialId)
                .Index(t => t.StageId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserAvatars", "UserId", "dbo.Users");
            DropForeignKey("dbo.PostLikes", "UserId", "dbo.Users");
            DropForeignKey("dbo.PostLikes", "PostId", "dbo.Posts");
            DropForeignKey("dbo.Posts", "Id", "dbo.UserStages");
            DropForeignKey("dbo.Posts", "UserId", "dbo.Users");
            DropForeignKey("dbo.Posts", "TrialId", "dbo.Trials");
            DropForeignKey("dbo.Trials", "UserId", "dbo.Users");
            DropForeignKey("dbo.Trials", "TrialTemplateId", "dbo.TrialTemplates");
            DropForeignKey("dbo.UserStages", "UserId", "dbo.Users");
            DropForeignKey("dbo.UserStages", "TrialId", "dbo.Trials");
            DropForeignKey("dbo.UserStages", "StageId", "dbo.Stages");
            DropForeignKey("dbo.Stages", "TrialTemplateId", "dbo.TrialTemplates");
            DropForeignKey("dbo.TrialLikes", "UserId", "dbo.Users");
            DropForeignKey("dbo.TrialLikes", "TrialId", "dbo.Trials");
            DropForeignKey("dbo.UserAvatars", "AvatarId", "dbo.Avatars");
            DropIndex("dbo.UserStages", new[] { "StageId" });
            DropIndex("dbo.UserStages", new[] { "TrialId" });
            DropIndex("dbo.UserStages", new[] { "UserId" });
            DropIndex("dbo.Stages", new[] { "TrialTemplateId" });
            DropIndex("dbo.TrialLikes", new[] { "UserId" });
            DropIndex("dbo.TrialLikes", new[] { "TrialId" });
            DropIndex("dbo.Trials", new[] { "TrialTemplateId" });
            DropIndex("dbo.Trials", new[] { "UserId" });
            DropIndex("dbo.Posts", new[] { "UserStageId" });
            DropIndex("dbo.Posts", new[] { "TrialId" });
            DropIndex("dbo.Posts", new[] { "UserId" });
            DropIndex("dbo.Posts", new[] { "Id" });
            DropIndex("dbo.PostLikes", new[] { "UserId" });
            DropIndex("dbo.PostLikes", new[] { "PostId" });
            DropIndex("dbo.Users", "IX_UserNickName");
            DropIndex("dbo.Users", "IX_UserEmail");
            DropIndex("dbo.UserAvatars", new[] { "AvatarId" });
            DropIndex("dbo.UserAvatars", new[] { "UserId" });
            DropTable("dbo.UserStages");
            DropTable("dbo.Stages");
            DropTable("dbo.TrialTemplates");
            DropTable("dbo.TrialLikes");
            DropTable("dbo.Trials");
            DropTable("dbo.Posts");
            DropTable("dbo.PostLikes");
            DropTable("dbo.Users");
            DropTable("dbo.UserAvatars");
            DropTable("dbo.Avatars");
        }
    }
}
