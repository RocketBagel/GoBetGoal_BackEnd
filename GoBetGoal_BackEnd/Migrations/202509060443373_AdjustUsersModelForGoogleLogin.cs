namespace GoBetGoal_BackEnd.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AdjustUsersModelForGoogleLogin : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "GoogleId", c => c.String());
            AddColumn("dbo.Users", "GoogleName", c => c.String());
            AlterColumn("dbo.Users", "PasswordHash", c => c.String(maxLength: 100));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Users", "PasswordHash", c => c.String(nullable: false, maxLength: 100));
            DropColumn("dbo.Users", "GoogleName");
            DropColumn("dbo.Users", "GoogleId");
        }
    }
}
