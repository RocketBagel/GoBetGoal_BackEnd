namespace GoBetGoal_BackEnd.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveUserStageNavigationFromPost : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Posts", "Id", "dbo.UserStages");
            DropIndex("dbo.Posts", new[] { "Id" });
            AddColumn("dbo.UserStages", "Post_Id", c => c.Int());
            CreateIndex("dbo.UserStages", "Post_Id");
            AddForeignKey("dbo.UserStages", "Post_Id", "dbo.Posts", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserStages", "Post_Id", "dbo.Posts");
            DropIndex("dbo.UserStages", new[] { "Post_Id" });
            DropColumn("dbo.UserStages", "Post_Id");
            CreateIndex("dbo.Posts", "Id");
            AddForeignKey("dbo.Posts", "Id", "dbo.UserStages", "Id");
        }
    }
}
