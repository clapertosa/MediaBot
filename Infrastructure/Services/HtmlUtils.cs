using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Application.HtmlPaths;
using Application.Interfaces.Services;
using Domain.Entities;
using HtmlAgilityPack;

namespace Infrastructure.Services
{
    public class HtmlUtils : IHtmlUtils
    {
        private static HttpClient _client;
        private readonly HtmlWeb _web = new HtmlWeb();
        private readonly string _imdbMediaUrl = "https://www.imdb.com";

        private string GetId(string url)
        {
            int startIndex = url.Substring(1).IndexOf("/", StringComparison.Ordinal) + 2;
            int endIndex = url.LastIndexOf("/", StringComparison.Ordinal);
            return url[startIndex..endIndex];
        }

        private int GetYearFromTitle(string title)
        {
            string pattern = @"(\d+)";
            int.TryParse(Regex.Match(title, pattern).Value, out int number);
            return number;
        }

        private string GetPosterOriginalSize(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath)) return "";
            int extensionStartIndex = imagePath.LastIndexOf(".", StringComparison.Ordinal) + 1;
            int sizesStartIndex = imagePath.IndexOf("@.", StringComparison.Ordinal);
            if (sizesStartIndex <= 0) return imagePath;
            sizesStartIndex += 2;
            string extension = imagePath[extensionStartIndex..imagePath.Length];
            return imagePath[..sizesStartIndex] + extension;
        }

        private List<Media> ParseTableResults(HtmlNode htmlNode)
        {
            HtmlNodeCollection columns = htmlNode.SelectNodes(".//tr");
            List<Media> media = new List<Media>();

            foreach (HtmlNode column in columns)
            {
                string id = GetId(column.SelectSingleNode(".//a").Attributes["href"].Value);
                string title = column.InnerText.Trim();
                int year = GetYearFromTitle(title);
                string posterPath = GetPosterOriginalSize(column.SelectSingleNode(".//img").Attributes["src"].Value);
                Media m = new Media
                {
                    Id = id,
                    Title = title,
                    Year = year,
                    PosterPath = posterPath,
                    Url = $"{_imdbMediaUrl}/title/{id}"
                };

                media.Add(m);
            }

            return media;
        }

        private Media ParseMediaPage(HtmlNode htmlNode, string url)
        {
            double.TryParse(htmlNode.SelectSingleNode(ImdbPaths.Vote)?.InnerText, NumberStyles.Float,
                CultureInfo.InvariantCulture, out double rating);

            Media media = new Media
            {
                Id = GetId(url),
                Title = htmlNode.SelectSingleNode(ImdbPaths.TitleBlockContainerTitle)?.InnerText,
                Plot = WebUtility.HtmlDecode(htmlNode.SelectSingleNode(ImdbPaths.Plot)?.FirstChild?.InnerText),
                PosterPath =
                    GetPosterOriginalSize(htmlNode.SelectSingleNode(ImdbPaths.Poster)?.Attributes["src"].Value),
                Director = new Director
                {
                    FullName = htmlNode.SelectSingleNode(ImdbPaths.DirectorAnchor)?.InnerText,
                    Url =
                        $"{_imdbMediaUrl}{htmlNode.SelectSingleNode(ImdbPaths.DirectorAnchor)?.Attributes["href"].Value}"
                },
                ReleaseDate = htmlNode.SelectSingleNode(ImdbPaths.ReleaseDate)?.InnerText,
                Url = url,
                Vote = rating,
                VotesNumber = htmlNode.SelectSingleNode(ImdbPaths.VotesNumber)?.InnerText,
            };

            // Genres
            foreach (var genre in htmlNode.SelectNodes(ImdbPaths.Genres))
            {
                media.Genres.Add(genre.InnerText);
            }

            // Actors
            var nodes = htmlNode.SelectNodes(ImdbPaths.ActorNamesAnchor);
            int actorsIndex = nodes.Count > 5 ? 5 : nodes.Count;
            for (int i = 0; i < actorsIndex; i++)
            {
                string actorFullName = nodes[i]?.InnerText;
                string actorUrl = $"{_imdbMediaUrl}{nodes[i].Attributes["href"].Value}";
                media.Actors.Add(new Actor {FullName = actorFullName, Url = actorUrl});
            }

            // MetaData
            foreach (var metaData in htmlNode.SelectNodes(ImdbPaths.TitleBlockContainerMetaDataList))
            {
                media.MetaData.Add(metaData.FirstChild.InnerText);
            }

            return media;
        }

        public async Task<HtmlDocument> GetHtmlDocAsync(string url)
        {
            _web.UserAgent =
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36";
            ///<summary>
            /// Get the beta UI
            /// </summary>
            CookieContainer cookieContainer = new CookieContainer();
            cookieContainer.Add(new Uri(url),
                new Cookie
                {
                    Name = "beta-control", Value = "tmd=in", Domain = ".imdb.com", HttpOnly = false, Secure = true
                });
            _client = new HttpClient(new HttpClientHandler {CookieContainer = cookieContainer});
            var stream = await _client.GetStreamAsync(url);
            var doc = new HtmlDocument();
            doc.Load(stream);
            return doc;
        }

        public List<Media> GetMediaList(HtmlDocument htmlDoc)
        {
            /// <summary>
            /// Check if div tag with class "findNoResults" exists and if it does return an empty list of Media
            /// </summary>
            bool hasResults = htmlDoc.DocumentNode.SelectSingleNode(ImdbPaths.FindListTitlesHeader) != null;
            if (!hasResults) return new List<Media>();
            /// <summary>
            /// Parse table with results and return a media List.
            /// </summary>
            var media = ParseTableResults(htmlDoc.DocumentNode.SelectSingleNode(ImdbPaths.FindListTable));
            return media;
        }

        public Media GetMediaInfo(HtmlDocument htmlDoc, string url)
        {
            HtmlNode htmlNode = htmlDoc.DocumentNode;
            return ParseMediaPage(htmlNode, url);
        }
    }
}