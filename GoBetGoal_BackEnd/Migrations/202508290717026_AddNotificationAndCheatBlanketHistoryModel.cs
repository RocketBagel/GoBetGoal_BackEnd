namespace GoBetGoal_BackEnd.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddNotificationAndCheatBlanketHistoryModel : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CheatBlanketHistories",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Guid(nullable: false),
                        UserStageId = c.Int(),
                        Quantity = c.Int(nullable: false),
                        BalanceBefore = c.Int(nullable: false),
                        BalanceAfter = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId)
                .ForeignKey("dbo.UserStages", t => t.UserStageId)
                .Index(t => t.UserId)
                .Index(t => t.UserStageId);
            
            CreateTable(
                "dbo.Notifications",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ReceiverId = c.Guid(nullable: false),
                        SenderId = c.Guid(),
                        NotificationType = c.Int(nullable: false),
                        Content = c.String(nullable: false, maxLength: 150),
                        IsRead = c.Boolean(nullable: false),
                        ReferenceId_Int = c.Int(),
                        ReferenceId_Guid = c.Guid(),
                        CreatedAt = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        UpdatedAt = c.DateTime(precision: 7, storeType: "datetime2"),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.ReceiverId)
                .ForeignKey("dbo.Users", t => t.SenderId)
                .Index(t => t.ReceiverId)
                .Index(t => t.SenderId);
            
            AddColumn("dbo.PaymentTransactions", "OrderNo", c => c.String(nullable: false, maxLength: 100));
            AddColumn("dbo.PaymentTransactions", "UpdatedAt", c => c.DateTime(precision: 7, storeType: "datetime2"));
            AlterColumn("dbo.BagelTransactions", "ItemName", c => c.String(nullable: false, maxLength: 100));
            AlterColumn("dbo.PaymentTransactions", "Status", c => c.String(nullable: false, maxLength: 50));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Notifications", "SenderId", "dbo.Users");
            DropForeignKey("dbo.Notifications", "ReceiverId", "dbo.Users");
            DropForeignKey("dbo.CheatBlanketHistories", "UserStageId", "dbo.UserStages");
            DropForeignKey("dbo.CheatBlanketHistories", "UserId", "dbo.Users");
            DropIndex("dbo.Notifications", new[] { "SenderId" });
            DropIndex("dbo.Notifications", new[] { "ReceiverId" });
            DropIndex("dbo.CheatBlanketHistories", new[] { "UserStageId" });
            DropIndex("dbo.CheatBlanketHistories", new[] { "UserId" });
            AlterColumn("dbo.PaymentTransactions", "Status", c => c.Int(nullable: false));
            AlterColumn("dbo.BagelTransactions", "ItemName", c => c.String(nullable: false));
            DropColumn("dbo.PaymentTransactions", "UpdatedAt");
            DropColumn("dbo.PaymentTransactions", "OrderNo");
            DropTable("dbo.Notifications");
            DropTable("dbo.CheatBlanketHistories");
        }
    }
}
