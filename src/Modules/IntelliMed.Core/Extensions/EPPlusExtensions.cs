using IntelliMed.Core.Helpers;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace OrganiMedCore.DiabetesCareCenterManager.Helpers
{
    public static class EPPlusExtensions
    {
        public static IEnumerable<T> ConvertSheetToObjects<T>(this Stream stream, int sheetIndex, int headerRows)
            where T : new()
        {
            using (var excel = new ExcelPackage(stream))
            {
                return Convert<T>(excel.Workbook.Worksheets[sheetIndex], headerRows);
            }
        }

        public static byte[] SimpleDataExport<T>(this IEnumerable<T> data, string title) where T : class
        {
            using (var stream = new MemoryStream())
            {
                using (var package = new ExcelPackage(stream))
                {
                    var sheet = package.Workbook.Worksheets.Add(title);
                    sheet.Cells["A1"].LoadFromCollection(Collection: data, PrintHeaders: true);
                    package.Save();
                }

                return stream.ToArray();
            }
        }


        private static IEnumerable<T> Convert<T>(ExcelWorksheet worksheet, int headerRows)
            where T : new()
        {
            var columns = typeof(T)
                .GetProperties()
                .Where(x => x.CustomAttributes.Any(y => y.AttributeType == typeof(ExcelColumnAttribute)))
                .Select(p => new
                {
                    Property = p,
                    Column = p.GetCustomAttributes<ExcelColumnAttribute>().First().ColumnIndex
                })
                .ToList();

            var rows = worksheet
                .Cells
                .Select(cell => cell.Start.Row)
                .Distinct()
                .OrderBy(x => x);


            return rows
                .Skip(headerRows)
                .Select(row =>
                {
                    var tNew = new T();
                    columns.ForEach(col =>
                    {
                        //This is the real wrinkle to using reflection. Excel stores all numbers as double including int
                        var val = worksheet.Cells[row, col.Column];

                        //If it is numeric it is a double since that is how excel stores all numbers
                        if (val.Value == null)
                        {
                            col.Property.SetValue(tNew, null);

                            return;
                        }

                        if (col.Property.PropertyType == typeof(int))
                        {
                            col.Property.SetValue(tNew, val.GetValue<int>());

                            return;
                        }

                        if (col.Property.PropertyType == typeof(double))
                        {
                            col.Property.SetValue(tNew, val.GetValue<double>());

                            return;
                        }

                        if (col.Property.PropertyType == typeof(DateTime))
                        {
                            col.Property.SetValue(tNew, val.GetValue<DateTime>());

                            return;
                        }

                        col.Property.SetValue(tNew, val.GetValue<string>());
                    });

                    return tNew;
                })
                .ToArray();
        }
    }
}
