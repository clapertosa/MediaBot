using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface IImdbRepository
    {
        Task<List<Media>> Search(string title);
        Task<Media> GetMedia(string url);
    }
}