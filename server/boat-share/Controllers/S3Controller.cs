using Amazon.S3;
using Amazon.S3.Model;
using boat_share.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;

namespace boat_share.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class S3Controller : ControllerBase
    {
        private readonly S3Service _s3Service;

        public S3Controller(S3Service s3Service)
        {
            _s3Service = s3Service;
        }

        // POST: api/s3/upload
        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file.Length > 0)
            {
                using (var stream = file.OpenReadStream())
                {
                    await _s3Service.UploadFileAsync(file.FileName, stream);
                }
                return Ok("File uploaded successfully.");
            }
            return BadRequest("No file uploaded.");
        }

        // GET: api/s3/download/{key}
        [HttpGet("download/{key}")]
        public async Task<IActionResult> DownloadFile(string key)
        {
            var fileStream = await _s3Service.DownloadFileAsync(key);
            return File(fileStream, "application/octet-stream", key);
        }

        // DELETE: api/s3/delete/{key}
        [HttpDelete("delete/{key}")]
        public async Task<IActionResult> DeleteFile(string key)
        {
            await _s3Service.DeleteFileAsync(key);
            return Ok("File deleted successfully.");
        }
    }
}
