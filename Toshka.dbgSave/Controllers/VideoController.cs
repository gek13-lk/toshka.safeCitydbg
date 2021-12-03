using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace Toshka.dbgSave.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class VideoController : ControllerBase
    {
        [HttpGet]
        [Route("stream/{cameraId?}")]
        public IActionResult Video(int cameraId)
        {
            //add streaming sources
            string filePath;
            filePath = cameraId < 8 ? "video1.mp4" : "video2.mp4";

            if (TryOpenFile(System.IO.Path.Combine(Environment.CurrentDirectory, filePath), FileMode.Open, out FileStream fs))
            {
                FileStreamResult result = File(
                    fs,
                    new MediaTypeHeaderValue("video/mp4").MediaType.Value,
                    true

                );
                return result;
            }

            return BadRequest();
        }

        private static bool TryOpenFile(string file, FileMode open, out FileStream fileStream)
        {
            fileStream = System.IO.File.Open(file, open,
                FileAccess.Read, FileShare.Read);
            if (fileStream != null)
            {
                return true;
            }

            return false;
        }
    }
}