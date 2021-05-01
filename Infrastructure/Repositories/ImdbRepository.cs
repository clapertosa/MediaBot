using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;

namespace Infrastructure.Repositories
{
    public class ImdbRepository : IImdbRepository
    {
        private readonly IHtmlUtils _htmlUtils;
        private readonly string _searchUrl = $"https://www.imdb.com/find?q=";

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
    }
}