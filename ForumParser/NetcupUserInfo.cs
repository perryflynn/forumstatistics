using PerryFlynn.ForumStatistics.Parser;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Linq;

namespace ForumParser
{
    /// <summary>
    /// Regular expressions to parse a user page inside the netcup customer forum
    /// </summary>
    public class NetcupUserInfo : IUserInfo
    {
        // regex for user detail page
        //public string RegexUid => "<link rel=\"canonical\" href=\".+/user/([0-9]+)[^/]*?/\">";
        //public string RegexUsername => "<meta property=\"profile:username\" content=\"([^\"]+)\">";
        //public string RegexUrl => "<meta property=\"og:url\" content=\"([^\"]+)\">";

        public string RegexUid => "data-object-id=\"([0-9]+)\"";
        public string RegexUsername => "class=\"username userLink\" data-user-id=\"[0-9]+\">([^<]+)</a>";
        public string RegexUserTitle => "<span class=\"[^\"]*userTitleBadge[^\"]*\">([^<]+)</span>";
        public string RegexRegistrationDateString => "<li>Member since ([^<]+)</li>";
        public string RegexPostCount => "<dt><a href=\"[^\"]+\" title=\"Posts by [^\"]+\" class=\"jsTooltip\">Posts</a></dt>\\s*<dd>([0-9,]+)</dd>";
        public string RegexUrl => "<h3><a href=\"([^\"]+)\" class=\"username userLink\"";
        public string RegexIsBanned => "The user “[^”]+” (has been banned).";

        public Func<string, DateTime> RegistrationDateParseFunc => (str) =>
        {
            var rgx = new Regex(@"(1|21|31)st|(2|22)nd|(3|23)rd|([0-9]+)th");
            if (rgx.IsMatch(str))
            {
                var temp = rgx.Match(str);
                var search = temp.Groups[0].Value;
                var replace = temp.Groups.Where(v => int.TryParse(v.Value, out int foo)).Single().Value;

                str = str.Replace(search, replace);
            }
            return DateTime.ParseExact(str, "MMM d yyyy", CultureInfo.InvariantCulture);
        };
    }
}
