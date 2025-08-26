using GoBetGoal_BackEnd.Enums;
using GoBetGoal_BackEnd.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Linq;
using System.Net;
using System.Text;


namespace GoBetGoal_BackEnd.Services
{
    public class CreateTrialsService
    {
        private readonly Context _context;

        // Service 層內部定義 NotFoundException
        public class NotFoundException : Exception
        {
            public NotFoundException(string message) : base(message) { }
        }


        public CreateTrialsService(Context context)
        {
            _context = context;
        }

        //建立Trial >> TrialParticipant >> BagelTransaction >> User貝果總數更新
        public Trial CreateTrialRelatedLogic(Guid userId, int templateId, string title, DateTime startTime, int deposit)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                
                    var template = _context.TrialTemplates.FirstOrDefault(t => t.Id == templateId);
                    if (template == null) 
                    {
                        throw new NotFoundException("TrialTemplate 不存在");
                    }

                    // 檢查使用者是否已解鎖該模板
                    var hasTemplate = _context.UserTrialTemplates
                        .Any(ut => ut.UserId == userId && ut.TrialTemplateId == templateId);

                    if (!hasTemplate)
                    {
                        throw new InvalidOperationException("該模板尚未購買解鎖");
                    }

                    // 計算 EndTime = StartTime + (StageCount * TrialFrequency)天
                    var totalDays = template.StageCount * template.TrialFrequency;
                    var endTime = startTime.Date.AddDays(totalDays);

                    // 建立 Trial
                    var trial = new Trial
                    {
                        UserId = userId,
                        TrialTemplateId = templateId,
                        TrialName = title,
                        TrialDeposit = deposit,
                        StartTime = startTime.Date,
                        EndTime = endTime,
                        TrialStatus = Status.pending,
                        CreatedAt = DateTime.Now
                    };

                    _context.Trials.Add(trial);
                    _context.SaveChanges();
                    // 建立 TrialParticipant
                    var participant = new TrialParticipant
                    {
                        TrialId = trial.Id,
                        ParticipantId = userId,
                        InviteeId = userId,
                        InviteAt = DateTime.Now,
                        Status = Status.accepted  // 因為創建者自己加入，必為 accepted
                    };

                    var user = _context.Users.FirstOrDefault(u => u.Id == userId);
                    if (user == null) 
                    {
                        throw new NotFoundException("User 不存在");
                    }

                    int balanceBefore = user.BagelCount;
                    int balanceAfter = balanceBefore - deposit;
                    if (balanceAfter < 0)
                    {
                        throw new Exception("押金貝果數不足");
                    }

                    user.BagelCount = balanceAfter;
                    user.UpdatedAt = DateTime.Now;

                    var bageldata = new BagelTransaction
                    {
                        User = user,
                        TransactionType = TransactionType.試煉收付,
                        ProductType = ProductType.Bagel,
                        ReferenceId = trial.Id,
                        ItemName = trial.TrialName,
                        Price = deposit,
                        Quantity = 1,
                        Amount = deposit * 1,
                        BalanceBefore = balanceBefore,
                        BalanceAfter = balanceAfter,
                        CreatedAt = DateTime.Now
                    };

            
                    _context.TrialParticipants.Add(participant);
                    _context.BagelTransactions.Add(bageldata);
                    

                    // << 新增 UserStage >>
                    // 根據模板的所有 Stage 建立 UserStage 紀錄 (目前只有創建者)
                    var stages = _context.Stages.Where(s => s.TrialTemplateId == templateId).ToList();

                    foreach (var stage in stages)
                    {
                        var userStage = new UserStage
                        {
                            UserId = userId,
                            TrialId = trial.Id,
                            StageId = stage.Id,
                            ChanceRemain = 3,
                            StartTime = startTime,
                            EndTime = endTime,
                            IsCheat = false,
                            Status = Status.pending,
                            CreatedAt = DateTime.Now
                        };
                        _context.UserStages.Add(userStage);
                    }

                    _context.SaveChanges();


                    // 全部成功才提交
                    transaction.Commit();
                    return trial;
                }
                catch
                {
                    // 失敗就回滾
                    transaction.Rollback();
                    throw;
                }
            }
        }
    }
}
