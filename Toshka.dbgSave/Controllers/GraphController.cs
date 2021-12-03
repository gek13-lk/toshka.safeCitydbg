using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Toshka.dbgSave.DataAccess;
using Toshka.dbgSave.JsonModel;

namespace Toshka.dbgSave.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GraphController : ControllerBase
    {
        private readonly EfContext _context;
        public GraphController(EfContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("garbage-export")]
        public JsonResult Index()
        {
            var dataExportAndFullness = _context.ModelsInput.ToList();

            List<DateTime> dateGraph = new List<DateTime>();
            List<int> dataFullness = new List<int>();
            List<int> dataExport = new List<int>();
            int export;
            foreach (var data in dataExportAndFullness)
            {
                dateGraph.Add(data.RentalDate);
                dataFullness.Add((int)data.Fullness);

                export = 0;
                if (data.Export == 1)
                {
                    export = 100;
                }

                dataExport.Add(export);
            }

            GraphDataModel model = new GraphDataModel();
            model.dateGraph = dateGraph;
            model.dataFullness = dataFullness;
            model.dataExport = dataExport;
            //var dataExport = _context.ModelsInput.Select(x => x.Export == 1).ToList();
            //var dataFullness;
            return new JsonResult(model);
        }

    }
}
