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

        public bool Import(Uri url)
        {
            if (!this.Contains(url))
            {
                var user = this.UserParser.ParseUserPage(url).Result;
                if (!this.Contains(user.Uid))
                {
                    this.Users.Add(user);
                    return true;
                }
            }
            return false;
        }

        public bool Import(string userSiteHtml)
        {
            var user = this.UserParser.ParseUserPage(userSiteHtml).Result;
            if(!this.Contains(user.Username))
            {
                this.Users.Add(user);
                return true;
            }
            return false;
        }

        public ForumUser Get(string username)
        {
            return this.Users.Single(u => u.Username==username);
        }

        public ForumUser GetOrImport(Uri url)
        {
            if (!this.Contains(url))
            {
                this.Import(url);
            }
            return this.Users.Single(v => v.Url == url.ToString());
        }

        public ForumUser GetOrImport(string userSiteHtml)
        {
            var user = this.UserParser.ParseUserPage(userSiteHtml).Result;
            if(!this.Contains(user.Username))
            {
                this.Users.Add(user);
                return user;
            }
            return user;
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
