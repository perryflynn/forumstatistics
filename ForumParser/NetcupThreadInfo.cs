using PerryFlynn.ForumStatistics.Parser;
using System;

namespace ForumParser
{
    /// <summary>
    /// Regular expressions to parse a thread inside the netcup customer forum
    /// </summary>
    public class NetcupThreadInfo : IThreadInfo
    {
        public string RegexStartUrl => "<meta property=\"og:url\" content=\"([^\"]+)\">";
        public string RegexPageMaxCount => "<nav class=\"pagination\"[^>]+ data-pages=\"([0-9]+)\">";

        public Func<string, uint, string> BuildPageUrlFunc => (url, pagenum) =>
        {
            return $"{url}?pageNo={pagenum}";
        };

        public string XpathPosts => "//ul[contains(@class, 'wbbThreadPostList')]/li/article";
        public string RegexThreadUid => "data-thread-id=\"([0-9]+)\"";
        public string RegexCurrentPageNo => "<li class=\"active\"><span>[0-9]+</span><span class=\"invisible\">Page ([0-9,]+) of [0-9,]+</span></li>";

        public Func<string, double> NormalizeNumbersFunc => (number) =>
        {
            var temp = number.Trim().Replace(",", "");

            if(double.TryParse(temp, out double parsedNumber))
            {
                return parsedNumber;
            }

            throw new ArgumentException($"Cannot parse '{number}' into a number");
        };
    }
}
