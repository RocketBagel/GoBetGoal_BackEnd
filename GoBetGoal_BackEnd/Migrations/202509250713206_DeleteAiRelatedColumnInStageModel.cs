namespace GoBetGoal_BackEnd.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DeleteAiRelatedColumnInStageModel : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Stages", "VerificationMode");
            DropColumn("dbo.Stages", "ChallengeType");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Stages", "ChallengeType", c => c.String(maxLength: 50));
            AddColumn("dbo.Stages", "VerificationMode", c => c.String(maxLength: 50));
        }
    }
}
