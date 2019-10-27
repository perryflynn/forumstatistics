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

        public async Task<ForumUser> ParseUserPageAsync(Uri userpageurl)
        {
            Thread.Sleep(200);
            var response = await this.Client.GetAsync(userpageurl);
            var html = await response.Content.ReadAsStringAsync();
            return await this.ParseUserPageAsync(html);
        }

        public async Task<ForumUser> ParseUserPageAsync(string usersitehtml)
        {
            var datestr = await this.ExtractStringAsync(usersitehtml, "registration date", this.Info.RegexRegistrationDateString, 1);
            DateTime date = this.Info.RegistrationDateParseFunc(datestr);

            return new ForumUser()
            {
                Uid = (await this.ExtractUnsignedIntAsync(usersitehtml, "user uid", this.Info.RegexUid, 1)).Value,
                Username = await this.ExtractStringAsync(usersitehtml, "username", this.Info.RegexUsername, 1),
                Title = await this.ExtractStringAsync(usersitehtml, "user title", this.Info.RegexUserTitle, 1, true, null),
                PostCount = await this.ExtractPostcountAsync(usersitehtml),
                MemberSince = date,
                Url = await this.ExtractStringAsync(usersitehtml, "page url", this.Info.RegexUrl, 1),
                IsBanned = (await this.ExtractStringAsync(usersitehtml, "user is banned", this.Info.RegexIsBanned, 1, true, null)) != null
            };
        }

        protected virtual async Task<uint> ExtractPostcountAsync(string usersitehtml)
        {
            string strpostcount = await this.ExtractStringAsync(usersitehtml, "post count", this.Info.RegexPostCount, 1, true, "nohit");
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
