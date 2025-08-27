namespace GoBetGoal_BackEnd.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemovePostModelUserStageIdRequired : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Posts", new[] { "UserStageId" });
            AlterColumn("dbo.Posts", "UserStageId", c => c.Int());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Posts", "UserStageId", c => c.Int(nullable: false));
            CreateIndex("dbo.Posts", "UserStageId");
        }
    }
}
