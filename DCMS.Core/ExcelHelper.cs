using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Reflection;
using System.Text;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace DCMS.Core
{
    public class ExcelHelper
    {

        private static IWorkbook workbook = null;

        private static FileStream fs = null;

        private bool disposed;

        //public ExcelHelper(string fileName) 
        //{
        //    this.fileName = fileName;
        //    this.disposed = false;
        //}

        /// <summary>
        /// 将DataTable数据导入到excel中
        /// </summary>
        /// <param name="data">要导入的数据</param>
        /// <param name="isColumnWritten">DataTable的列名是否要导入</param>
        /// <param name="sheetName">要导入的excel的sheet的名称</param>
        /// <returns>导入数据行数(包含列名那一行)</returns>
        public static int DataTableToExcel(DataTable data, string sheetName, bool isColumnWritten,string fileName)
        {
            int i = 0;
            int j = 0;
            int count = 0;
            ISheet sheet = null;
            fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            try
            {
                workbook = new XSSFWorkbook(); // 2007版本
            }
#pragma warning disable CS0168 // 声明了变量“ex”，但从未使用过
            catch (Exception ex)
#pragma warning restore CS0168 // 声明了变量“ex”，但从未使用过
            {
                workbook = new HSSFWorkbook(); // 2003版本
            }
            try
            {
                if (workbook != null) sheet = workbook.CreateSheet(sheetName);
                else return -1;
                if (isColumnWritten == true) //写入DataTable的列名
                {
                    IRow row = sheet.CreateRow(0);
                    for (j = 0; j < data.Columns.Count; ++j)
                    {
                        row.CreateCell(j).SetCellValue(data.Columns[j].ColumnName);
                    }
                    count = 1;
                }
                else
                    count = 0;
                for (i = 0; i < data.Rows.Count; ++i)
                {
                    IRow row = sheet.CreateRow(count);
                    for (j = 0; j < data.Columns.Count; ++j)
                    {
                        row.CreateCell(j).SetCellValue(data.Rows[i][j].ToString());
                    }
                    ++count;
                }
                workbook.Write(fs); //写入到excel
                return count;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
                return -1;
            }
        }
        /// <summary>
        /// 将excel中的数据导入到DataTable中
        /// </summary>
        /// <param name="sheetName">excel工作薄sheet的名称</param>
        /// <param name="isFirstRowColumn">第一行是否是DataTable的列名</param>
        /// <returns>返回的DataTable</returns>
        public static DataTable ExcelToDataTable(string fileName, string sheetName = null, bool isFirstRowColumn =  true)
        {
            ISheet sheet = null;
            DataTable data = new DataTable("ExlImport");
            int startRow = 0;
            try
            {
                fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                try
                {
                    workbook = new XSSFWorkbook(fs); // 2007版本
                }
#pragma warning disable CS0168 // 声明了变量“ex”，但从未使用过
                catch (Exception ex)
#pragma warning restore CS0168 // 声明了变量“ex”，但从未使用过
                {
                    workbook = new HSSFWorkbook(fs); // 2003版本
                }
                if (sheetName != null)
                {
                    sheet = workbook.GetSheet(sheetName);
                    if (sheet == null) //如果没有找到指定的sheetName对应的sheet，则尝试获取第一个sheet
                        sheet = workbook.GetSheetAt(0);
                }
                else
                    sheet = workbook.GetSheetAt(0);
                if (sheet != null)
                {
                    IRow firstRow = sheet.GetRow(0);
                    int cellCount = firstRow.LastCellNum; //一行最后一个cell的编号 即总的列数
                    if (isFirstRowColumn)
                    {
                        for (int i = firstRow.FirstCellNum; i < cellCount; ++i)
                        {
                            ICell cell = firstRow.GetCell(i);
                            if (cell != null)
                            {
                                string cellValue = cell.StringCellValue;
                                if (cellValue != null)
                                {
                                    DataColumn column = new DataColumn(cellValue);
                                    data.Columns.Add(column);
                                }
                            }
                        }
                        startRow = sheet.FirstRowNum + 1;
                    }
                    else
                        startRow = sheet.FirstRowNum;
                    //最后一列的标号
                    int rowCount = sheet.LastRowNum;
                    for (int i = startRow; i <= rowCount; ++i)
                    {
                        IRow row = sheet.GetRow(i);
                        if (row == null) continue; //没有数据的行默认是null　　　　　　　

                        DataRow dataRow = data.NewRow();
                        for (int j = row.FirstCellNum; j < cellCount; ++j)
                        {
                            if (row.GetCell(j) != null) //同理，没有数据的单元格都默认是null
                                dataRow[j] = row.GetCell(j).ToString().TrimEnd();
                        }
                        data.Rows.Add(dataRow);
                    }
                }
                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
                return null;
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (fs != null)
                        fs.Close();
                }
                fs = null;
                disposed = true;
            }
        }
        /// <summary>
        /// 导出数据到Excel文件
        /// </summary>
        /// <param name="table"></param>
        /// <param name="headFiledName"></param>
        /// <param name="columnsWidth"></param>
        /// <param name="tempDirectory"></param>
        /// <returns>导出文件存储位置</returns>
        public static string ExpertExcel(DataTable table, string[] headFiledName, int columnsWidth, string tempDirectory)
        {
            if (table.Columns.Count != headFiledName.Length)
                throw new Exception("导出Excel错误：数据表列与表头不一致");
            if (Directory.Exists(tempDirectory) == false)
                Directory.CreateDirectory(tempDirectory);
            string fileFullPath = tempDirectory + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".xlsx";
            //新建一个excel
            IWorkbook workbook = new XSSFWorkbook();
            //excel样式
            XSSFCellStyle style = (XSSFCellStyle)workbook.CreateCellStyle();
            //创建一个sheet
            ISheet sheet = workbook.CreateSheet("Sheet");
            //给指定sheet的内容设置每列宽度（index从0开始，width1000相当于excel设置的列宽3.81）
            ICellStyle cellStyle = workbook.CreateCellStyle();
            cellStyle.Alignment = HorizontalAlignment.Center;
            for (int i = 0; i < headFiledName.Length; i++)
            {
                sheet.SetColumnWidth(i, 256 * columnsWidth);
            }
            Dictionary<int, string> columnDataType = new Dictionary<int, string>();
            int colIndex = 0;
            foreach (DataColumn col in table.Columns)
            {
                columnDataType.Add(colIndex, col.DataType.FullName);
                colIndex = colIndex + 1;
            }
            //在sheet里创建行
            IRow row1 = sheet.CreateRow(0);
            //第一行，列名
            for (var i = 0; i < headFiledName.Length; i++)
            {
                ICell cell = row1.CreateCell(i);
                cell.CellStyle = cellStyle;
                cell.SetCellValue(headFiledName[i]);
            }
            for (var r = 0; r < table.Rows.Count; r++)
            {
                var row_r = sheet.CreateRow(r + 1);
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    ICell cell = row_r.CreateCell(i);
                    cell.CellStyle = cellStyle;
                    if (table.Rows[r][i] != null)
                        SetCellValue(columnDataType[i], cell, table.Rows[r][i].ToString());
                }
            }

            MemoryStream ms = new MemoryStream();
            workbook.Write(ms);

            using (FileStream fs = new FileStream(fileFullPath, FileMode.Create, FileAccess.Write))
            {
                byte[] data = ms.ToArray();
                fs.Write(data, 0, data.Length);
                fs.Flush();
            }
            ms.Close();
            ms.Dispose();
            return fileFullPath;
        }
        /// <summary>
        /// 导出数据到Excel文件
        /// </summary>
        /// <param name="table"></param>
        /// <param name="headFiledName"></param>
        /// <param name="columnsWidth"></param>
        /// <param name="tempDirectory"></param>
        /// <param name="fileName"></param>
        /// <returns>导出文件存储位置</returns>
        public static byte[] ExpertExcel<T>(IList<T> lst, string[] headFiledName, int columnsWidth=0)
        {
            try
            {
                //新建一个excel
                IWorkbook workbook = new XSSFWorkbook();
                //excel样式
                XSSFCellStyle style = (XSSFCellStyle)workbook.CreateCellStyle();
                //创建一个sheet
                ISheet sheet = workbook.CreateSheet("Sheet");
                //给指定sheet的内容设置每列宽度（index从0开始，width1000相当于excel设置的列宽3.81）
                ICellStyle cellStyle = workbook.CreateCellStyle();
                cellStyle.Alignment = HorizontalAlignment.Center;
                for (int i = 0; i < headFiledName.Length; i++)
                {
                    sheet.SetColumnWidth(i, 256 * columnsWidth);
                }
                //在sheet里创建行
                IRow row1 = sheet.CreateRow(0);
                //获取列表的列
                Type type = typeof(T);
                var fields = type.GetProperties();
                //如果headFiledName为空，则使用对象添加了DisplayName的属性作为导出列
                if (headFiledName == null || headFiledName.Length == 0) 
                {
                    var head_lst = new List<string>();
                    foreach (var field in fields) 
                    {
                        var displayNameAttribute = field.GetCustomAttribute<DisplayNameAttribute>();
                        if (displayNameAttribute == null)
                            continue;
                        var displayName = displayNameAttribute.DisplayName;
                        head_lst.Add(displayName);
                    }
                    headFiledName = head_lst.ToArray();
                }
                //第一行，列名
                for (var i = 0; i < headFiledName.Length; i++)
                {
                    ICell cell = row1.CreateCell(i);
                    var font = workbook.CreateFont();
                    font.Boldweight = (short)FontBoldWeight.Bold;
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue(headFiledName[i]);
                }
                var r = 0;
                foreach (var item in lst)
                {
                    var row_r = sheet.CreateRow(r + 1);
                    var c = 0;
                    foreach (var field in fields)
                    {
                        var displayNameAttribute = field.GetCustomAttribute<DisplayNameAttribute>();
                        if (displayNameAttribute == null)
                            continue;
                        var displayName = displayNameAttribute.DisplayName;
                        if (((IList<string>)headFiledName).Contains(displayName))
                        {
                            var value = field.GetValue(item);
                            ICell cell = row_r.CreateCell(c);
                            cell.CellStyle = cellStyle;
                            SetCellValue(field.GetType().FullName, cell, value.ToString());
                            c++;
                        }
                    }
                    r++;
                }
                MemoryStream ms = new MemoryStream();
                workbook.Write(ms);
                return ms.ToArray();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// 设置单元格值
        /// </summary>
        /// <param name="cellValueType">数据类型</param>
        /// <param name="cell">单元格</param>
        /// <param name="data">数据</param>
        public static void SetCellValue(string cellValueType, ICell cell, string data)
        {
            if (string.IsNullOrEmpty(data) == true)
                return;
            switch (cellValueType)
            {
                case "System.Decimal":
                    cell.SetCellValue(double.Parse(data.ToString()));
                    break;
                case "System.Double":
                    cell.SetCellValue(double.Parse(data.ToString()));
                    break;
                case "System.Int32":
                    cell.SetCellValue(int.Parse(data.ToString()));
                    break;
                default:
                    cell.SetCellValue(data.ToString());
                    break;
            }
        }

        public static List<T> TableToList<T>(DataTable dtTable) where T : new()
        {
            try
            {
                List<T> list = new List<T>();
                Type type = typeof(T);
                foreach (DataRow row in dtTable.Rows)
                {
                    T t = new T();
                    // 获得此模型的公共属性      
                    PropertyInfo[] fields = type.GetProperties();
                    foreach (PropertyInfo field in fields)
                    {
                        var displayNameAttribute = field.GetCustomAttribute<DisplayNameAttribute>();
                        if (displayNameAttribute == null)
                            continue;
                        var displayName = displayNameAttribute.DisplayName;
                        // 检查DataTable是否包含此列  
                        if (dtTable.Columns.Contains(displayName))
                        {
                            // 判断此属性是否有Setter      
                            if (!field.CanWrite)
                                continue;
                            object value = row[displayName];
                            if (value != DBNull.Value)
                            {
                                //获取此字段的类型
                                Type typePi = field.PropertyType;
                                TypeCode typeCodePi = new TypeCode();
                                //判断此字段是否为nullable泛型类
                                //如果是就利用NullableConverter类获取其基本类型
                                //如果不是就直接获取其基本类型
                                if (typePi.IsGenericType && typePi.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
                                {
                                    NullableConverter nullableConverter = new NullableConverter(typePi);
                                    Type type1 = nullableConverter.UnderlyingType;
                                    typeCodePi = Type.GetTypeCode(type1);
                                }
                                else
                                {
                                    typeCodePi = Type.GetTypeCode(typePi);
                                }
                                //转换数据类型
                                switch (typeCodePi)
                                {
                                    case TypeCode.DateTime:
                                        DateTime.TryParse(value.ToString(), out DateTime dateValue);
                                        field.SetValue(t, dateValue, null);
                                        break;
                                    case TypeCode.Double:
                                        double.TryParse(value.ToString(), out double doubleValue);
                                        field.SetValue(t, doubleValue, null);
                                        break;
                                    case TypeCode.Empty:
                                        field.SetValue(t, "", null);
                                        break;
                                    case TypeCode.Int32:
                                        int.TryParse(value.ToString(), out int intValue);
                                        field.SetValue(t, intValue, null);
                                        break;
                                    case TypeCode.Object:
                                        field.SetValue(t, value, null);
                                        break;
                                    case TypeCode.String:
                                        field.SetValue(t, Convert.ToString(value), null);
                                        break;
                                    case TypeCode.Boolean:
                                        field.SetValue(t, true, null);
                                        break;
                                    case TypeCode.Decimal:
                                        decimal.TryParse(value.ToString(), out decimal decimalValue);
                                        field.SetValue(t, decimalValue, null);
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                    list.Add(t);
                }
                return list;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
