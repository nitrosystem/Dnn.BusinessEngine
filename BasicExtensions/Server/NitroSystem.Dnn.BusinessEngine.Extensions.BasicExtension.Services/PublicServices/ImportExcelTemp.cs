using ExcelDataReader;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtension.Services.PublicServices
{
    public static class ImportExcelTemp
    {
        public static JArray PopulateExcelData(string excelFilePath, List<string> excelFieldList)
        {
            var result = new JArray();

            using (var stream = File.Open(excelFilePath, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    int i = 0;
                    while (reader.Read())
                    {
                        if (i++ == 0) continue;

                        var rowItem = new JObject();

                        for (int columnIndex = 0; columnIndex < excelFieldList.Count; columnIndex++)
                        {
                            var itemName = excelFieldList[columnIndex];
                            object ItemValue = (reader.GetValue(columnIndex) != null ? reader.GetValue(columnIndex) : string.Empty);

                            rowItem.Add(new JProperty(itemName, ItemValue));
                        }

                        result.Add(rowItem);
                    }
                }
            }

            return result;
        }
    }
}
