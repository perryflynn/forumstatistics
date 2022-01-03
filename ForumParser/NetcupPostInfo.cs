using PerryFlynn.ForumStatistics.Parser;

namespace ForumParser
{
    /// <summary>
    /// Expessions reuired to parse a post inside the netcup customer forum
    /// </summary>
    public class NetcupPostInfo : IPostInfo
    {
        public string IsDeletedRegex => "wbbPostDeleted";
        public string RegexUid => "data-post-id=\"([0-9]+)\"";
        public string RegexPostUrl => "itemid=\"([^\"]+)\"";
        public string RegexUserUid => "data-user-id=\"([0-9]+)\"";
        public string RegexUserUrl => "(?:<a href=\"([^\"]+)\" class=\"username userLink\" data-user-id=\"[0-9]+\" itemprop=\"url\">|<h2 class=\"messageTitle\">Post by <a href=\"([^\"]+)\">[^<]+</a>)";
        public int[] RegexUserUrlGroups => new int[] { 1, 2 };
        public string RegexUsername => "(?:<span(?: class=\"username\")? itemprop=\"name\">(?:<strong>)?([^<]+)(?:</strong>)?</span>|<h2 class=\"messageTitle\">Post by <a href=\"[^\"]+\">([^<]+)</a>)";
        public int[] RegexUsernameGroups => new int[] { 1, 2 };
        public string RegexDate => "<meta itemprop=\"dateCreated\" content=\"([^\"]+)\">";
        public string DateFormat => "yyyy-MM-dd\\THH:mm:sszzz"; // 2018-10-16T11:16:08+02:00
        public string XpathMessageHtml => "//div[contains(@class, 'messageContent')]/div[contains(@class, 'messageBody')]";
        public string XpathSignature => "//footer[contains(@class, 'messageFooter')]/div[contains(@class, 'messageSignature')]/div";

        public string RegexGuestUsername => "<span class=\"username\" itemprop=\"name\">(?:<strong>)?([^<]+)(?:</strong>)?</span>";
        public string RegexGuestTitle => "<span class=\"badge\">([^<]+)</span>";

        public string RegexLikeCount => "data-like-likes=\"([0-9]+)\"";
        public string RegexDislikeCount => "data-like-dislikes=\"([0-9]+)\"";

    }
}
