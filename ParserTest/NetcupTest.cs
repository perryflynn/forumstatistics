using ForumParser;
using PerryFlynn.ForumStatistics.Parser;
using System;
using System.Globalization;
using System.IO;
using Xunit;
using System.Linq;

namespace ParserTest
{
    /// <summary>
    /// Some unit tests for the parser
    /// </summary>
    public class NetcupTest
    {

        /// <summary>
        /// Test the profile parser
        /// </summary>
        /// <param name="teststr">Profile URL</param>
        /// <param name="username">Expected username</param>
        /// <param name="uid">Expected UID</param>
        /// <param name="regdate">Expected register date</param>
        /// <param name="title">Expected user title</param>
        /// <param name="zero">Expect no posts for this user</param>
        [Theory]
        [InlineData("https://forum.netcup.de/user/1320-perryflynn/", "perryflynn", 1320, "2008-12-27", "Enlightened", false)]
        [InlineData("https://forum.netcup.de/user/3-netcup-felix/", "[netcup] Felix", 3, "2008-02-29", "Geschäftsführer", false)]
        [InlineData("https://forum.netcup.de/user/4-admin/", "admin", 4, "2008-02-29", null, true)]
        public void TestParseUser(string teststr, string username, uint uid, string regdate, string title, bool zero)
        {
            var usersearch = new SearchPageParser(new NetcupSearchInfo());
            var parser = new UserParser(new NetcupUserInfo());

            var users = new ForumUserCollection(parser);
            foreach(var newUser in usersearch.SearchUser(username).Result)
            {
                users.Import(newUser);
            }

            var user = users.Get(username);

            Assert.Equal<uint>(uid, user.Uid);
            Assert.Equal(teststr, user.Url);
            Assert.Equal(username, user.Username);
            Assert.Equal(DateTime.ParseExact(regdate, "yyyy-MM-dd", CultureInfo.InvariantCulture), user.MemberSince);
            Assert.Equal(title, user.Title);
            Assert.True(zero ? user.PostCount == 0 : user.PostCount > 0);
        }

        /// <summary>
        /// Test guest posts parsing
        /// </summary>
        /// <param name="url">Page with guest post</param>
        [Theory]
        [InlineData("https://forum.netcup.de/sonstiges/smalltalk/p13789-das-l%C3%A4ngste-thema/")]
        public void TestGuestUserPost(string url)
        {
            var parser = new ThreadParser(new NetcupThreadInfo(), new NetcupPostInfo(), new NetcupUserInfo(), new NetcupSearchInfo());
            var thread = parser.ParseThread(new Uri(url), 30, 30).Result;

            Assert.Contains(thread.Posts, v => v.IsGuestPost && string.IsNullOrEmpty(v.GuestUsername) == false);
        }

        /// <summary>
        /// Test parsing a thread (the first 3 pages)
        /// </summary>
        /// <param name="teststr">Thread url</param>
        [Theory]
        [InlineData("https://forum.netcup.de/sonstiges/smalltalk/1051-das-l%C3%A4ngste-thema/?pageNo=760")]
        public void TestParseThread(string teststr)
        {
            var parser = new ThreadParser(new NetcupThreadInfo(), new NetcupPostInfo(), new NetcupUserInfo(), new NetcupSearchInfo());
            var thread = parser.ParseThread(new Uri(teststr), 1, 3).Result;

            var postUsers = thread.Posts.Where(v => v.User != null).Select(v => v.User).Distinct();
            var userIntersects = thread.Users.Users.Intersect(postUsers);

            Assert.Equal("https://forum.netcup.de/sonstiges/smalltalk/1051-das-l%C3%A4ngste-thema/", thread.StartpageUrl);
            Assert.True(thread.PageCount >= 383);
            Assert.True(thread.CurrentPageNo <= thread.PageCount && thread.CurrentPageNo >= 1);
            Assert.True(thread.Uid > 0);
            Assert.True(thread.Posts.Count > 0);
            Assert.True(thread.Users.Users.Count > 0);
            Assert.Equal<int>(postUsers.Count(), userIntersects.Count());
        }

        /// <summary>
        /// Test the post parser
        /// </summary>
        /// <param name="url">URL with posts to parse</param>
        /// <param name="page">Target page inside the thread</param>
        /// <param name="postuid">ID of the post to test</param>
        /// <param name="haslikes">Has this post likes</param>
        /// <param name="hassiganture">Has this post signatures</param>
        /// <param name="hasuser">Has this post a user profile</param>
        [Theory]
        [InlineData("https://forum.netcup.de/sonstiges/smalltalk/p102253-das-l%C3%A4ngste-thema/#post102253", 767, 102253, true, true, true)]
        [InlineData("https://forum.netcup.de/sonstiges/smalltalk/p102260-das-l%C3%A4ngste-thema/#post102260", 767, 102260, false, false, true)]
        [InlineData("https://forum.netcup.de/sonstiges/smalltalk/p13789-das-l%C3%A4ngste-thema/#post13789", 30, 13789, false, false, false)]
        public void TestPostParser(string url, uint page, uint postuid, bool haslikes, bool hassiganture, bool hasuser)
        {
            var parser = new ThreadParser(new NetcupThreadInfo(), new NetcupPostInfo(), new NetcupUserInfo(), new NetcupSearchInfo());
            var thread = parser.ParseThread(new Uri(url.Substring(0, url.IndexOf('#'))), page, page).Result;

            var post = thread.Posts.Where(v => v.Uid == postuid).Single();

            Assert.Equal<uint>(postuid, post.Uid);
            Assert.Equal(url, post.Url);
            Assert.True(haslikes ? (post.LikeCount > 0) : true);
            Assert.True(post.DislikeCount < 1);
            Assert.Equal<bool>(hasuser, post.User != null);
            Assert.Equal<bool>(hassiganture, !string.IsNullOrEmpty(post.User?.SignatureHtml));
        }

    }
}
