namespace GoBetGoal_BackEnd.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateUserAchievementModel2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserAchievements", "TrialId", c => c.Int(nullable: true));
            CreateIndex("dbo.UserAchievements", "TrialId");
            AddForeignKey("dbo.UserAchievements", "TrialId", "dbo.Trials", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserAchievements", "TrialId", "dbo.Trials");
            DropIndex("dbo.UserAchievements", new[] { "TrialId" });
            DropColumn("dbo.UserAchievements", "TrialId");
        }
    }
}
