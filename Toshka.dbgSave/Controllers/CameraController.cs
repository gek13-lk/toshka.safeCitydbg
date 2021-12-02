using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Toshka.dbgSave.DataAccess;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.TimeSeries;
using System.IO;
using Toshka.dbgSave.Model;
using System.Data;
using Toshka.dbgSave.Services.Abstraction;
using Telegram.Bot;

namespace Toshka.dbgSave.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CameraController : ControllerBase
    {
        private readonly IBotService _botService;
        private readonly EfContext _context;

        public CameraController(IBotService botService,
        EfContext context)
        {
            _botService = botService;
            _context = context;
        }

        [HttpGet]
        public JsonResult Index()
        {
            return new JsonResult(1);
        }
    }
}
