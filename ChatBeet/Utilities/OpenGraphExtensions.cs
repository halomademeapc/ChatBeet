﻿using GravyBot;
using HtmlAgilityPack;
using OpenGraphNet;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChatBeet.Utilities
{
    public static class OpenGraphExtensions
    {
        public static string ToIrcSummary(this OpenGraph meta, int maxDescriptionLength = 50)
        {
            IEnumerable<string> GetSegments()
            {
                if (!string.IsNullOrEmpty(meta.Title))
                {
                    yield return $"{IrcValues.BOLD}{HttpUtility.HtmlDecode(meta.Title)}{IrcValues.RESET}";
                }
                else
                {
                    // try to get title from title tag since opengraph is missing
                    var htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(meta.OriginalHtml);
                    var title = htmlDoc.DocumentNode.Descendants("title").FirstOrDefault();
                    if (title != default && !string.IsNullOrEmpty(title.InnerText))
                        yield return $"{IrcValues.BOLD}{HttpUtility.HtmlDecode(title.InnerText)}{IrcValues.RESET}";
                }
                if (meta.Metadata.ContainsKey("og:description"))
                {
                    var description = HttpUtility.HtmlDecode(meta.Metadata["og:description"].Select(m => m.Value?.Trim()).FirstOrDefault(v => !string.IsNullOrEmpty(v)));
                    if (description != default)
                    {
                        if (description.Length > maxDescriptionLength)
                            yield return $"{description.Substring(0, maxDescriptionLength).Trim()}…";
                        else
                            yield return description;
                    }
                }
            }
            return string.Join(" | ", GetSegments());
        }
    }
}