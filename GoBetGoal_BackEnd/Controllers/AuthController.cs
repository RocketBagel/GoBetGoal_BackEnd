using GoBetGoal_BackEnd.Models;
using GoBetGoal_BackEnd.Models.DTOs;
using GoBetGoal_BackEnd.Security;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Web.Http;

namespace GoBetGoal_BackEnd.Controllers
{
    /// <summary>
    /// 處理所有與使用者認證相關的 API，例如註冊與登入。
    /// </summary>
    public class AuthController : ApiController
    {
        // 準備資料庫連線
        private readonly Context _db = new Context();

        /// <summary>
        /// 註冊流程 - 步驟一：建立核心帳號
        /// </summary>
        /// <param name="model">使用者提供的 Email 和密碼</param>
        /// <returns>成功後回傳包含 JWT 的物件</returns>
        [HttpPost]
        [Route("api/auth/register")]
        public IHttpActionResult RegisterStepOne(RegisterStepOneRequestDto model)
        {
            //// 驗證傳入的資料是否符合 DTO 的規則 (例如 [Required], [EmailAddress])
            //if (!ModelState.IsValid)
            //{
            //    // 如果驗證失敗，回傳 400 Bad Request 並附上錯誤訊息
            //    return BadRequest(ModelState);
            //}

            // ---新增：將 Email 正規化為小寫-- -
            // ToLowerInvariant() 是用於處理非顯示用的、程式內部比較的標準方法
            string normalizedEmail = model.Email.ToLowerInvariant();

            // 檢查 Email 是否已經被註冊
            bool IsEmailTaken = _db.Users.Any(u => u.Email == normalizedEmail);
            if (IsEmailTaken)
            {
                var error = new ErrorResponseDto
                {
                    ErrorCode = "EMAIL_ALREADY_EXISTS",
                    Message = "此 Email 已被註冊，請嘗試使用其他 Email 或登入現有帳號。" // 是否直接改成網頁跳出來的紅色警告訊息?
                };

                // 如果 Email 已存在，回傳 409 Conflict，並附上易於理解的錯誤訊息
                return Content(HttpStatusCode.Conflict, error);
            }

            // 後端自動生成一個保證唯一的 PlayerId
            string newPlayerId = GenerateUniquePlayerId(normalizedEmail);

            // 建立一個新的 User 物件，準備存入資料庫
            var newUser = new User
            {
                Email = normalizedEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                PlayerId = newPlayerId,
                NickName = newPlayerId,
                UserAvatars = new List<UserAvatar>() // 初始化集合
            };

            // 注意：這裡先不要 Add 到 _db，讓我們先建立好完整的物件圖 (Object Graph)

            // 找出所有免費的頭像
            var freeAvatars = _db.Avatars.Where(u => u.AvatarPrice == 0 && u.IsActive).ToList();

            // 為每一個免費頭像，建立關聯紀錄
            foreach (var avatar in freeAvatars)
            {
                newUser.UserAvatars.Add(new UserAvatar
                {
                    // 不需要再手動設定 UserId！
                    // User = newUser, // 也可以這樣寫，EF 都看得懂
                    AvatarId = avatar.Id,
                });

            }

            _db.Users.Add(newUser);


            try
            {
                // 4. 將所有變更 (新增 User 和 新增多筆 UserAvatar) 在一個交易中，一次性存入資料庫
                _db.SaveChanges();
            }
            // 捕捉 Entity Framework 在儲存前的模型驗證錯誤
            //catch (DbEntityValidationException ex)
            //{
            //    // 這可以捕捉到更詳細的 Model 層驗證錯誤，對於偵錯非常有幫助
            //    // (這個錯誤會被下面的 GlobalExceptionFilter 捕捉到並記錄)
            //    return InternalServerError(ex);
            //}
            // 捕捉所有其他可能的例外狀況
            catch (Exception ex)
            {
                // 這裡的錯誤會被您的 GlobalExceptionFilter 捕捉到
                // 所以 InternalServerError() 主要是為了中斷執行
                // 您也可以在這裡手動記錄 Log
                return InternalServerError(ex);
            }



            // 註冊成功後，立即產生 JWT，讓前端可以無縫進行下一步
            var jwtAuthUtil = new JwtAuthUtility();
            // 呼叫您寫好的工具來產生 Token
            string token = jwtAuthUtil.GenerateToken(newUser);

            // 3 個月大約的總秒數
            long totalSeconds = (long)TimeSpan.FromDays(90).TotalSeconds;

            return Ok(new AuthSuccessResponseDto
            {
                Message = "帳號建立成功，請繼續完善資料",
                UserId = newUser.Id,
                Token = token,
                ExpiresIn = totalSeconds // 權杖有效期限 (3個月)

            });
        }

        [HttpPost]
        [Route("api/auth/login")]
        public IHttpActionResult Login(LoginRequestDto model)
        {
            var lowerCaseEmail = model.Email.ToLowerInvariant();

            User user = _db.Users.FirstOrDefault(a => a.Email == lowerCaseEmail);
            if (user == null)
            {
                var error = new ErrorResponseDto
                {
                    ErrorCode = "INVALID_CREDENTIALS",
                    Message = "Email 或密碼錯誤，請檢查後重試。"
                };
                return Content(HttpStatusCode.Unauthorized, error);
            }
            else
            {
                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash);
                if (!isPasswordValid)
                {
                    var error = new ErrorResponseDto
                    {
                        ErrorCode = "INVALID_CREDENTIALS",
                        Message = "Email 或密碼錯誤，請檢查後重試。"
                    };
                    return Content(HttpStatusCode.Unauthorized, error);
                }
            }

            var jwtAuthUtil = new JwtAuthUtility();
            // 呼叫您寫好的工具來產生 Token
            string token = jwtAuthUtil.GenerateToken(user);

            // 3 個月大約的總秒數
            long totalSeconds = (long)TimeSpan.FromDays(90).TotalSeconds;

            return Ok(new AuthSuccessResponseDto
            {
                Message = "帳號登入成功，歡迎回來!",
                UserId = user.Id,
                Token = token,
                ExpiresIn = totalSeconds // 權杖有效期限 (3個月)

            });
        }

        // --- 新增方法開始 ---

        /// <summary>
        /// 生成一個基於 Email 且保證在資料庫中唯一的 PlayerId。
        /// </summary>
        /// <remarks>
        /// 長遠來看，這個方法適合放在一個獨立的 Helper/Utility 類別中，以利於程式碼重用和測試。
        /// </remarks>
        /// <param name="email">使用者的 Email。</param>
        /// <returns>一個唯一的 PlayerId 字串。</returns>
        private string GenerateUniquePlayerId(string email)
        {
            string newPlayerId;
            bool isUnique;

            // 提取 Email 前綴
            string prefix = ExtractPrefixFromEmail(email);

            do
            {
                // 生成短 GUID (前 5 個字元)
                string shortGuid = Guid.NewGuid().ToString("N").Substring(0, 5);
                newPlayerId = $"{prefix}_{shortGuid}";

                // 檢查資料庫中此 ID 是否已存在
                isUnique = !_db.Users.Any(u => u.PlayerId == newPlayerId);

            } while (!isUnique); // 如果不唯一，就重新循環一次

            return newPlayerId;
        }

        /// <summary>
        /// 從 Email 中提取 2-4 個字母作為 PlayerId 的前綴。
        /// </summary>
        private string ExtractPrefixFromEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
                return "player"; // 預設值

            string username = email.Split('@')[0];
            string letters = string.Concat(username.Where(char.IsLetter)).ToLower();

            if (letters.Length < 2)
            {
                return "gbg";
            }

            int lengthToTake = Math.Min(letters.Length, 4);
            return letters.Substring(0, lengthToTake);
        }



        // 釋放資料庫連線資源
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}