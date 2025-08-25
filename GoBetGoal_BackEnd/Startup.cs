using GoBetGoal_BackEnd.Jobs;
using GoBetGoal_BackEnd.Models;
using Hangfire;
using Microsoft.Owin;
using Owin;
using System;
using System.Linq;
using System.Threading.Tasks;

[assembly: OwinStartup(typeof(GoBetGoal_BackEnd.Startup))]

namespace GoBetGoal_BackEnd
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // 如需如何設定應用程式的詳細資訊，請瀏覽 https://go.microsoft.com/fwlink/?LinkID=316888
            // --- Hangfire 設定開始 ---

            // 1. 設定儲存方式
            // !!! 請將 "DefaultConnection" 換成您在步驟 2 中記下的連線字串 name !!!
            GlobalConfiguration.Configuration.UseSqlServerStorage("Context");

            // 2. 啟動 Hangfire 儀表板 (Dashboard)
            // 之後您可以透過 http://您的網址/hangfire 來訪問
            app.UseHangfireDashboard();

            // 3. 啟動 Hangfire 伺服器，它會在背景處理任務
            app.UseHangfireServer();

            // 4. 註冊您的第一個重複性任務
            RecurringJob.AddOrUpdate(
                "system-status-check",                  // 任務的唯一 ID
                () => new TrialProcessingJob().CheckSystemStatus(), // 執行的目標方法
                Cron.Minutely()                         // 每分鐘執行一次
            );

            // --- Hangfire 設定結束 ---
        }
    }
}
