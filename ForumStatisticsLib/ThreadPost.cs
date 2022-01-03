using Newtonsoft.Json;
using System;

namespace PerryFlynn.ForumStatistics.Parser
{
    /// <summary>
    /// Object to serialize a single post
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class ThreadPost
    {

        [JsonProperty]
        public uint Uid { get; set; }

        [JsonProperty]
        public string Url { get; set; }

        [JsonProperty]
        public uint? UserUid { get; set; }

        [JsonProperty]
        public bool IsGuestPost { get; set; }

        [JsonProperty]
        public bool IsDeletedPost { get; set; }


        [JsonProperty]
        public string GuestUsername { get; set; }

        [JsonProperty]
        public string GuestTitle { get; set; }

        [JsonProperty]
        public string UserUrl { get; set; }

        [JsonProperty]
        public ForumUser User { get; set; }

        [JsonProperty]
        public DateTime Date { get; set; }

        [JsonProperty]
        public string MessageHtml { get; set; }

        [JsonProperty]
        public uint LikeCount { get; set; }

        [JsonProperty]
        public uint DislikeCount { get; set; }

    }
}