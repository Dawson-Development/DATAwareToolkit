using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPOI.XSSF;
using NPOI.HSSF;
using NPOI.SS.UserModel;

namespace DATAware_Tools.Classes
{
    public class ConvertTool
    {
        ConvertOptions _Options { get; set; }
        public String[] HeaderRecord { get; set; }
        public FileWriter Writer { get; private set; }
        public int CurrentRecordIndex { get; set; }

        public ConvertTool(ConvertOptions options)
        {
            _Options = options;
        }

        public async Task ConvertExcel(IWorkbook workbook)
        {





        }

    }
}
