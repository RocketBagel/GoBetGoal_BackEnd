using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GoBetGoal_BackEnd.Models.DTOs
{
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
        public object ImageResults { get; set; } // 使用 object 以支援兩種不同的 ImageResult
        public int ChanceRemain { get; set; }
    }

    // 用於 PerImage 模式的回應
    public class PerImageResult
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
    public class OpenAIChatResponse { [JsonProperty("choices")] public List<ResponseChoice> Choices { get; set; } }
    public class ResponseChoice { [JsonProperty("message")] public ResponseMessage Message { get; set; } }
    public class ResponseMessage { [JsonProperty("role")] public string Role { get; set; } [JsonProperty("content")] public JToken Content { get; set; } }

}