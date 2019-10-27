using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;

namespace ForumParser
{
    /// <summary>
    /// Some extensions for various class types
    /// </summary>
    public static class Extensions
    {

        public static DateTime GetMonth(this DateTime date)
        {
            var temp = $"{date.ToString("yyyy-MM")}-01 00:00:00";
            return DateTime.ParseExact(temp, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        }

        private static Regex striphtml = new Regex("<.*?>", RegexOptions.Compiled);

        public static string RemoveHtml(this string html)
        {
            html = striphtml.Replace(html, "");
            html = WebUtility.HtmlDecode(html);

            html = html.Replace('\t', ' ');
            html = html.Replace("\r", "");
            html = html.Replace("\n", "\r\n");

            while (html.Contains("  "))
            {
                html = html.Replace("  ", " ");
            }

            html = html.Trim();

            return html;
        }

        public static string PrettyPrint(this TimeSpan span)
        {
            int years = span.Days / 365;
            int days = span.Days % 365;
            int months = days / 30;
            days = days % 30;

            List<string> parts = new List<string>();

            if (years > 0)
            {
                parts.Add($"{years} year(s)");
            }
            if (months > 0)
            {
                parts.Add($"{months} month(s)");
            }
            if (days > 0)
            {
                parts.Add($"{days} day(s)");
            }
            if (span.Hours > 0)
            {
                parts.Add($"{span.Hours} hour(s)");
            }
            if (span.Minutes > 0)
            {
                parts.Add($"{span.Minutes} minute(s)");
            }

            return parts.Count > 0 ? string.Join(", ", parts) : "0 minutes";
        }

        public static string HumanBytes(this long bytes)
        {
            decimal decbytes = Convert.ToDecimal(bytes);
            long ckbytes = 1024;
            long cmbytes = ckbytes * 1024;
            long cgbytes = cmbytes * 1024;
            long ctbytes = cgbytes * 1024;
            long cpbytes = ctbytes * 1024;

            if (decbytes >= cpbytes)
            {
                return (decbytes / cpbytes).ToString("#,###.00") + " Petabyte";
            }
            else if (decbytes >= ctbytes)
            {
                return (decbytes / ctbytes).ToString("#,###.00") + " Terabyte";
            }
            else if (decbytes >= cgbytes)
            {
                return (decbytes / cgbytes).ToString("#,###.00") + " Gigabyte";
            }
            else if (decbytes >= cmbytes)
            {
                return (decbytes / cmbytes).ToString("#,###.00") + " Megabyte";
            }
            else if (decbytes >= ckbytes)
            {
                return (decbytes / ckbytes).ToString("#,###.00") + " Kilobyte";
            }
            return decbytes.ToString("#,###") + " Bytes";
        }

    }
}
