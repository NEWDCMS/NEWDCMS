using System.Collections.Generic;
using System.Data;
using System.IO;

namespace DCMS.Services.Global.Common
{
    public interface IExportService
    {
        //根据list导出数据
        MemoryStream ExportListToExcel<T>(string ExportTableName, IList<KeyValuePair<string, string>> headerNameList, IList<T> list);

        //根据datatable导出数据
        MemoryStream ExportTableToExcel(string ExportTableName, DataTable data);

        MemoryStream ExportDataSetToExcel(string ExportTableName, DataSet data);
    }
}
