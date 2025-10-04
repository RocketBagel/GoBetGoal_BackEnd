using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GoBetGoal_BackEnd.Models.DTOs
{
    /// <summary>
    /// 【V2 版】為 AI 審核微服務設計的請求模型
    /// </summary>
    public class AiVerificationRequestV2
    {
        /// <summary>
        /// 圖片 URL 陣列，會依照 StageDescriptions 的順序進行一對一審核
        /// </summary>
        [Required]
        public List<string> ImageUrls { get; set; }

        /// <summary>
        /// 關卡的「審核類型」，例如 "FoodCombination", "FitnessOCR"。
        /// 後端將依此來決定使用哪一套 AI 指令。
        /// </summary>
        [Required]
        public string ChallengeType { get; set; }

        /// <summary>
        /// 針對每張圖片的「具體關卡規則」陣列。
        /// 陣列長度應與 ImageUrls 一致，或只有一個元素（代表所有圖片共用此規則）。
        /// </summary>
        [Required]
        public List<string> StageDescriptions { get; set; }

        /// <summary>
        /// 整個試煉的「通用規則」列表。
        /// </summary>
        [Required]
        public List<string> TrialRules { get; set; }
    }

    /// <summary>
    /// 【V2 版】AI 審核微服務回傳給前端的、簡潔的結果模型
    /// </summary>
    public class AiVerificationResponseV2
    {
        /// <summary>
        /// 整個關卡（所有圖片）是否最終通過
        /// </summary>
        public bool OverallResult { get; set; }

        /// <summary>
        /// 對整個關卡結果的總結訊息
        /// </summary>
        public string OverallMessage { get; set; }
        public int ChanceRemain { get; set; }

        /// <summary>
        // 針對每一張圖片的獨立、詳細的審核結果
        /// </summary>
        public List<ImageResult> ImageResults { get; set; } = new List<ImageResult>();
    }

    public class ChallengeSubmissionRequest
    {
        public int TrialId { get; set; }
        public int StageIndex { get; set; }
        public List<string> ImageUrls { get; set; }
    }

   

    public class ChallengeSubmissionResponse
    {
        public bool OverallResult { get; set; }
        public string OverallMessage { get; set; }

        // 【修正 #1】我們不再使用 object，而是永遠使用一個清晰、強型別的 List<ImageResult>
        public List<ImageResult> ImageResults { get; set; } = new List<ImageResult>();
        public int ChanceRemain { get; set; }
    }

    // 用於 PerImage 模式的回應
    public class ImageResult
    {
        public string ImageUrl { get; set; }
        public bool IsSafe { get; set; }
        public bool IsCompliant { get; set; }
        public string Reason { get; set; }
    }

    // 用於 Collective 模式的回應
    public class CollectiveImageResult
    {
        public string ImageUrl { get; set; }
        public bool IsSafe { get; set; }
    }

    /// <summary>
    /// 用於解析 AI 回應的專用模型
    /// </summary>
    public class AIVerificationResult
    {
        [JsonProperty("isSafe")]
        public bool IsSafe { get; set; }

        [JsonProperty("isCompliant")]
        public bool IsCompliant { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }
    }

    /// <summary>
    /// 用於解析「綜合審核」AI 回應的專用頂層模型
    /// </summary>
    public class AICollectiveResponse
    {
        [JsonProperty("overall_assessment")]
        public AIVerificationResult OverallAssessment { get; set; }

        [JsonProperty("per_image_details")]
        public List<AIImageDetail> PerImageDetails { get; set; } = new List<AIImageDetail>();
    }

    /// <summary>
    /// AI 對單張圖片的詳細分析
    /// </summary>
    public class AIImageDetail
    {
        [JsonProperty("image_index")]
        public int ImageIndex { get; set; }

        [JsonProperty("is_safe")]
        public bool IsSafe { get; set; }

        [JsonProperty("detected_items")]
        public string DetectedItems { get; set; }
    }

    /// <summary>
    /// OpenAI API 回傳的 token 用量物件 (官方收據)
    /// </summary>
    public class UsageData
    {
        [JsonProperty("prompt_tokens")]
        public int PromptTokens { get; set; }

        [JsonProperty("completion_tokens")]
        public int CompletionTokens { get; set; }

        [JsonProperty("total_tokens")]
        public int TotalTokens { get; set; }
    }

    /// <summary>
    /// 我們 OpenAIHttpClientService 的標準化回傳格式 (內部工作報告)
    /// </summary>
    public class AiServiceResponse
    {
        public string MessageContent { get; set; }
        public UsageData Usage { get; set; }
        public bool IsSuccess { get; set; }
    }

    /// <summary>
    /// 【升級版】用於解析 OpenAI 完整回應的模型，現在包含了 Usage
    /// </summary>
    public class OpenAIChatResponse
    {
        [JsonProperty("choices")]
        public List<ResponseChoice> Choices { get; set; }

        [JsonProperty("usage")]
        public UsageData Usage { get; set; }
    }

    //public class AICollectiveDetectionResult
    //{
    //    [JsonProperty("detected_matching_foods")]
    //    public List<string> DetectedMatchingFoods { get; set; }
    //    [JsonProperty("violated_rules")]
    //    public List<string> ViolatedRules { get; set; }
    //}
    // 圖片+文字用
    //public class VerificationRequest
    //{
    //    public List<string> ImageUrls { get; set; } = new List<string>();
    //    public string Text { get; set; }  // 可選，用於文字審核
    //}

    // --- OpenAI API 請求的模型 ---
    public class OpenAIChatRequest { [JsonProperty("model")] public string Model { get; set; } [JsonProperty("messages")] public List<RequestMessage> Messages { get; set; } [JsonProperty("max_tokens")] public int MaxTokens { get; set; } }
    public class RequestMessage { [JsonProperty("role")] public string Role { get; set; } [JsonProperty("content")] public List<RequestContent> Content { get; set; } }
    public class RequestContent { [JsonProperty("type")] public string Type { get; set; } [JsonProperty("text", NullValueHandling = NullValueHandling.Ignore)] public string Text { get; set; } [JsonProperty("image_url", NullValueHandling = NullValueHandling.Ignore)] public RequestImageUrl ImageUrl { get; set; } }
    public class RequestImageUrl { [JsonProperty("url")] public string Url { get; set; } }
    //public class OpenAIChatResponse { [JsonProperty("choices")] public List<ResponseChoice> Choices { get; set; } }
    public class ResponseChoice { [JsonProperty("message")] public ResponseMessage Message { get; set; } }
    public class ResponseMessage { [JsonProperty("role")] public string Role { get; set; } [JsonProperty("content")] public JToken Content { get; set; } }

}