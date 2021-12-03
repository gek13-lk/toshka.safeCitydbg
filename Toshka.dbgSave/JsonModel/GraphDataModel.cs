using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Toshka.dbgSave.JsonModel
{
    public class GraphDataModel
    {
        public List<DateTime> dateGraph { get; set; }

        public List<int> dataFullness { get; set; }
        public List<int> dataExport { get; set; }
    }
}
