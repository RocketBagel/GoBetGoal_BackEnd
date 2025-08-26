using GoBetGoal_BackEnd.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GoBetGoal_BackEnd.Services
{
    public static class ChallengeHelper
    {
        /// <summary>
        /// 獲取所有 AI 審核任務共用的「系統指令 (Master System Prompt)」。
        /// 這個指令定義了 AI 的角色、核心安全規則與回應格式。
        /// </summary>
        /// <returns>一個包含核心指令的字串。</returns>
        public static string GetMasterSystemPrompt()
        {
            return @"
You are a helpful and fair AI health coach named 'Habit Helper' for a health and wellness platform. Your personality is fair, helpful, and encouraging, but you are also a meticulous and strict judge when it comes to verifying user-submitted challenge tasks.

Your absolute first priority is community safety. You must analyze every image for harmful content based on these specific rules:
- **Unsafe Content (FAIL IMMEDIATELY):** Nudity, pornography, violence, gore, self-harm, hate symbols, illegal substances, and firearms.
- **Safe Contextual Items (DO NOT FAIL):** Kitchen knives, cutlery, or fire used in a normal cooking or dining context are considered SAFE. People in swimwear at a beach or pool are considered SAFE.

Your second priority is to strictly verify the user's task compliance based ONLY on the rules provided.

You MUST ALWAYS respond ONLY with a JSON object, with no additional text or explanations outside of the JSON structure.";
        }
        /// <summary>
        /// 建立「逐張審核」模式的 Prompt。【已修正為最簡單的字串語法】
        /// </summary>
        public static string BuildUserPrompt(string specificRule, List<string> generalRules, string challengeType)
        {
            string verificationInstructions;

            // 根據前端傳來的 ChallengeType，選擇最精準的 AI 思考模式
            switch (challengeType)
            {
                case "FoodCombination":
                    verificationInstructions = "Your method is to act as a nutritionist. Visually identify each required food item from the 'Specific Rule'. Be flexible with 'OR' conditions and assume common healthy cooking methods are used. An unpeeled egg in a breakfast context is a boiled egg.";
                    break;

                case "FitnessOCR":
                    verificationInstructions = "Your method is to act as a data verifier. Perform OCR on the image to find numbers on a digital display (like a treadmill or smartwatch). The extracted value (time or steps) must be equal to or greater than the value specified in the 'Specific Rule'.";
                    break;

                case "NegativeList":
                    verificationInstructions = "Your method is to act as a compliance officer. Scan the image meticulously to ensure NONE of the prohibited items listed in the 'Specific Rule' are present. The presence of ANY prohibited item means the task fails.";
                    break;

                case "ExclusiveDiet":
                    verificationInstructions = "Your method is to act as a strict diet supervisor. The user is only allowed to eat items from a specific food group mentioned in the 'Specific Rule' (e.g., only meat, only eggs). Identify all food items in the image and fail the task if ANY non-allowed food group is present.";
                    break;

                case "AbstractHonor":
                    // 對於 AI 無法驗證的任務，我們給予一個直接通過的指令
                    verificationInstructions = "This is an honor-system task that cannot be visually verified (like 'Healthy Diet' or 'Read a book'). Assume the user has completed it. Your task is to confirm the image is safe and then pass the compliance check.";
                    break;

                default:
                    verificationInstructions = "Your method is to analyze the image and judge its compliance against the 'Specific Rule' below.";
                    break;
            }

            // --- 動態 User Prompt 範本 ---
            return $@"
Please perform a verification task.

## Verification Method:
{verificationInstructions}

## General Rules to Enforce:
- {string.Join("\n- ", generalRules ?? new List<string>())}

## Specific Rule for this task/image:
- {specificRule}

## Final Judgment:
Based on your analysis, provide the final JSON output. The JSON format MUST be:
{{
  ""isSafe"": true or false,
  ""isCompliant"": true or false,
  ""reason"": ""Provide a brief, helpful, and encouraging explanation in Traditional Chinese.""
}}
- Do NOT include any extra text, commentary, or formatting outside the JSON object.
- Make sure 'isSafe' and 'isCompliant' are **strict booleans**, not strings.
- Be concise in 'reason', max 20 words.
";
        }


        /// <summary>
        /// 建立「綜合審核」模式的 Prompt
        /// </summary>
        public static string BuildCollectivePrompt(string rule, List<string> generalRules)
        {
            string generalRulesText = string.Join("\n- ", generalRules);

            // 這個 Prompt 指示 AI 扮演最終的法官角色
            return $@"
You are a helpful and fair AI health coach acting as a final judge.
Your first priority is community safety. You must analyze all provided images for harmful content.
- **Safety Rules:** Immediately fail any image containing nudity, violence, etc. Kitchen knives in a dining context are SAFE.

Your second priority is to perform a COLLECTIVE analysis.
## Task:
Analyze ALL of the images provided to you in this single prompt. Determine if the COMBINED food items across ALL images satisfy the 'Daily Meal Rule' below.

## Judging Philosophy:
- You must enforce all 'General Challenge Rules'.
- Your final 'isCompliant' judgment should be based on the TOTALITY of food shown across all images.

## General Challenge Rules to Enforce:
- {generalRulesText}

## Daily Meal Rule to Verify:
- {rule}

## Final Judgment:
Based on your collective analysis of ALL images, provide a single JSON output.
The final JSON object MUST be in this exact format:
{{
  ""overall_assessment"": {{
    ""isCompliant"": true or false,
    ""reason"": ""A brief, overall explanation in Traditional Chinese for the final result.""
  }},
  ""per_image_details"": [
    {{
      ""image_index"": 0,
      ""isSafe"": true or false,
      ""detected_items"": ""...""
    }}
  ]
}}";
        }

        public static T ParseAIResponse<T>(string rawResponse) where T : class, new()
        {
            if (string.IsNullOrWhiteSpace(rawResponse))
            {
                Console.WriteLine("AI Response is empty.");
                return new T();
            }

            // 嘗試去掉可能的多餘符號（如 ``` 或 "json\n"）
            var cleanJson = rawResponse.Trim().Trim('`').Replace("json\n", "").Trim();

            try
            {
                var result = JsonConvert.DeserializeObject<T>(cleanJson);
                if (result == null)
                {
                    Console.WriteLine("Deserialized result is null. Raw response: " + rawResponse);
                    return new T();
                }
                return result;
            }
            catch (JsonException ex)
            {
                // Log 原始回應，方便排查問題
                Console.WriteLine("Failed to parse AI response. Exception: " + ex.Message);
                Console.WriteLine("Raw AI response: " + rawResponse);
                return new T();
            }
        }


        //public static bool CheckFreeMealCompliance(List<string> detectedFoods, string rule)
        //{
        //    var lowerCaseDetectedFoods = detectedFoods.Select(f => f.ToLower()).ToList();
        //    var requiredFoods = new List<string>();
        //    if (rule.Contains("雞胸肉")) requiredFoods.Add("chicken");
        //    if (rule.Contains("番茄")) requiredFoods.Add("tomato");

        //    foreach (var required in requiredFoods)
        //    {
        //        if (!lowerCaseDetectedFoods.Any(detected => detected.Contains(required)))
        //        {
        //            return false;
        //        }
        //    }
        //    return true;
        //}
    }
}