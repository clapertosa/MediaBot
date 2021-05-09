using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;

namespace Infrastructure.Repositories
{
    public class ImdbRepository : IImdbRepository
    {
        private readonly IHtmlUtils _htmlUtils;
        private readonly string _imdbUrl = "https://www.imdb.com/title/";
        private readonly string _searchUrl = "https://www.imdb.com/find?q=";

        public ImdbRepository(IHtmlUtils htmlUtils)
        {
            _htmlUtils = htmlUtils;
        }

        public async Task<List<Media>> Search(string title)
        {
            string url = $"{_searchUrl}{title}";
            var htmlDoc = await _htmlUtils.GetHtmlDocAsync(url);
            return _htmlUtils.GetMediaList(htmlDoc);
        }

        public async Task<Media> GetMedia(string url)
        {
            var htmlDoc = await _htmlUtils.GetHtmlDocAsync(url);
            return _htmlUtils.GetMediaInfo(htmlDoc, url);
        }

        public async Task<List<Media>> GetUserMedia(ulong userId, string connectionString)
        {
            List<Media> media = new List<Media>();
            string storedProcedure = "SP_GetUserMedia";
            await using var sqlConnection =
                new SqlConnection(connectionString + "discord_imdbot");
            await using var sqlCommand = new SqlCommand(storedProcedure, sqlConnection)
                {CommandType = CommandType.StoredProcedure};

            sqlCommand.Parameters.Add(new SqlParameter("@UserId", SqlDbType.BigInt) {Value = userId});

            await sqlConnection.OpenAsync();
            await using SqlDataReader sqlReader = await sqlCommand.ExecuteReaderAsync();
            while (sqlReader.Read())
            {
                media.Add(new Media
                {
                    Id = sqlReader.GetString(0), Title = sqlReader.GetString(1), PosterPath = sqlReader.GetString(2),
                    Url = _imdbUrl + sqlReader.GetString(0)
                });
            }

            return media;
        }
    }
}