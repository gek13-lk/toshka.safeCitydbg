using Microsoft.AspNetCore.Mvc;
using Toshka.dbgSave.DataAccess;

namespace Toshka.SafeCity.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FullnessForecastController : ControllerBase
    {
        private readonly EfContext _context;
        public FullnessForecastController(EfContext context)
        {
            _context = context;
        }
    }
}
