using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace PerryFlynn.ForumStatistics.Parser
{
    /// <summary>
    /// Main object to serialize the thread statistic
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class ForumThread : IDisposable
    {

        [JsonProperty(Order = 5)]
        public DateTime CrawlTimestamp { get; private set; } = DateTime.Now;

        [JsonProperty(Order = 10)]
        public uint Uid { get; set; }

        [JsonProperty(Order = 20)]
        public string StartpageUrl { get; set; }

        [JsonProperty(Order = 30)]
        public uint CurrentPageNo { get; set; }

        [JsonProperty(Order = 40)]
        public uint PageCount { get; set; }

        [JsonProperty(Order = 50)]
        public ForumUserCollection Users { get; private set; }

        [JsonProperty(Order = 60)]
        public List<ThreadPost> Posts { get; private set; } = new List<ThreadPost>();


        public ForumThread(UserParser userparser)
        {
            this.Users = new ForumUserCollection(userparser);
        }

        public void Serialize(FileInfo file)
        {
            var settings = new JsonSerializerSettings()
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            };

            string json = JsonConvert.SerializeObject(this, settings);
            File.WriteAllText(file.FullName, json);
        }

        public void Dispose()
        {
            if (this.Posts != null)
            {
                this.Posts.Clear();
                this.Posts = null;
            }
            if (this.Users != null)
            {
                this.Users.Dispose();
                this.Users = null;
            }
        }
    }
}
