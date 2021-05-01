namespace Application.HtmlPaths
{
    public class ImdbPaths
    {
        // Search
        public const string FindListTitlesHeader = "//a[@name='tt']";
        public const string FindListTable = "//table[@class='findList']";
        // Media
        public const string TitleBlockContainerTitle = "//h1[contains(@class, 'TitleHeader__TitleText')]";
        public const string TitleBlockContainerMetaDataList = "//div[contains(@class, 'TitleBlock__TitleContainer')]//li"; // First child
        public const string Plot = "//div[contains(@class, 'GenresAndPlot__TextContainer')]";
        public const string Poster = "//div[contains(@class, 'ipc-poster')]//img";
        public const string Genres = "//a[contains(@class, 'GenresAndPlot__Genre')]//span";
        public const string ActorNamesAnchor = "//a[contains(@class, 'ActorName')]"; // Href
        public const string Vote = "//span[contains(@class, 'AggregateRatingButton__RatingScore')]";
        public const string VotesNumber = "//div[contains(@class, 'AggregateRatingButton__TotalRatingAmount')]";
        public const string DirectorAnchor = "//span[contains(text(), 'Director')]/parent::li//a"; // Href
        public const string ReleaseDate = "//a[contains(text(), 'Release date')]/parent::li/div";
    }
}