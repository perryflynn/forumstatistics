using System;
using System.Threading;
using System.Threading.Tasks;

namespace PerryFlynn.ForumStatistics.Parser
{
    /// <summary>
    /// Parse a user profile page
    /// </summary>
    public class UserParser : Parser
    {

        public IUserInfo Info { get; set; }

        public UserParser(IUserInfo info)
        {
            this.Info = info;
        }

        public async Task<ForumUser> ParseUserPage(Uri userpageurl)
        {
            Thread.Sleep(200);
            var response = await this.Client.GetAsync(userpageurl);
            var html = await response.Content.ReadAsStringAsync();
            return await this.ParseUserPage(html);
        }

        public async Task<ForumUser> ParseUserPage(string usersitehtml)
        {
            var datestr = await this.ExtractString(usersitehtml, "registration date", this.Info.RegexRegistrationDateString, 1);
            DateTime date = this.Info.RegistrationDateParseFunc(datestr);

            return new ForumUser()
            {
                Uid = (await this.ExtractUnsignedInt(usersitehtml, "user uid", this.Info.RegexUid, 1)).Value,
                Username = await this.ExtractString(usersitehtml, "username", this.Info.RegexUsername, 1),
                Title = await this.ExtractString(usersitehtml, "user title", this.Info.RegexUserTitle, 1, true, null),
                PostCount = await this.ExtractPostcount(usersitehtml),
                MemberSince = date,
                Url = await this.ExtractString(usersitehtml, "page url", this.Info.RegexUrl, 1),
                IsBanned = (await this.ExtractString(usersitehtml, "user is banned", this.Info.RegexIsBanned, 1, true, null)) != null
            };
        }

        protected virtual async Task<uint> ExtractPostcount(string usersitehtml)
        {
            string strpostcount = await this.ExtractString(usersitehtml, "post count", this.Info.RegexPostCount, 1, true, "nohit");
            if (uint.TryParse(strpostcount.Replace(",", ""), out uint postcount) == false)
            {
                postcount = 0;
            }
            return postcount;
        }

        public override void Dispose()
        {
            this.Info = null;
        }

    }
}
