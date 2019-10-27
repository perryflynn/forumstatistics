using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace PerryFlynn.ForumStatistics.Parser
{
    public class SearchPageParser : Parser
    {
        public string FormToken { get; set; }
        public IUserSearchInfo Info { get; set; }

        public SearchPageParser(IUserSearchInfo info) : base()
        {
            this.Info = info;

            var task = Task.Run(async () =>
            {
                this.FormToken = await this.GetFormTokenAsync();
            });

            Task.WaitAll(task);
        }

        public async Task<string> GetFormTokenAsync()
        {
            var response = await this.Client.GetAsync(this.Info.SearchPageUrl);
            var html = await response.Content.ReadAsStringAsync();
            return await this.ExtractStringAsync(html, "form token", this.Info.SearchTokenRegex, 1, true, null);
        }

        public async Task<List<string>> SearchUserAsync(string username, bool retry = false)
        {
            Thread.Sleep(200);

            var parameters = new List<KeyValuePair<string,string>>()
            {
                new KeyValuePair<string, string>("username", username.Replace("\\", "\\\\")),
                new KeyValuePair<string, string>("t", this.FormToken)
            };

            var request = new FormUrlEncodedContent(parameters);
            var response = await this.Client.PostAsync(this.Info.SearchUrl, request);
            var html = await response.Content.ReadAsStringAsync();

            if((new Regex(this.Info.SessionExpiredRegex)).IsMatch(html))
            {
                if(retry == false)
                {
                    this.FormToken = await this.GetFormTokenAsync();
                    return await this.SearchUserAsync(username, true);
                }
                else
                {
                    throw new Exception("Unable to get a valid form session token");
                }
            }

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var usersHtml = doc.DocumentNode.SelectNodes(this.Info.UserBlockXPath);

            return usersHtml?.Select(u => u.OuterHtml).ToList() ?? new List<string>();
        }
    }
}
