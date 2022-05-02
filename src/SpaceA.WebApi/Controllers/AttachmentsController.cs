using System;
using System.Threading.Tasks;
using SpaceA.Model.Entities;
using Microsoft.AspNetCore.Mvc;
using DTO = SpaceA.Model.Dto;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.StaticFiles;
using SpaceA.Repository.Context;
using SpaceA.Repository.Interfaces;
using Microsoft.Extensions.Configuration;
using SpaceA.WebApi.Services.Interfaces;
using SpaceA.Model.Mapper;
using Microsoft.Extensions.Logging;

namespace SpaceA.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttachmentsController : ControllerBase
    {
        private readonly SpaceAContext _context;
        private readonly ILogger _logger;
        private readonly Lazy<Member> _me;
        private readonly IAttachmentRepository _attachmentRepository;
        private readonly IMemberRepository _memberRepository;
        //private readonly string _resourceRoot;
        private readonly IContentService _contentService;

        public AttachmentsController(
            SpaceAContext context,
            ILogger<AttachmentsController> logger,
            ITokenService tokenService,
            IAttachmentRepository attachmentRepository,
            IMemberRepository memberRepository,
            IContentService contentService,
            IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _me = new Lazy<Member>(() => tokenService.GetMember(User));
            _attachmentRepository = attachmentRepository;
            _contentService = contentService;
            _memberRepository = memberRepository;
            //_resourceRoot = configuration["ResourceRoot"];
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            var id = Guid.NewGuid();
            //string dirPath = Path.Combine(_resourceRoot, id.ToString());
            //string filePath = Path.Combine(dirPath, file.FileName);
            var filePath = $"attachments/{id}/{file.FileName}";
            var now = DateTime.Now;
            var attachment = new Attachment
            {
                Id = id,
                FileName = file.FileName,
                Size = file.Length,
                UploadedTime = now,
                CreatorId = _me.Value.Id
            };
            using (var trans = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    _attachmentRepository.Add(attachment);
                    await _context.SaveChangesAsync();
                    //Directory.CreateDirectory(dirPath);
                    //using (var fs = new FileStream(filePath, FileMode.Create))
                    //{
                    //    await file.CopyToAsync(fs);
                    //    await fs.FlushAsync();
                    //}
                    await _contentService.UploadAsync(file, filePath);
                    await trans.CommitAsync();
                }
                catch (Exception ex)
                {
                    await trans.RollbackAsync();
                    _logger.LogError(ex.Message);
                    throw ex;
                }
            }
            var me = await _memberRepository.GetAsync(_me.Value.Id);
            return Ok(new DTO.Attachment
            {
                Id = attachment.Id,
                FileName = attachment.FileName,
                Size = attachment.Size,
                UploadedTime = attachment.UploadedTime,
                Creator = me.ToDto()
            });
        }

        [HttpGet("{id}/download")]
        public async Task<IActionResult> DownloadFile(Guid id, string fileName)
        {
            if (fileName == null)
            {
                //_attachmentRepository
            }
            //string filePath = Path.Combine(_resourceRoot, id.ToString(), fileName);
            //if (System.IO.File.Exists(filePath))
            //{
            //    var fileProvider = new FileExtensionContentTypeProvider();
            //    fileProvider.TryGetContentType(fileName, out var contentType);
            //    return PhysicalFile(filePath, contentType, fileName);
            //}
            //else
            //{
            //    return NotFound();
            //}
            var filePath = $"attachments/{id}/{fileName}";
            var stream = await _contentService.DownloadAsync(filePath);
            var fileProvider = new FileExtensionContentTypeProvider();
            fileProvider.TryGetContentType(fileName, out var contentType);
            return new FileStreamResult(stream, "application/octet-stream");
        }

        [HttpPut("{id}/attachto/{workItemId}")]
        public async Task<IActionResult> AttachToAsync(Guid id, uint workItemId)
        {
            await _attachmentRepository.UpdateWorkItemIdAsync(id, workItemId);
            return Ok();
        }

        [HttpPut("{id}/detach")]
        public async Task<IActionResult> DetachAsync(Guid id)
        {
            await _attachmentRepository.UpdateWorkItemIdAsync(id, null);
            return Ok();
        }
    }
}