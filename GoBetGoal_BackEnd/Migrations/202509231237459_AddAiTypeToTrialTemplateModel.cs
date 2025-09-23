namespace GoBetGoal_BackEnd.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAiTypeToTrialTemplateModel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TrialTemplates", "AiType", c => c.String(maxLength: 50));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TrialTemplates", "AiType");
        }
    }
}
