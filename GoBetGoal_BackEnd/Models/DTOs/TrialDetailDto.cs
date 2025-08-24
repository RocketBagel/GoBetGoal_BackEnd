// 檔案: Models/DTOs/TrialDetailDto.cs
using GoBetGoal_BackEnd.Models.DTOs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

public class TrialDetailDto
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("title")] // 對應前端的 TrialSupa.title
    public string TrialName { get; set; }

    [JsonProperty("start_at")]
    public DateTime StartAt { get; set; }

    [JsonProperty("end_at")]
    public DateTime EndAt { get; set; }

    [JsonProperty("trial_status")]
    public string TrialStatus { get; set; }

    [JsonProperty("deposit")]
    public int Deposit { get; set; }

    [JsonProperty("create_by")]
    public Guid CreatorId { get; set; }


    [JsonProperty("created_at")]
    public DateTime CreatedAt { get; set; }

    // 這裡的屬性名稱 challenge，剛好與前端的 ChallengeSupa 屬性 challenge 同名
    // 所以可以不用加 [JsonProperty]，但為了風格一致，加上也很好
    [JsonProperty("challenge")]
    public TrialTemplateInfoDto TrialTemplateInfo { get; set; }

    [JsonProperty("trial_participant")]
    public List<TrialParticipantDto> Participants { get; set; }

    // 假設前端圍觀者列表叫做 trial_likes
    [JsonProperty("trial_likes")]
    public List<TrialLikeDto> TrialLikes { get; set; }
}