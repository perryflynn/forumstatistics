namespace PerryFlynn.ForumStatistics.Parser
{
    public interface IUserSearchInfo
    {
        string SearchPageUrl { get; }
        string SearchTokenRegex { get; }
        string SearchUrl { get; }
        string SessionExpiredRegex { get; }
        string UserBlockXPath { get; }
    }
}