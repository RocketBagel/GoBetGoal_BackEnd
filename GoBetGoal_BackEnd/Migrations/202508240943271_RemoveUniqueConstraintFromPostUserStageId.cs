namespace GoBetGoal_BackEnd.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveUniqueConstraintFromPostUserStageId : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Posts", new[] { "UserStageId" });
            CreateIndex("dbo.Posts", "UserStageId");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Posts", new[] { "UserStageId" });
            CreateIndex("dbo.Posts", "UserStageId", unique: true);
        }
    }
}
