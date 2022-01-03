using Newtonsoft.Json;
using PerryFlynn.ForumStatistics.Parser;
using PerrysNetConsole;
using PerrysNetConsoleHtml;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ForumParser
{
    /// <summary>
    /// Main program of the parser
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            if (!(args.Length > 2 && args[0].StartsWith("http") && args[1].EndsWith(".json") && args[2].EndsWith(".html")))
            {
                CoEx.WriteLine("Usage: ForumParser.exe http(s)://... output.json output.html");
                return;
            }

            CoEx.ForcedBufferWidth = 135;

            /*
            var url = @"https://forum.netcup.de/sonstiges/smalltalk/1051-das-l%C3%A4ngste-thema/";
            var path = @"H:\laengstes.json";
            var htmloutfile = @"H:\laengstes.html";
            */

            var forcecrawl = args.Length > 3 && args[3] == "--force";
            var url = args[0];
            var path = args[1];
            var htmloutfile = args[2];

            if (File.Exists(path) == false || forcecrawl)
            {
                Crawl(url, path);
            }

            PrintStats(path, htmloutfile);
        }

        private static void Crawl(string url, string path)
        {
            var parser = new ThreadParser(new NetcupThreadInfo(), new NetcupPostInfo(), new NetcupUserInfo(), new NetcupSearchInfo());

            var task = Task.Run(async () =>
            {
                var thread = await parser.ParseThreadAsync(new Uri(url));
                thread.Serialize(new FileInfo(path));
            });

            Task.WaitAll(task);
        }

        private static ForumThread Load(string jsonfile)
        {
            return JsonConvert.DeserializeObject<ForumThread>(File.ReadAllText(jsonfile));
        }

        private static void PrintStats(string path, string htmloutfile)
        {
            using (var thread = Load(path))
            using (var writer = new CoExHtmlWriter() { Title = "Das Längste" })
            {
                CoEx.WriteTitleLarge($"Statistics for {thread.StartpageUrl}");
                CoEx.WriteLine();

                CoEx.WriteLine(new string[] {
                    "If you don't want to appear in this statistic, ",
                    "just add the keyword 'donottrack' to your signature."
                });

                CoEx.WriteLine();

                //--> Filter functions
                bool filteroptoutpost(ThreadPost post)
                {
                    return (post?.User?.SignatureHtml?.RemoveHtml().Contains("donottrack", StringComparison.InvariantCultureIgnoreCase) ?? false) == false;
                }

                bool filteroptoutuser(ForumUser user)
                {
                    return (user?.SignatureHtml?.RemoveHtml().Contains("donottrack", StringComparison.InvariantCultureIgnoreCase) ?? false) == false;
                }

                //--> Default filters
                var cleanposts = thread.Posts.Where(v => !v.IsDeletedPost && filteroptoutpost(v));
                var cleanusers = thread.Users.Users
                    .Where(v => filteroptoutuser(v))
                    .Intersect(cleanposts.Select(u => u.User).Distinct());

                //--> Current status
                var curstats = new string[][]
                {
                    new string[] { "JSON file:", Path.GetFileName(path) },
                    new string[] { "JSON file size:", (new FileInfo(path)).Length.HumanBytes() },
                    new string[] { "Crawl date:", thread.CrawlTimestamp.ToString("yyyy-MM-dd HH:mm:ss") },
                    new string[] { "Post count:", cleanposts.Count().ToString("#,##0") },
                    new string[] { "Deleted posts count:", thread.Posts.Count(p => p.IsDeletedPost && filteroptoutpost(p)).ToString("#,##0") },
                    new string[] { "Users involved:", cleanusers.Count().ToString("#,##0") },
                    new string[] { "Banned users:", cleanusers.Where(u => u.IsBanned).Count().ToString("#,##0") },
                    new string[] { "Opt-Out users:", thread.Users.Users.Where(v=>filteroptoutuser(v)==false).Count().ToString("#,##0") },
                    new string[] { "Total likes:", cleanposts.Sum(v => v.LikeCount).ToString("#,##0") },
                    new string[] { "Total dislikes:", cleanposts.Sum(v => v.DislikeCount).ToString("#,##0") },
                    new string[] { "Posts with likes:", cleanposts.Where(v => v.LikeCount>0).Count().ToString("#,##0") },
                    new string[] { "Posts with dislikes:", cleanposts.Where(v => v.DislikeCount > 0).Count().ToString("#,##0") },
                    new string[] { "First post:", cleanposts.Min(v => v.Date).ToString("yyyy-MM-dd HH:mm:ss") },
                    new string[] { "Last post:", cleanposts.Max(v => v.Date).ToString("yyyy-MM-dd HH:mm:ss") },
                    new string[] { "Thread age:", (cleanposts.Max(v => v.Date)-cleanposts.Min(v=>v.Date)).PrettyPrint() },
                    new string[] { "First user:", cleanposts.Where(v => v.User!=null).GroupBy(v=>v.User).OrderBy(v=>v.Min(j=>j.Date)).Select(v=>$"{v.Key.Username} ({v.Count()})").First() },
                    new string[] { "Newest user:", cleanposts.Where(v => v.User!=null).GroupBy(v=>v.User).OrderByDescending(v=>v.Min(j=>j.Date)).Select(v=>$"{v.Key.Username} ({v.Count()})").First() }
                };

                CoEx.WriteTitle("Current status");
                CoEx.WriteTable(RowCollection.Create(curstats));
                CoEx.WriteLine();

                //--> User statistics
                var userstats = cleanposts
                    .Where(v => v.User != null)
                    .GroupBy(v => v.User)
                    .OrderByDescending(v => v.Count())
                    .Take(30)
                    .Select(v => new string[]
                    {
                        v.Key.Username,
                        v.Key.PostCount.ToString("#,###"),
                        v.Count().ToString("#,###"),
                        (100.0*v.Count()/v.Key.PostCount).ToString("0.0")+"%",
                        (100.0*v.Count()/cleanposts.Count()).ToString("0.0")+"%",
                        v.Min(j=>j.Date).ToString("yyyy-MM-dd HH:mm"),
                        v.Max(j=>j.Date).ToString("yyyy-MM-dd HH:mm"),
                        v.Key.MemberSince.ToString("yyyy-MM-dd")
                    })
                    .Prepend(new string[]
                    {
                        "Username",
                        "All Posts",
                        "Thread Posts",
                        "User %",
                        "Thread %",
                        "First Post",
                        "Last Post",
                        "Member since"
                    });

                var userrows = RowCollection.Create(userstats.ToArray());
                userrows.Settings.Border.Enabled = true;

                userrows.Settings.Align = (conf, colidx, s) =>
                {
                    switch (colidx)
                    {
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                            return RowCollectionSettings.ALIGN.RIGHT;
                        default:
                            return RowCollectionSettings.ALIGN.LEFT;
                    }
                };

                CoEx.WriteTitle("Statistics by user");
                CoEx.WriteTable(userrows);
                CoEx.WriteLine();


                //--> User like statistics
                var userlikes = cleanposts
                    .Where(v => v.User != null)
                    .GroupBy(v => v.User)
                    .Where(v => v.Sum(j => j.LikeCount) > 0)
                    .OrderByDescending(v => v.Sum(j => j.LikeCount))
                    .Take(10)
                    .Select(v => new string[]
                    {
                        v.Key.Username,
                        v.Sum(j=>j.LikeCount).ToString("#,##0"),
                        v.Where(j=>j.LikeCount>0).Min(j=>j.Date).ToString("yyyy-MM-dd HH:mm"),
                        v.Where(j=>j.LikeCount>0).Max(j=>j.Date).ToString("yyyy-MM-dd HH:mm")
                    })
                    .Prepend(new string[]
                    {
                        "Username",
                        "Likes",
                        "First like received",
                        "Last like received"
                    });

                var userlikerows = RowCollection.Create(userlikes.ToArray());
                userlikerows.Settings.Border.Enabled = true;

                userlikerows.Settings.Align = (conf, colidx, s) =>
                {
                    switch (colidx)
                    {
                        case 1:
                            return RowCollectionSettings.ALIGN.RIGHT;
                        default:
                            return RowCollectionSettings.ALIGN.LEFT;
                    }
                };

                CoEx.WriteTitle("Statistics by likes");
                CoEx.WriteTable(userlikerows);
                CoEx.WriteLine();


                //--> User title statistics
                var titlestats = cleanposts
                    .Where(v => v.User != null)
                    .GroupBy(v => v.User.Title)
                    .OrderByDescending(v => v.Count())
                    .Select(v => new string[]
                    {
                        v.Key,
                        v.Count().ToString("#,###"),
                        v.Min(j=>j.Date).ToString("yyyy-MM-dd HH:mm"),
                        v.Max(j=>j.Date).ToString("yyyy-MM-dd HH:mm")
                    })
                    .Prepend(new string[]
                    {
                        "Title",
                        "Posts",
                        "First Post",
                        "Last Post"
                    });

                var titlerows = RowCollection.Create(titlestats.ToArray());
                titlerows.Settings.Border.Enabled = true;

                titlerows.Settings.Align = (conf, colidx, s) =>
                {
                    switch (colidx)
                    {
                        case 1:
                            return RowCollectionSettings.ALIGN.RIGHT;
                        default:
                            return RowCollectionSettings.ALIGN.LEFT;
                    }
                };

                CoEx.WriteTitle("Statistics by user title");
                CoEx.WriteTable(titlerows);
                CoEx.WriteLine();


                //--> Post likes
                var likeposts = cleanposts
                    .Where(v => v.User != null)
                    .OrderByDescending(v => v.LikeCount)
                    .Take(10)
                    .Select(v => new string[]
                    {
                        v.Date.ToString("yyyy-MM-dd"),
                        v.LikeCount.ToString("#,###"),
                        v.User.Username,
                        v.Url
                    })
                    .Prepend(new string[]
                    {
                        "Date",
                        "Likes",
                        "Author",
                        "Post Url"
                    });

                var likepostrows = RowCollection.Create(likeposts.ToArray());
                likepostrows.Settings.Border.Enabled = true;

                likepostrows.Settings.Align = (conf, colidx, s) =>
                {
                    switch (colidx)
                    {
                        case 1:
                            return RowCollectionSettings.ALIGN.RIGHT;
                        default:
                            return RowCollectionSettings.ALIGN.LEFT;
                    }
                };

                CoEx.WriteTitle("Most liked posts");
                CoEx.WriteTable(likepostrows);
                CoEx.WriteLine();


                //--> Year statistics
                var yearmaxlength = cleanposts
                    .GroupBy(v => v.Date.Year)
                    .Select(v => v.Count().ToString().Length)
                    .Max();

                var yearstats = cleanposts
                    .GroupBy(v => v.Date.Year)
                    .Select(v => new string[]
                    {
                        v.Key.ToString(),
                        v.Min(j=>j.Date).ToString("MM-dd HH:mm"),
                        v.Max(j=>j.Date).ToString("MM-dd HH:mm"),
                        v.GroupBy(j=>j.User).Count().ToString("#,###"),
                        v.Count().ToString("#,###"),
                        string.Join(", ", v.GroupBy(j=>j.User).OrderByDescending(j=>j.Count()).Take(3).Select(j=>$"{j.Key?.Username??"Guest"} ({j.Count()})")),
                        v.Max(j=>j.MessageHtml.RemoveHtml().Length).ToString("#,###")
                    })
                    .Prepend(new string[]
                    {
                        "Year",
                        "First Post",
                        "Last Post",
                        "Active",
                        "Posts",
                        "Top Users",
                        "Largest Msg"
                    });

                var yeargraphstats = cleanposts
                    .GroupBy(v => v.Date.Year)
                    .Select(v => new dynamic[]
                    {
                        v.Key.ToString(),
                        v.Count(),
                    })
                    .OrderByDescending(v => (string)v[0])
                    .Take((int)((CoEx.ForcedBufferWidth.Value - yearmaxlength - 4) / 2))
                    .OrderBy(v => (string)v[0])
                    .ToDictionary(key => (string)key[0], value => (double)value[1]);

                var yearrows = RowCollection.Create(yearstats.ToArray());
                yearrows.Settings.Border.Enabled = true;

                yearrows.Settings.Align = (conf, colidx, s) =>
                {
                    switch (colidx)
                    {
                        case 3:
                        case 4:
                        case 6:
                            return RowCollectionSettings.ALIGN.RIGHT;
                        default:
                            return RowCollectionSettings.ALIGN.LEFT;
                    }
                };

                CoEx.WriteTitle("Statistics by year");
                var graph = new SimpleGraph() { Height = 15 };

                graph.Draw(yeargraphstats);
                CoEx.WriteLine();

                CoEx.WriteTable(yearrows);
                CoEx.WriteLine();

                //--> Month statistics
                var monthstats = cleanposts
                    .GroupBy(v => v.Date.GetMonth())
                    .Select(v => new string[]
                    {
                        v.Key.ToString("yyyy-MM"),
                        v.Min(j=>j.Date).ToString("dd HH:mm"),
                        v.Max(j=>j.Date).ToString("dd HH:mm"),
                        v.GroupBy(j=>j.User).Count().ToString("#,###"),
                        v.Count().ToString("#,###"),
                        string.Join(", ", v.GroupBy(j=>j.User).OrderByDescending(j=>j.Count()).Take(3).Select(j=>$"{j.Key?.Username??"Guest"} ({j.Count()})")),
                        v.Max(j=>j.MessageHtml.RemoveHtml().Length).ToString("#,###")
                    })
                    .Prepend(new string[]
                    {
                        "Month",
                        "First Post",
                        "Last Post",
                        "Active",
                        "Posts",
                        "Top Users",
                        "Largest Msg"
                    });

                var monthmaxlength = cleanposts
                    .GroupBy(v => v.Date.GetMonth())
                    .Select(v => v.Count().ToString().Length)
                    .Max();

                var monthgraphstats = cleanposts
                    .GroupBy(v => v.Date.GetMonth())
                    .Select(v => new dynamic[]
                    {
                        v.Key.ToString("yyMM"),
                        v.Count(),
                    })
                    .OrderByDescending(v => (string)v[0])
                    .Take((int)((CoEx.ForcedBufferWidth.Value - monthmaxlength - 4) / 2))
                    .OrderBy(v => (string)v[0])
                    .ToDictionary(key => (string)key[0], value => (double)value[1]);

                var monthrows = RowCollection.Create(monthstats.ToArray());
                monthrows.Settings.Border.Enabled = true;

                monthrows.Settings.Align = (conf, colidx, s) =>
                {
                    switch (colidx)
                    {
                        case 3:
                        case 4:
                        case 6:
                            return RowCollectionSettings.ALIGN.RIGHT;
                        default:
                            return RowCollectionSettings.ALIGN.LEFT;
                    }
                };

                CoEx.WriteTitle("Statistics by month");

                graph.Draw(monthgraphstats);
                CoEx.WriteLine();

                CoEx.WriteTable(monthrows);
                CoEx.WriteLine();

                writer.SaveAs(htmloutfile);
            }
        }

    }
}
