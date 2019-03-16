using System;

namespace PerryFlynn.ForumStatistics.Parser
{
    /// <summary>
    /// Informations to parse a user profile page
    /// </summary>
    public interface IUserInfo
    {
        string RegexUid { get; }
        string RegexUsername { get; }
        string RegexUserTitle { get; }
        string RegexRegistrationDateString { get; }
        string RegexPostCount { get; }
        string RegexUrl { get; }
        Func<string, DateTime> RegistrationDateParseFunc { get; }
    }
}
