using System;
using System.Globalization;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PerryFlynn.ForumStatistics.Parser
{
    /// <summary>
    /// The parser lib
    /// </summary>
    public abstract class Parser : IDisposable
    {
        public HttpClient Client { get; set; }

        public Parser()
        {
            this.Client = this.InitializeHttpClient();
        }

        public virtual void Dispose()
        {
            if(this.Client != null)
            {
                this.Client.Dispose();
                this.Client = null;
            }
        }

        protected virtual HttpClient InitializeHttpClient()
        {
            var handler = new HttpClientHandler()
            {
                AllowAutoRedirect = true,
                MaxAutomaticRedirections = 42,
                UseCookies = true,
                CookieContainer = new System.Net.CookieContainer()
            };

            var client = new HttpClient(handler, true);
            client.DefaultRequestHeaders.Add("User-Agent", "perryflynns Forum Statistics Crawler");
            client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9,de;q=0.8");
            return client;
        }

        protected async Task<uint?> ExtractUnsignedInt(string content, string propertyname, string regex, int group, bool usedefaultvalue, uint? defaultvalue)
        {
            return await Task.Run(() =>
            {
                var rgx = new Regex(regex);
                if (rgx.IsMatch(content))
                {
                    return uint.Parse(rgx.Match(content).Groups[group].Value);
                }
                else if (usedefaultvalue)
                {
                    return defaultvalue;
                }
                throw new NoMatchException(propertyname, regex);
            });
        }

        protected async Task<uint?> ExtractUnsignedInt(string content, string propertyname, string regex, int group)
        {
            return await this.ExtractUnsignedInt(content, propertyname, regex, group, false, null);
        }

        protected virtual async Task<string> ExtractString(string content, string propertyname, string regex, int group, bool usedefaultvalue, string defaultvalue)
        {
            return await Task.Run(() =>
            {
                var rgx = new Regex(regex);
                if (rgx.IsMatch(content))
                {
                    return rgx.Match(content).Groups[group].Value;
                }
                else if (usedefaultvalue)
                {
                    return defaultvalue;
                }
                throw new NoMatchException(propertyname, regex);
            });
        }

        protected virtual async Task<string> ExtractString(string content, string propertyname, string regex, int group)
        {
            return await this.ExtractString(content, propertyname, regex, group, false, null);
        }

        protected virtual async Task<DateTime?> ExtractDateTime(string content, string propertyname, string regex, int group, string dateformat, bool usedefaultvalue, DateTime? defaultvalue)
        {
            try
            {
                string strdate = await this.ExtractString(content, propertyname, regex, group);
                return DateTime.ParseExact(strdate, dateformat, CultureInfo.InvariantCulture);
            }
            catch (Exception ex) when (usedefaultvalue && (ex is ArgumentNullException || ex is FormatException))
            {
                return defaultvalue;
            }
        }

        protected virtual async Task<DateTime?> ExtractDateTime(string content, string propertyname, string regex, int group, string dateformat)
        {
            return await this.ExtractDateTime(content, propertyname, regex, group, dateformat, false, null);
        }

    }
}
