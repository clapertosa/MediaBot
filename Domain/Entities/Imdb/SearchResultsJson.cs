using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Domain.Entities.Imdb

{
    public class SearchResultsJson
    {
        [JsonPropertyName("d")] public IEnumerable<SearchResultJson> ResultList { get; set; }

        public class SearchResultJson
        {
            public string Id { get; set; }
            [JsonPropertyName("l")] public string Title { get; set; }
            public long Rank { get; set; }
            [JsonPropertyName("s")] public string Stars { get; set; }
            [JsonPropertyName("y")] public int Year { get; set; }
            [JsonPropertyName("q")] public string Type { get; set; }
            [JsonPropertyName("i")] public PosterJson Poster { get; set; }
        }

        public class PosterJson
        {
            public int Width { get; set; }
            public int Height { get; set; }
            public string ImageUrl { get; set; }
        }
    }
}