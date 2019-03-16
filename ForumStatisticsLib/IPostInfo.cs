namespace PerryFlynn.ForumStatistics.Parser
{
    /// <summary>
    /// Informations required to parse a post
    /// </summary>
    public interface IPostInfo
    {
        string RegexUid { get; }
        string RegexPostUrl { get; }
        string RegexUserUrl { get; }
        string RegexUserUid { get; }
        string RegexDate { get; }
        string DateFormat { get; }
        string XpathMessageHtml { get; }
        string XpathSignature { get; }
        string RegexLikeCount { get; }
        string RegexDislikeCount { get; }

        string RegexGuestUsername { get; }
        string RegexGuestTitle { get; }
    }
}
