using FileSystemCommon;
using FileSystemWeb.Data;
using FileSystemWeb.Exceptions;
using FileSystemWeb.Extensions.Http;
using FileSystemWeb.Helpers;
using FileSystemWeb.Models;
using FileSystemWeb.Models.RequestBodies;
using FileSystemWeb.Services.File;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FileSystemWeb.Controllers.API
{
    [Route("api/[controller]")]
    [Authorize]
    public class BigFileController : Controller
    {
        private readonly BigFileService bigFileService;

        public BigFileController(BigFileService bigFileService)
        {
            this.bigFileService = bigFileService;
        }

        [HttpPost("start")]
        [HttpPost("{encodedVirtualPath}/start")]
        public async Task<ActionResult<string>> StartUpload(string encodedVirtualPath, [FromQuery] string path)
        {
            string userId = HttpContext.GetUserId();
            string virtualPath = PathHelper.DecodeAndValidatePath(encodedVirtualPath ?? path);
            Guid uuid = await bigFileService.StartUpload(userId, virtualPath);

            return uuid.ToString();
        }


        [HttpPost("{uuid}/append")]
        public async Task<ActionResult> AppendUpload(Guid uuid, [FromForm] AppendBigFileBody form)
        {
            if (form.PartialFile == null) return BadRequest("No data or file");

            string userId = HttpContext.GetUserId();
            await bigFileService.AppendUpload(userId, uuid, form.PartialFile);

            return Ok();
        }


        [HttpPut("{uuid}/finish")]
        public async Task<ActionResult> FinishUpload(Guid uuid)
        {
            string userId = HttpContext.GetUserId();
            await bigFileService.FinishUpload(userId, uuid);

            return Ok();
        }


        [HttpDelete("{uuid}")]
        public async Task<ActionResult> CancelUpload(Guid uuid)
        {
            string userId = HttpContext.GetUserId();
            await bigFileService.CancelUpload(userId, uuid);

            return Ok();
        }
    }
}
