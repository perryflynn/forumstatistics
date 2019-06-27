using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PerryFlynn.ForumStatistics.Parser
{
    /// <summary>
    /// Parse a thread
    /// </summary>
    public class ThreadParser : Parser
    {

        public IThreadInfo ThreadInfo { get; private set; }
        public IPostInfo PostInfo { get; private set; }
        public IUserInfo UserInfo { get; private set; }

        public ThreadParser(IThreadInfo threadinfo, IPostInfo postinfo, IUserInfo userinfo) : base()
        {
            this.ThreadInfo = threadinfo;
            this.PostInfo = postinfo;
            this.UserInfo = userinfo;
        }

        public async Task<ForumThread> ParseThread(Uri threadpageurl)
        {
            return await this.ParseThread(threadpageurl, null, null);
        }

        public async Task<ForumThread> ParseThread(Uri threadpageurl, uint? startpage, uint? lastpage)
        {
            Thread.Sleep(200);
            var response = await this.Client.GetAsync(threadpageurl);
            var html = await response.Content.ReadAsStringAsync();
            return await this.ParseThread(html, startpage, lastpage);
        }

        public async Task<ForumThread> ParseThread(string threadpagehtml, uint? startpage, uint? lastpage)
        {
            var thread = await this.ParseThreadMetadata(threadpagehtml);

            if (startpage.HasValue && startpage.Value < 1)
            {
                throw new ArgumentException("startpage must be at least 1");
            }

            if (lastpage.HasValue && (lastpage.Value < startpage.Value || lastpage.Value > thread.PageCount))
            {
                throw new ArgumentException("lastpage must be greater than startpage and smaller or equals thread pagecount");
            }

            for (uint i = startpage ?? 1; i <= (lastpage ?? thread.PageCount); i++)
            {
                var pageurl = this.ThreadInfo.BuildPageUrlFunc(thread.StartpageUrl, i);
                System.Diagnostics.Debug.WriteLine("Fetch " + pageurl);
                thread.Posts.AddRange(await this.ExtractPosts(new Uri(pageurl), thread.Users));
            }

            return thread;
        }

        public async Task<ForumThread> ParseThreadMetadata(Uri threadpageurl)
        {
            var response = await this.Client.GetAsync(threadpageurl);
            var html = await response.Content.ReadAsStringAsync();
            return await this.ParseThreadMetadata(html);
        }

        public async Task<ForumThread> ParseThreadMetadata(string threadsitehtml)
        {
            return new ForumThread(new UserParser(this.UserInfo))
            {
                Uid = (await this.ExtractUnsignedInt(threadsitehtml, "thread uid", this.ThreadInfo.RegexThreadUid, 1)).Value,
                CurrentPageNo = (uint) this.ThreadInfo.NormalizeNumbersFunc( (await this.ExtractString(threadsitehtml, "current page no", this.ThreadInfo.RegexCurrentPageNo, 1)) ),
                StartpageUrl = await this.ExtractString(threadsitehtml, "startpage url", this.ThreadInfo.RegexStartUrl, 1),
                PageCount = (await this.ExtractUnsignedInt(threadsitehtml, "page count", this.ThreadInfo.RegexPageMaxCount, 1)).Value
            };
        }

        public async Task<List<ThreadPost>> ExtractPosts(Uri threadpageurl, ForumUserCollection users)
        {
            Thread.Sleep(200);
            var response = await this.Client.GetAsync(threadpageurl);
            var html = await response.Content.ReadAsStringAsync();
            return await this.ExtractPosts(html, users);
        }

        public virtual async Task<List<ThreadPost>> ExtractPosts(string threadpagehtml, ForumUserCollection users)
        {
            return await Task.Run(() =>
            {
                var postparser = new PostParser(this.PostInfo, users);
                var result = new List<ThreadPost>();

                var doc = new HtmlDocument();
                doc.LoadHtml(threadpagehtml);

                var posts = doc.DocumentNode.SelectNodes(this.ThreadInfo.XpathPosts);

                foreach (var post in posts)
                {
                    result.Add(postparser.Parse(post.OuterHtml).Result);
                }

                return result;
            });
        }

        public override void Dispose()
        {
            this.ThreadInfo = null;
            this.PostInfo = null;
            this.UserInfo = null;
        }

    }
}
