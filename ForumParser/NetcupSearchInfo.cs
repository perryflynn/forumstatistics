using PerryFlynn.ForumStatistics.Parser;

namespace ForumParser
{
    public class NetcupSearchInfo : IUserSearchInfo
    {
        public string SearchPageUrl => "https://forum.netcup.de/system/user-search/";

        public string SearchTokenRegex => "var SECURITY_TOKEN = '([^']+)';";

        public string SearchUrl => "https://forum.netcup.de/system/user-search/";

        public string SessionExpiredRegex => "Your session has expired, please submit the form again\\.";

        public string UserBlockXPath => "//ol[contains(@class, 'userList')]/li";
    }
}
