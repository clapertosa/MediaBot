using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities;
using HtmlAgilityPack;

namespace Application.Interfaces.Services
{
    public interface IHtmlUtils
    {
        Task<HtmlDocument> GetHtmlDocAsync(string url);
        List<Media> GetMediaList(HtmlDocument htmlDoc);
        Media GetMediaInfo(HtmlDocument htmlDoc, string url);
    }
}