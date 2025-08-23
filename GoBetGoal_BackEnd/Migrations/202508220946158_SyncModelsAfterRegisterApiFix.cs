namespace GoBetGoal_BackEnd.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SyncModelsAfterRegisterApiFix : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.PostLikes", "PostId", "dbo.Posts");
            DropIndex("dbo.Posts", new[] { "Id" });
            DropPrimaryKey("dbo.Posts");
            AlterColumn("dbo.Posts", "Id", c => c.Int(nullable: false, identity: true));
            AddPrimaryKey("dbo.Posts", "Id");
            CreateIndex("dbo.Posts", "Id");
            AddForeignKey("dbo.PostLikes", "PostId", "dbo.Posts", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PostLikes", "PostId", "dbo.Posts");
            DropIndex("dbo.Posts", new[] { "Id" });
            DropPrimaryKey("dbo.Posts");
            AlterColumn("dbo.Posts", "Id", c => c.Int(nullable: false));
            AddPrimaryKey("dbo.Posts", "Id");
            CreateIndex("dbo.Posts", "Id");
            AddForeignKey("dbo.PostLikes", "PostId", "dbo.Posts", "Id", cascadeDelete: true);
        }
    }
}
