using System;

namespace PerryFlynn.ForumStatistics.Parser
{
    /// <summary>
    /// Informations required to parse a thread
    /// </summary>
    public interface IThreadInfo
    {
        string RegexThreadUid { get; }
        string RegexStartUrl { get; }
        string RegexCurrentPageNo { get; }
        string RegexPageMaxCount { get; }
        string XpathPosts { get; }
        Func<string, uint, string> BuildPageUrlFunc { get; }
    }
}
