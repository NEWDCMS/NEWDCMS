using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace DCMS.Services.Global.Common
{
    //导出公共类
    public class ExportService : IExportService
    {
        public ExportService()
        {

        }

        //根据List导出数据
        public MemoryStream ExportListToExcel<T>(string ExportTableName, IList<KeyValuePair<string, string>> headerNameList, IList<T> list)
        {
            if (list.Count == 0)
            {
                return null;
            }

            HSSFWorkbook book = new HSSFWorkbook();
            ISheet sheet = book.CreateSheet(ExportTableName);
            IRow row = sheet.CreateRow(0);

            //设置表格样式
            ICellStyle cellStyle = book.CreateCellStyle();
            cellStyle.BorderBottom = BorderStyle.Thin;
            cellStyle.BorderLeft = BorderStyle.Thin;
            cellStyle.BorderTop = BorderStyle.Thin;
            cellStyle.BorderRight = BorderStyle.Thin;
            cellStyle.VerticalAlignment = VerticalAlignment.Center;
            cellStyle.Alignment = HorizontalAlignment.Center;

            //创建表头
            for (int i = 0; i < headerNameList.Count; i++)
            {
                ICell cell = row.CreateCell(i);
                cell.SetCellValue(headerNameList[i].Value);
                cell.CellStyle = cellStyle;
            }

            //数据填充
            Type t = typeof(T);
            int rowIndex = 1;
            foreach (T item in list)
            {
                IRow dataRow = sheet.CreateRow(rowIndex);
                for (int n = 0; n < headerNameList.Count; n++)
                {
                    object pValue = t.GetProperty(headerNameList[n].Key).GetValue(item, null);
                    dataRow.CreateCell(n).SetCellValue((pValue ?? "").ToString());
                }
                rowIndex++;
            }

            //写入客户端
            MemoryStream ms = new MemoryStream();
            book.Write(ms);
            ms.Seek(0, SeekOrigin.Begin);

            return ms;
        }

        public MemoryStream ExportTableToExcel(string ExportTableName, DataTable data)
        {
            if (data.Rows.Count <= 0)
            {
                return null;
            }

            HSSFWorkbook book = new HSSFWorkbook();
            ISheet sheet = book.CreateSheet(ExportTableName);
            IRow row = sheet.CreateRow(0);

            //设置表格样式
            ICellStyle cellStyle = book.CreateCellStyle();
            cellStyle.BorderBottom = BorderStyle.Thin;
            cellStyle.BorderLeft = BorderStyle.Thin;
            cellStyle.BorderTop = BorderStyle.Thin;
            cellStyle.BorderRight = BorderStyle.Thin;
            cellStyle.VerticalAlignment = VerticalAlignment.Center;
            cellStyle.Alignment = HorizontalAlignment.Center;


            // 创建表头
            foreach (DataColumn column in data.Columns)
            {
                ICell headerCell = row.CreateCell(column.Ordinal);
                headerCell.SetCellValue(column.ColumnName);
                headerCell.CellStyle = cellStyle;
            }

            // 填充数据
            int rowIndex = 1;

            foreach (DataRow rowmodel in data.Rows)
            {
                IRow dataRow = sheet.CreateRow(rowIndex);

                foreach (DataColumn column in data.Columns)
                {
                    dataRow.CreateCell(column.Ordinal).SetCellValue((rowmodel[column] ?? "").ToString());
                }

                rowIndex++;
            }

            //写入客户端
            MemoryStream ms = new MemoryStream();
            book.Write(ms);
            ms.Seek(0, SeekOrigin.Begin);

            return ms;
        }

        public MemoryStream ExportDataSetToExcel(string ExportTableName, DataSet data)
        {
            HSSFWorkbook book = new HSSFWorkbook();
            ISheet sheet = book.CreateSheet();
            IRow row = sheet.CreateRow(0);

            //设置表格样式
            ICellStyle cellStyle = book.CreateCellStyle();
            cellStyle.BorderBottom = BorderStyle.Thin;
            cellStyle.BorderLeft = BorderStyle.Thin;
            cellStyle.BorderTop = BorderStyle.Thin;
            cellStyle.BorderRight = BorderStyle.Thin;
            cellStyle.VerticalAlignment = VerticalAlignment.Center;
            cellStyle.Alignment = HorizontalAlignment.Center;

            for (int i = 0; i < data.Tables.Count; i++)
            {
                DataTable table = data.Tables[i];
                string sheetName = ExportTableName + i.ToString();
                ISheet sheetNew = book.CreateSheet(sheetName);
                IRow headerRow = sheet.CreateRow(0);

                // 创建表头
                foreach (DataColumn column in table.Columns)
                {
                    ICell cell = headerRow.CreateCell(column.Ordinal);
                    cell.SetCellValue(column.ColumnName);
                    cell.CellStyle = cellStyle;
                }

                // 填充数据
                int rowIndex = 1;

                foreach (DataRow rowModel in table.Rows)
                {
                    IRow dataRow = sheet.CreateRow(rowIndex);

                    foreach (DataColumn column in table.Columns)
                    {
                        dataRow.CreateCell(column.Ordinal).SetCellValue((rowModel[column] ?? "").ToString());
                    }

                    rowIndex++;
                }
            }

            //写入客户端
            MemoryStream ms = new MemoryStream();
            book.Write(ms);
            ms.Seek(0, SeekOrigin.Begin);

            return ms;
        }
    }
}
