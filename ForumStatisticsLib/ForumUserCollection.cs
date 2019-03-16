using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Newtonsoft.Json;

namespace PerryFlynn.ForumStatistics.Parser
{
    /// <summary>
    /// Collection of forum users
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class ForumUserCollection : IDisposable
    {

        public UserParser UserParser { get; private set; }

        [JsonProperty]
        public List<ForumUser> Users { get; private set; } = new List<ForumUser>();

        public ForumUserCollection(UserParser parser)
        {
            this.UserParser = parser;
        }

        public bool Contains(string name)
        {
            return this.Users.Any(v => v.Username == name);
        }

        public bool Contains(uint uid)
        {
            return this.Users.Any(v => v.Uid == uid);
        }

        public bool Contains(Uri uri)
        {
            return this.Users.Any(v => v.Uri == uri);
        }

        public bool Import(string url)
        {
            if (!this.Contains(new Uri(url)))
            {
                var user = this.UserParser.ParseUserPage(new Uri(url)).Result;
                if (!this.Contains(user.Uid))
                {
                    this.Users.Add(user);
                    return true;
                }
            }
            return false;
        }

        public ForumUser GetOrImport(string url)
        {
            if (!this.Contains(new Uri(url)))
            {
                this.Import(url);
            }
            return this.Users.Where(v => v.Url == url).Single();
        }

        public void Dispose()
        {
            this.UserParser = null;
            if (this.Users != null)
            {
                this.Users.Clear();
                this.Users = null;
            }
        }
    }
}
