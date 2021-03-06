﻿using HtmlAgilityPack;
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
        public IUserSearchInfo SearchInfo { get; set; }

        public ThreadParser(IThreadInfo threadinfo, IPostInfo postinfo, IUserInfo userinfo, IUserSearchInfo searchInfo) : base()
        {
            this.ThreadInfo = threadinfo;
            this.PostInfo = postinfo;
            this.UserInfo = userinfo;
            this.SearchInfo = searchInfo;
        }

        public async Task<ForumThread> ParseThreadAsync(Uri threadpageurl)
        {
            return await this.ParseThreadAsync(threadpageurl, null, null);
        }

        public async Task<ForumThread> ParseThreadAsync(Uri threadpageurl, uint? startpage, uint? lastpage)
        {
            Thread.Sleep(200);
            var response = await this.Client.GetAsync(threadpageurl);
            var html = await response.Content.ReadAsStringAsync();
            return await this.ParseThreadAsync(html, startpage, lastpage);
        }

        public async Task<ForumThread> ParseThreadAsync(string threadpagehtml, uint? startpage, uint? lastpage)
        {
            var thread = await this.ParseThreadMetadataAsync(threadpagehtml);

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
                thread.Posts.AddRange(await this.ExtractPostsAsync(new Uri(pageurl), thread.Users));
            }

            return thread;
        }

        public async Task<ForumThread> ParseThreadMetadataAsync(Uri threadpageurl)
        {
            var response = await this.Client.GetAsync(threadpageurl);
            var html = await response.Content.ReadAsStringAsync();
            return await this.ParseThreadMetadataAsync(html);
        }

        public async Task<ForumThread> ParseThreadMetadataAsync(string threadsitehtml)
        {
            return new ForumThread(new UserParser(this.UserInfo))
            {
                Uid = (await this.ExtractUnsignedIntAsync(threadsitehtml, "thread uid", this.ThreadInfo.RegexThreadUid, 1)).Value,
                CurrentPageNo = (uint) this.ThreadInfo.NormalizeNumbersFunc( (await this.ExtractStringAsync(threadsitehtml, "current page no", this.ThreadInfo.RegexCurrentPageNo, 1)) ),
                StartpageUrl = await this.ExtractStringAsync(threadsitehtml, "startpage url", this.ThreadInfo.RegexStartUrl, 1),
                PageCount = (await this.ExtractUnsignedIntAsync(threadsitehtml, "page count", this.ThreadInfo.RegexPageMaxCount, 1)).Value
            };
        }

        public async Task<List<ThreadPost>> ExtractPostsAsync(Uri threadpageurl, ForumUserCollection users)
        {
            Thread.Sleep(200);
            var response = await this.Client.GetAsync(threadpageurl);
            var html = await response.Content.ReadAsStringAsync();
            return await this.ExtractPostsAsync(html, users);
        }

        public virtual async Task<List<ThreadPost>> ExtractPostsAsync(string threadpagehtml, ForumUserCollection users)
        {
            var searchparser = new SearchPageParser(this.SearchInfo);
            var postparser = new PostParser(this.PostInfo, searchparser, users);
            var result = new List<ThreadPost>();

            var doc = new HtmlDocument();
            doc.LoadHtml(threadpagehtml);

            var posts = doc.DocumentNode.SelectNodes(this.ThreadInfo.XpathPosts);

            foreach (var post in posts)
            {
                result.Add(await postparser.ParseAsync(post.OuterHtml));
            }

            return result;
        }

        public override void Dispose()
        {
            this.ThreadInfo = null;
            this.PostInfo = null;
            this.UserInfo = null;
            this.SearchInfo = null;
        }

    }
}
