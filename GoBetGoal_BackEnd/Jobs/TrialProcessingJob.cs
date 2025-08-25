using GoBetGoal_BackEnd.Enums;
using GoBetGoal_BackEnd.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace GoBetGoal_BackEnd.Jobs
{
    public class TrialProcessingJob
    {
        // 這是 Hangfire 要呼叫的主方法
        public void CheckSystemStatus()
        {
            Console.WriteLine("Hangfire Job: Starting system status check...");

            // 將兩個主要邏輯分開處理，讓程式碼更清晰
            ProcessPendingTrials();
            ProcessOngoingTrials();

            Console.WriteLine("Hangfire Job: System status check finished.");
        }

        /// <summary>
        /// 處理待開始的試煉，將其狀態從 Pending 改為 Ongoing
        /// </summary>
        private void ProcessPendingTrials()
        {
            using (var dbContext = new Context()) // !!! 替換成您的 DbContext !!!
            {
                var now = DateTime.Now;

                // 1. 找出所有狀態為 Pending 且開始時間已到的試煉
                var trialsToStart = dbContext.Trials
                    .Where(t => t.TrialStatus == Status.pending && now >= t.StartTime)
                    .ToList();

                if (!trialsToStart.Any()) return;

                Console.WriteLine($"Found {trialsToStart.Count} trials to start.");

                foreach (var trial in trialsToStart)
                {
                    // 2. 更新狀態為 Ongoing
                    trial.TrialStatus = Status.ongoing;
                }

                // 3. 儲存變更
                dbContext.SaveChanges();
            }
        }
        /// <summary>
        /// 處理進行中的試煉，在結束時進行結算
        /// </summary>
        private void ProcessOngoingTrials()
        {
            using (var dbContext = new Context()) // !!! 替換成您的 DbContext !!!
            {
                var now = DateTime.Now;

                // 1. 找出所有狀態為 Ongoing 且結束時間已到的試煉
                //    同時使用 .Include() 載入相關的 UserStages 和 TrialParticipants，避免 N+1 查詢問題，效能更好
                var trialsToEnd = dbContext.Trials
                    .Include(t => t.UserStages)
                    .Include(t => t.TrialParticipants)
                    .Where(t => t.TrialStatus == Status.ongoing && now >= t.EndTime)
                    .ToList();

                if (!trialsToEnd.Any()) return;

                Console.WriteLine($"Found {trialsToEnd.Count} trials to end and process.");

                foreach (var trial in trialsToEnd)
                {
                    // 對每一個結束的試煉，使用資料庫交易來確保所有操作都成功或都失敗
                    using (var transaction = dbContext.Database.BeginTransaction())
                    {
                        try
                        {
                            // --- 結算邏輯開始 ---

                            // 2. 更新逾期的 UserStage 狀態
                            foreach (var stage in trial.UserStages)
                            {
                                // 如果到試煉結束時，關卡狀態仍然是 Pending，則標記為失敗
                                if (stage.Status == Status.pending)
                                {
                                    stage.Status = Status.fail;
                                }
                            }
                            // 先儲存一次關卡狀態的變更
                            dbContext.SaveChanges();


                            // 3. 為每個參與者計算成功率、獎勵 Bagel、授予成就
                            foreach (var participant in trial.TrialParticipants)
                            {
                                var user = dbContext.Users.Find(participant.UserId);
                                if (user == null) continue;

                                // 找到這位使用者在此次試煉中的所有關卡
                                var userStagesInThisTrial = trial.UserStages.Where(us => us.UserId == user.Id).ToList();

                                if (userStagesInThisTrial.Any())
                                {
                                    // 計算成功率
                                    double successfulStages = userStagesInThisTrial.Count(us => us.Status == Status.Success);
                                    double totalStages = userStagesInThisTrial.Count;
                                    double successRate = totalStages > 0 ? successfulStages / totalStages : 0;

                                    // 根據成功率獎勵或扣除 Bagel
                                    if (successRate >= 0.8) // 成功率 80% 以上
                                    {
                                        user.BagelCount += 10; // 假設獎勵 10 個
                                    }
                                    else if (successRate < 0.5) // 成功率低於 50%
                                    {
                                        user.BagelCount -= 5; // 假設扣除 5 個
                                    }
                                }

                                // 授予「首次完成試煉」徽章的邏輯
                                // 檢查這位使用者是否「首次」成功完成「任何」試煉
                                bool hasCompletedTrialBefore = dbContext.Trials
                                    .Any(t => t.TrialParticipants.Any(p => p.UserId == user.Id) && t.TrialStatus == Status.Completed);

                                if (!hasCompletedTrialBefore)
                                {
                                    // 假設 AchievementId = 1 是「首次完成試煉」
                                    var newAchievement = new UserAchievement { UserId = user.Id, AchievementId = 1, CreatedAt = now };
                                    dbContext.UserAchievements.Add(newAchievement);
                                }
                            }

                            // 4. 更新試煉本身的最終狀態
                            trial.TrialStatus = Status.Completed; // 將試煉標記為已結算

                            // 5. 儲存所有變更
                            dbContext.SaveChanges();

                            // 6. 提交交易
                            transaction.Commit();
                            Console.WriteLine($"Successfully processed Trial ID: {trial.Id}");
                        }
                        catch (Exception ex)
                        {
                            // 如果中間發生任何錯誤，就回滾所有變更
                            transaction.Rollback();
                            Console.WriteLine($"Failed to process Trial ID: {trial.Id}. Error: {ex.Message}");
                        }
                    }
                }
            }
        }
    }
}