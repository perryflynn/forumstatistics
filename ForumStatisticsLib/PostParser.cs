using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace PerryFlynn.ForumStatistics.Parser
{
    /// <summary>
    /// Parse a post
    /// </summary>
    public class PostParser : Parser
    {

        public IPostInfo Info { get; private set; }
        public ForumUserCollection Users { get; private set; }
        public SearchPageParser SearchParser { get; set; }

        public PostParser(IPostInfo info, SearchPageParser userParser, ForumUserCollection users) : base()
        {
            this.Info = info;
            this.Users = users;
            this.SearchParser = userParser;
        }

        public virtual async Task<ThreadPost> Parse(string posthtml)
        {
            var userurl = await this.ExtractString(posthtml, "post user url", this.Info.RegexUserUrl, 1, true, null);
            var username = await this.ExtractString(posthtml, "post username", this.Info.RegexUsername, 1, true, null);
            var useruid = await this.ExtractUnsignedInt(posthtml, "post user uid", this.Info.RegexUserUid, 1, true, null);

            bool guestpost = false;
            ForumUser user = null;
            string guestusername = null;
            string guesttitle = null;

            if (userurl == null || username == null)
            {
                guestpost = true;
                guestusername = await this.ExtractString(posthtml, "guest username", this.Info.RegexGuestUsername, 1);
                guesttitle = await this.ExtractString(posthtml, "guest title", this.Info.RegexGuestTitle, 1);
            }
            else
            {
                guestpost = false;
                if (this.Users.Contains(username))
                {
                    user = this.Users.Get(username);
                }
                else
                {
                    var searchResult = await this.SearchParser.SearchUser(username);
                    if(searchResult.Count > 0)
                    {
                        searchResult.ForEach(u => this.Users.Import(u));
                        user = this.Users.Get(username);
                    }
                }
            }

            if(user == null && guestusername == null)
            {
                throw new Exception("Could not extract user");
            }

            var post = new ThreadPost()
            {
                Uid = (await this.ExtractUnsignedInt(posthtml, "post uid", this.Info.RegexUid, 1)).Value,
                Url = await this.ExtractString(posthtml, "post url", this.Info.RegexPostUrl, 1),
                UserUid = useruid,
                UserUrl = userurl,
                User = user,
                IsGuestPost = guestpost,
                GuestUsername = guestusername,
                GuestTitle = guesttitle,
                Date = (await this.ExtractDateTime(posthtml, "post date", this.Info.RegexDate, 1, this.Info.DateFormat)).Value,
                MessageHtml = await this.ExtractMessageHtml(posthtml),
                LikeCount = (await this.ExtractUnsignedInt(posthtml, "post like count", this.Info.RegexLikeCount, 1, true, 0)).Value,
                DislikeCount = (await this.ExtractUnsignedInt(posthtml, "post dislike count", this.Info.RegexDislikeCount, 1, true, 0)).Value
            };

            if (user != null && string.IsNullOrEmpty(user.SignatureHtml))
            {
                user.SignatureHtml = await this.ExtractSignatureHtml(posthtml);
            }

            return post;
        }

        public virtual async Task<string> ExtractMessageHtml(string posthtml)
        {
            return await Task.Run(() =>
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(posthtml);

                var message = doc.DocumentNode.SelectNodes(this.Info.XpathMessageHtml);
                return message.First().InnerHtml;
            });
        }

        public virtual async Task<string> ExtractSignatureHtml(string posthtml)
        {
            return await Task.Run(() =>
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(posthtml);

                var message = doc.DocumentNode.SelectNodes(this.Info.XpathSignature);
                return message?.FirstOrDefault()?.InnerHtml;
            });
        }

        public override void Dispose()
        {
            this.Users = null;
            this.Info = null;
        }
    }
}
