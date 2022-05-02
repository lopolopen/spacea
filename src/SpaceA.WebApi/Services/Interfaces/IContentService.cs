using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SpaceA.WebApi.Services.Interfaces
{
    public interface IContentService
    {
        Task UploadAsync(IFormFile file, string targetPath, CancellationToken token = default);

        Task<Stream> DownloadAsync(string targetPath, CancellationToken token = default);

        Task RemoveAsync(string targetPath);
    }
}
