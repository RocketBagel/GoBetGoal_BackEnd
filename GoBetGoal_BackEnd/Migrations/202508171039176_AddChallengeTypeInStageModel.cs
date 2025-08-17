namespace GoBetGoal_BackEnd.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddChallengeTypeInStageModel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Stages", "ChallengeType", c => c.String(maxLength: 50));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Stages", "ChallengeType");
        }
    }
}
