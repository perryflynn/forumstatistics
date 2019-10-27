using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.Threading.Tasks;

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

        public async Task<bool> ImportAsync(Uri url)
        {
            if (!this.Contains(url))
            {
                var user = await this.UserParser.ParseUserPageAsync(url);
                if (!this.Contains(user.Uid))
                {
                    this.Users.Add(user);
                    return true;
                }
            }
            return false;
        }

        public async Task<bool> ImportAsync(string userSiteHtml)
        {
            var user = await this.UserParser.ParseUserPageAsync(userSiteHtml);
            if (!this.Contains(user.Username))
            {
                this.Users.Add(user);
                return true;
            }
            return false;
        }

        public ForumUser Get(string username)
        {
            return this.Users.Single(u => u.Username == username);
        }

        public async Task<ForumUser> GetOrImportAsync(Uri url)
        {
            if (!this.Contains(url))
            {
                await this.ImportAsync(url);
            }
            return this.Users.Single(v => v.Url == url.ToString());
        }

        public async Task<ForumUser> GetOrImportAsync(string userSiteHtml)
        {
            var user = await this.UserParser.ParseUserPageAsync(userSiteHtml);
            if (!this.Contains(user.Username))
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
