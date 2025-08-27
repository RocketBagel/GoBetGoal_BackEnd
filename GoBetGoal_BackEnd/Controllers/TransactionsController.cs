using GoBetGoal_BackEnd.Enums;
using GoBetGoal_BackEnd.Models;
using GoBetGoal_BackEnd.Models.DTOs;
using GoBetGoal_BackEnd.Security;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;

namespace GoBetGoal_BackEnd.Controllers
{
    public class TransactionsController : ApiController
    {
        private readonly Context _context = new Context();

        [HttpPost]
        [Route("api/transactions/purchaseItem")]
        public IHttpActionResult PurchaseItem([FromBody] PurchaseRequestDto request)
        {
            if (request == null)
            {
                return BadRequest("Invalid request.");
            }

            var user = _context.Users.SingleOrDefault(u => u.Id == request.UserId);
            if (user == null)
            {
                return NotFound();
            }

            string Itemquantity = request.ItemName.Contains("*") ? request.ItemName.Split('*')[1] : "";
            int.TryParse(Itemquantity, out var quantity);

            if (request.Price <= 0 || quantity <= 0)
            {
                return BadRequest("Invalid price or quantity.");
            }

            var totalCost = request.Price * quantity;

            // 檢查餘額
            if (user.BagelCount < totalCost)
                return BadRequest("貝果數量不足");

            var balanceBefore = user.BagelCount;
            var balanceAfter = balanceBefore - totalCost;

            // 建立交易紀錄
            var transaction = new BagelTransaction
            {
                UserId = user.Id,
                TransactionType = TransactionType.購買道具,
                ProductType = request.ItemType,
                ReferenceId = int.TryParse(request.ItemId, out var refId) ? (int?)refId : null,   // CheatBlanket 可為 null
                ItemName = request.ItemName,
                Price = request.Price,
                Quantity = quantity,
                Amount = totalCost,
                BalanceBefore = balanceBefore,
                BalanceAfter = balanceAfter,
                CreatedAt = DateTime.Now
            };
            _context.BagelTransactions.Add(transaction);

            // 更新貝果總數
            user.BagelCount = balanceAfter;

            // 更新遮羞布數量
            if (request.ItemType == ProductType.CheatBlanket)
            {
                user.CheatBlanketCount += quantity;
            }


            user.UpdatedAt = DateTime.Now;

            _context.SaveChanges();

            return Ok(new
            {
                message = "Purchase successful",
                userId = user.Id,
                balance = user.BagelCount,
                cheatBlanketCount = user.CheatBlanketCount
            });
        }
    }
}
