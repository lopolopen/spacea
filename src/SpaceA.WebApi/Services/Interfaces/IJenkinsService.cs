using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using SpaceA.Model.Entities;

namespace SpaceA.WebApi.Services.Interfaces
{
    public interface IJenkinsService
    {
        Task CreateJobAsync(string httpUrlToRepo, string path, string name);

        Task DisableAsync(string groupName, string name);

        Task EnableAsync(string groupName, string name);
    }
}