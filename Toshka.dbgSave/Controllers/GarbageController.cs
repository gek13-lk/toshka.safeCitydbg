using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Toshka.dbgSave.DataAccess;

namespace Toshka.dbgSave.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GarbageController : ControllerBase
    {
        private readonly EfContext _context;
        public GarbageController(EfContext context)
        {
            _context = context;
        }

        [HttpGet]
        public JsonResult Index()
        {
            return new JsonResult(_context.Garbages.ToList());
        }

        [HttpGet]
        [Route("get/{id?}")]
        public JsonResult GetById(long id)
        {
            return new JsonResult(_context.Garbages.FirstOrDefault(x => x.Id == id));
        }
    }
}
