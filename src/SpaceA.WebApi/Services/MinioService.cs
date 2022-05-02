using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;
using Minio.Exceptions;
using SpaceA.WebApi.Options;
using SpaceA.WebApi.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SpaceA.WebApi.Services
{
    public class MinioService : IContentService
    {
        private readonly MinioOptions _minioOptions;
        private readonly ILogger _logger;

        public MinioService(IOptions<MinioOptions> minioOptions, ILogger<MinioService> logger)
        {
            _logger = logger;
            _minioOptions = minioOptions.Value;
        }

        public async Task UploadAsync(IFormFile file, string targetPath, CancellationToken token = default)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }
            if (targetPath == null)
            {
                throw new ArgumentNullException(nameof(targetPath));
            }
            var opt = _minioOptions;
            var minioClient = new MinioClient(opt.Endpoint, opt.AccessKey, opt.SecretKey);
            using (var stream = file.OpenReadStream())
            {
                await minioClient.PutObjectAsync(opt.Bucket, targetPath, stream, file.Length, file.ContentType, cancellationToken: token);
            }
        }

        public async Task<Stream> DownloadAsync(string targetPath, CancellationToken token = default)
        {
            var opt = _minioOptions;
            var minioClient = new MinioClient(opt.Endpoint, opt.AccessKey, opt.SecretKey);
            //var tcs = new TaskCompletionSource<Stream>();
            var responseStream = new MemoryStream();
            try
            {
                await minioClient.GetObjectAsync(opt.Bucket, targetPath, stream =>
                {
                    stream.CopyTo(responseStream);
                    responseStream.Position = 0;
                    responseStream.SetLength(stream.Length);
                },
                cancellationToken: token);
            }
            catch (Exception ex)
            {
                //tcs.SetException(ex);
            }
            return responseStream;
        }

        public async Task RemoveAsync(string targetPath)
        {
            var opt = _minioOptions;
            var minio = new MinioClient(opt.Endpoint, opt.AccessKey, opt.SecretKey);
            try
            {
                await minio.RemoveObjectAsync(opt.Bucket, targetPath);
            }
            catch (MinioException ex)
            {
                _logger.LogError(ex.Message);
            }
        }
    }
}
