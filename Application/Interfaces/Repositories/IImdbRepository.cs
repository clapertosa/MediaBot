using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface IImdbRepository
    {
        Task<IEnumerable<Media>> Search(string title);
        Task<Media> GetMedia(string url);
        Task<IEnumerable<Media>> GetUserMedia(IUser user);
    }
}