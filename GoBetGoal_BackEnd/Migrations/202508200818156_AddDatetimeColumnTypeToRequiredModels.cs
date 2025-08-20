namespace GoBetGoal_BackEnd.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddDatetimeColumnTypeToRequiredModels : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Achievements", "AchievementTitle", c => c.String(nullable: false, maxLength: 200));
            AlterColumn("dbo.BagelTransactions", "CreatedAt", c => c.DateTime(nullable: false, precision: 7, storeType: "datetime2"));
            AlterColumn("dbo.TrialParticipants", "InviteAt", c => c.DateTime(nullable: false, precision: 7, storeType: "datetime2"));
            AlterColumn("dbo.TrialParticipants", "UpdatedAt", c => c.DateTime(precision: 7, storeType: "datetime2"));
            AlterColumn("dbo.PaymentTransactions", "CreatedAt", c => c.DateTime(nullable: false, precision: 7, storeType: "datetime2"));
            AlterColumn("dbo.FriendsRelationships", "InviteAt", c => c.DateTime(nullable: false, precision: 7, storeType: "datetime2"));
            AlterColumn("dbo.FriendsRelationships", "UpdatedAt", c => c.DateTime(precision: 7, storeType: "datetime2"));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.FriendsRelationships", "UpdatedAt", c => c.DateTime());
            AlterColumn("dbo.FriendsRelationships", "InviteAt", c => c.DateTime(nullable: false));
            AlterColumn("dbo.PaymentTransactions", "CreatedAt", c => c.DateTime(nullable: false));
            AlterColumn("dbo.TrialParticipants", "UpdatedAt", c => c.DateTime());
            AlterColumn("dbo.TrialParticipants", "InviteAt", c => c.DateTime(nullable: false));
            AlterColumn("dbo.BagelTransactions", "CreatedAt", c => c.DateTime(nullable: false));
            AlterColumn("dbo.Achievements", "AchievementTitle", c => c.Int(nullable: false));
        }
    }
}
