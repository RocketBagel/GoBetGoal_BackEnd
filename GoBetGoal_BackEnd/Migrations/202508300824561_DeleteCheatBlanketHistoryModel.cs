namespace GoBetGoal_BackEnd.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DeleteCheatBlanketHistoryModel : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.CheatBlanketHistories", "UserId", "dbo.Users");
            DropForeignKey("dbo.CheatBlanketHistories", "UserStageId", "dbo.UserStages");
            DropIndex("dbo.CheatBlanketHistories", new[] { "UserId" });
            DropIndex("dbo.CheatBlanketHistories", new[] { "UserStageId" });
            DropColumn("dbo.UserStages", "IsCheat");
            DropTable("dbo.CheatBlanketHistories");
        }
        
        public override void Down()
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
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.UserStages", "IsCheat", c => c.Boolean(nullable: false));
            CreateIndex("dbo.CheatBlanketHistories", "UserStageId");
            CreateIndex("dbo.CheatBlanketHistories", "UserId");
            AddForeignKey("dbo.CheatBlanketHistories", "UserStageId", "dbo.UserStages", "Id");
            AddForeignKey("dbo.CheatBlanketHistories", "UserId", "dbo.Users", "Id");
        }
    }
}
