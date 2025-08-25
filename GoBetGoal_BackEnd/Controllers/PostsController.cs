using GoBetGoal_BackEnd.Enums;
using GoBetGoal_BackEnd.Models;
using GoBetGoal_BackEnd.Models.DTOs;
using GoBetGoal_BackEnd.Security;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;


namespace GoBetGoal_BackEnd.Controllers
{
    public class PostsController : BaseApiController
    {
        private readonly Context _db = new Context();

        /// <summary>
        /// API 1: 獲取所有貼文
        /// </summary>
        [HttpGet]
        [Route("api/posts/all")]
        [AllowAnonymous]
        public IHttpActionResult GetAllPosts()
        {
            // 步驟 0：取得當前檢視者 ID (用於後續計算 is_liked_by_viewer)
            Guid? viewerId = TryGetCurrentUserId();

            // 步驟 1：查詢資料庫，只撈取原始資料
            // 這個查詢中的所有操作，EF 都看得懂，可以順利翻譯成 SQL
            var rawPostsData = _db.Posts
                .Include(p => p.User.UserAvatars.Select(ua => ua.Avatar))
                .Include(p => p.Trial.TrialTemplate)
                .Include(p => p.PostLikes)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new // 使用一個暫時的匿名型別來存放原始資料
                {
                    // --- Post 本身的欄位 ---
                    PostObject = p,

                    // --- 關聯的 User 欄位 ---
                    UserNickName = p.User.NickName,
                    UserCurrentAvatar = p.User.UserAvatars.FirstOrDefault(ua => ua.IsCurrent).Avatar.AvatarImagePath,

                    // --- 關聯的 Trial/Template 欄位 ---
                    TrialName = p.Trial.TrialName,
                    TemplateTitle = p.Trial.TrialTemplate.TrialTitle,
                    TemplateCategoryJson = p.Trial.TrialTemplate.TrialCategory, // <-- 直接取出 JSON 字串

                    // --- 關聯的 PostLikes 欄位 ---
                    Likes = p.PostLikes.Select(pl => pl.UserId) // <-- 先取出按讚者的 UserID 列表
                })
                .ToList(); // ★★★ 關鍵！執行 .ToList()，讓查詢送出到資料庫，將結果拉回記憶體 ★★★

            // 步驟 2：在記憶體中處理資料，組合成最終的 DTO
            // rawPostsData 現在是一個 C# List，我們可以用任何 C# 方法來處理它
            var posts = rawPostsData.Select(p => new PostDto
            {
                Id = p.PostObject.Id,
                Content = p.PostObject.Content,
                CreatedAt = p.PostObject.CreatedAt,
                UserStageId = p.PostObject.UserStageId,
                UserId = p.PostObject.UserId,
                TrialId = p.PostObject.TrialId,

                // ★ 在這裡使用 JsonConvert，現在可以正常運作了 ★
                ImageUrl = JsonConvert.DeserializeObject<List<string>>(p.PostObject.ImageUrl ?? "[]"),

                Trial = new TrialInfoDto
                {
                    Title = p.TrialName,
                    Challenge = new ChallengeInfoDto
                    {
                        Title = p.TemplateTitle,
                        // ★ 在這裡使用 JsonConvert ★
                        Category = JsonConvert.DeserializeObject<List<string>>(p.TemplateCategoryJson ?? "[]")
                    }
                },
                UserInfo = new UserInfoDto
                {
                    NickName = p.UserNickName,
                    CharacterImgLink = p.UserCurrentAvatar
                },
                PostLike = p.Likes.Select(likeUserId => new PostLikeInfoDto { LikeBy = likeUserId }).ToList(),
                IsLikedByViewer = viewerId.HasValue && p.Likes.Contains(viewerId.Value)

            })
            .ToList();

            return Ok(posts);
        }


        [HttpGet]
        [Route("api/posts")]
        [AllowAnonymous]
        public IHttpActionResult GetPosts([FromUri] string filter = null)
        {
            Guid? viewerId = TryGetCurrentUserId();
            IQueryable<Post> query = _db.Posts.AsQueryable(); // 1. 建立一個基礎查詢，我們先不排序

            // --- 步驟 1：根據 filter 套用「篩選」條件 (WHERE) ---
            if (filter?.ToLower() == "following")
            {
                if (!viewerId.HasValue) { return Ok(new List<PostDto>()); }
                // a. 找出我的好友 ID 列表
                var friendIds = _db.FriendsRelationships
                                   .Where(f => (f.UserId == viewerId.Value || f.InviteeId == viewerId.Value) && f.Status == Status.accepted)
                                   .Select(f => f.UserId == viewerId.Value ? f.InviteeId : f.UserId)
                                   .ToList();

                // b. 找出「我按過讚」的貼文 ID 列表
                var myLikedPostIds = _db.PostLikes
                                        .Where(pl => pl.UserId == viewerId.Value)
                                        .Select(pl => pl.PostId)
                                        .ToList();

                // c. 找出「我好友發布」的貼文 ID 列表
                var postsByFriendsIds = _db.Posts
                                           .Where(p => friendIds.Contains(p.UserId))
                                           .Select(p => p.Id)
                                           .ToList();

                // d. 將兩組 ID 合併，並用 Union 去除重複項
                var finalPostIds = myLikedPostIds.Union(postsByFriendsIds).ToList();

                query = query.Where(p => finalPostIds.Contains(p.Id));
            }
            else if (filter?.ToLower() == "sports")
            {
                query = query.Where(p => p.Trial.TrialTemplate.TrialCategory.Contains("運動"));
            }
            else if (filter?.ToLower() == "diet")
            {
                query = query.Where(p => p.Trial.TrialTemplate.TrialCategory.Contains("飲食"));
            }
            else if (filter?.ToLower() == "lifestyle")
            {
                query = query.Where(p => p.Trial.TrialTemplate.TrialCategory.Contains("作息"));
            }

            // --- 步驟 2：根據 filter 套用「排序」條件 (ORDER BY) ---
            if (filter?.ToLower() == "hot")
            {
                //var oneDayAgo = DateTime.Now.AddDays(-1);
                query = query
                             //.Where(p => p.CreatedAt >= oneDayAgo) // 熱門只看近一天
                             .OrderByDescending(p => p.PostLikes.Count())
                             .ThenByDescending(p => p.CreatedAt);
            }
            else
            {
                // 所有其他情況 (預設、following、分類) 都統一按時間排序
                query = query.OrderByDescending(p => p.CreatedAt);
            }

            // --- 步驟 3：執行查詢並轉換為 DTO (這是您原本寫的、完全正確的兩步驟模式) ---

            // a. 從資料庫撈取經過篩選和排序後的「最多 50 筆」原始資料
            var rawPostsData = query
                .Take(50)
                .Include(p => p.User.UserAvatars.Select(ua => ua.Avatar))
                .Include(p => p.Trial.TrialTemplate)
                .Include(p => p.PostLikes)
                .Select(p => new
                {
                    PostObject = p,
                    UserNickName = p.User.NickName,
                    UserCurrentAvatar = p.User.UserAvatars.FirstOrDefault(ua => ua.IsCurrent).Avatar.AvatarImagePath,
                    TrialName = p.Trial.TrialName,
                    TemplateTitle = p.Trial.TrialTemplate.TrialTitle,
                    TemplateCategoryJson = p.Trial.TrialTemplate.TrialCategory,
                    Likes = p.PostLikes.Select(pl => pl.UserId)
                })
                .ToList();

            // b. 在記憶體中，將原始資料轉換成前端要的 DTO 格式
            var finalPosts = rawPostsData.Select(p => new PostDto
            {
                Id = p.PostObject.Id,
                Content = p.PostObject.Content,
                CreatedAt = p.PostObject.CreatedAt,
                UserStageId = p.PostObject.UserStageId,
                UserId = p.PostObject.UserId,
                TrialId = p.PostObject.TrialId,
                ImageUrl = JsonConvert.DeserializeObject<List<string>>(p.PostObject.ImageUrl ?? "[]"),
                Trial = new TrialInfoDto
                {
                    Title = p.TrialName,
                    Challenge = new ChallengeInfoDto
                    {
                        Title = p.TemplateTitle,
                        Category = JsonConvert.DeserializeObject<List<string>>(p.TemplateCategoryJson ?? "[]")
                    }
                },
                UserInfo = new UserInfoDto
                {
                    NickName = p.UserNickName,
                    CharacterImgLink = p.UserCurrentAvatar
                },
                PostLike = p.Likes.Select(likeUserId => new PostLikeInfoDto { LikeBy = likeUserId }).ToList(),
                IsLikedByViewer = viewerId.HasValue && p.Likes.Contains(viewerId.Value)
            })
            .ToList();

            return Ok(finalPosts);
        }

        // 檔案路徑: Controllers/PostsController.cs

        // ... 放在 GetPosts 方法的下面 ...

        [HttpGet]
        [Route("api/posts/{postId:int}")] // :int 是一個路由約束，確保 id 必須是整數
        [AllowAnonymous]
        public IHttpActionResult GetPostById(int postId)
        {
            Guid? viewerId = TryGetCurrentUserId();

            // --- 步驟 1：從資料庫撈取指定 id 的原始資料 ---
            // 查詢邏輯和 GetPosts 非常相似，但結尾是 .FirstOrDefault() 來取得單一筆
            var rawPostData = _db.Posts
                .Where(p => p.Id == postId) // <-- 關鍵篩選條件
                .Include(p => p.User.UserAvatars.Select(ua => ua.Avatar))
                .Include(p => p.Trial.TrialTemplate)
                .Include(p => p.PostLikes)
                .Select(p => new
                {
                    PostObject = p,
                    UserNickName = p.User.NickName,
                    UserCurrentAvatar = p.User.UserAvatars.FirstOrDefault(ua => ua.IsCurrent).Avatar.AvatarImagePath,
                    TrialName = p.Trial.TrialName,
                    TemplateTitle = p.Trial.TrialTemplate.TrialTitle,
                    TemplateCategoryJson = p.Trial.TrialTemplate.TrialCategory,
                    Likes = p.PostLikes.Select(pl => pl.UserId)
                })
                .FirstOrDefault(); // <-- 使用 FirstOrDefault() 取得單一筆

            // --- 步驟 2：處理找不到貼文的情況 ---
            if (rawPostData == null)
            {
                return Content(HttpStatusCode.NotFound, new ErrorResponseDto { ErrorCode = "POST_NOT_FOUND", Message = "指定的貼文不存在。" });
            }

            // --- 步驟 3：在記憶體中，將原始資料轉換成 DTO ---
            var postDto = new PostDto
            {
                Id = rawPostData.PostObject.Id,
                Content = rawPostData.PostObject.Content,
                CreatedAt = rawPostData.PostObject.CreatedAt,
                UserStageId = rawPostData.PostObject.UserStageId,
                UserId = rawPostData.PostObject.UserId,
                TrialId = rawPostData.PostObject.TrialId,
                ImageUrl = JsonConvert.DeserializeObject<List<string>>(rawPostData.PostObject.ImageUrl ?? "[]"),
                Trial = new TrialInfoDto
                {
                    Title = rawPostData.TrialName,
                    Challenge = new ChallengeInfoDto
                    {
                        Title = rawPostData.TemplateTitle,
                        Category = JsonConvert.DeserializeObject<List<string>>(rawPostData.TemplateCategoryJson ?? "[]")
                    }
                },
                UserInfo = new UserInfoDto
                {
                    NickName = rawPostData.UserNickName,
                    CharacterImgLink = rawPostData.UserCurrentAvatar
                },
                PostLike = rawPostData.Likes.Select(likeUserId => new PostLikeInfoDto { LikeBy = likeUserId }).ToList(),
                IsLikedByViewer = viewerId.HasValue && rawPostData.Likes.Contains(viewerId.Value)
            };

            return Ok(postDto);
        }


        [HttpGet]
        [Route("api/posts/liked")]
        public IHttpActionResult GetMyLikedPosts()
        {
            // 步驟 1：獲取「當前登入者」是誰
            Guid currentUserId = GetCurrentUserId();

            // 步驟 2：修改查詢邏輯
            var likedPosts = _db.PostLikes
                // a. 先找到所有「我」按的讚
                .Where(pl => pl.UserId == currentUserId)
                // b. 透過導覽屬性，從「讚」找到對應的「完整貼文物件」
                .Select(pl => pl.Post)
                // c. 現在我們拿到的是貼文列表，可以套用和 GetAllPosts 幾乎一樣的查詢了
                .Include(p => p.User.UserAvatars.Select(ua => ua.Avatar))
                .Include(p => p.Trial.TrialTemplate)
                .Include(p => p.PostLikes)
                .OrderByDescending(p => p.CreatedAt) // 依然可以按貼文建立時間排序
                .Select(p => new PostDto
                {
                    Id = p.Id,
                    Content = p.Content,
                    CreatedAt = p.CreatedAt,
                    UserStageId = p.UserStageId,
                    ImageUrl = JsonConvert.DeserializeObject<List<string>>(p.ImageUrl ?? "[]"),
                    UserId = p.UserId,
                    TrialId = p.TrialId,
                    Trial = new TrialInfoDto
                    {
                        Title = p.Trial.TrialName,
                        Challenge = new ChallengeInfoDto
                        {
                            Title = p.Trial.TrialTemplate.TrialTitle,
                            Category = JsonConvert.DeserializeObject<List<string>>(p.Trial.TrialTemplate.TrialCategory ?? "[]")
                        }
                    },
                    UserInfo = new UserInfoDto
                    {
                        NickName = p.User.NickName,
                        CharacterImgLink = p.User.UserAvatars.FirstOrDefault(ua => ua.IsCurrent).Avatar.AvatarImagePath
                    },
                    PostLike = p.PostLikes.Select(pl => new PostLikeInfoDto
                    {
                        LikeBy = pl.UserId
                    }).ToList(),

                    // ★★★ 邏輯簡化 ★★★
                    // 因為這個列表本來就是「我按讚的貼文」，所以 IsLikedByViewer 必定為 true
                    IsLikedByViewer = true
                })
                .ToList();

            return Ok(likedPosts);
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