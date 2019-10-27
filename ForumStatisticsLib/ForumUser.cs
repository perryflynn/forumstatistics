using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace PerryFlynn.ForumStatistics.Parser
{
    /// <summary>
    /// Forum user object
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class ForumUser
    {

        [JsonProperty]
        public uint Uid { get; set; }

        [JsonProperty]
        public string Url { get; set; }

        [JsonProperty]
        public string Username { get; set; }

        [JsonProperty]
        public string Title { get; set; }

        [JsonProperty]
        public DateTime MemberSince { get; set; }

        [JsonProperty]
        public uint PostCount { get; set; }

        [JsonProperty]
        public string SignatureHtml { get; set; }

        [JsonProperty]
        public bool IsBanned { get; set; }

        public Uri Uri { get { return new Uri(this.Url); } }
    }
}
