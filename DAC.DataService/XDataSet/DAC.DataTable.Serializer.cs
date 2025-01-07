using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DAC.XDataSet

{
    class TDataTableSerializer
    {

        protected static string DataTableToJson2(DataTable dataTable)
        {
            Dictionary<string, object> result = new Dictionary<string, object>
            {
                //K Johnson answered Sep 17 at 13:10 https://stackoverflow.com/questions/52367299/c-sharp-datatable-to-json-with-custom-format
                //Но медленно :)

                ["Columns"] = dataTable.Columns.Cast<DataColumn>().Select(x => new Dictionary<string, object> { [x.ColumnName] = x.DataType.Name }).ToArray(),
                ["Rows"] = dataTable.Rows.Cast<DataRow>().Select(x => x.ItemArray).ToArray()
            };
            var json = JsonConvert.SerializeObject(result);
            return json;
        }

        public static string DataTableToJson(DataTable dataTable)
        {
            JObject jDataTable = new JObject();
            ToJObject(jDataTable, dataTable);
            return jDataTable.ToString();
        }

        private static void WriteColumns(JObject jDataTable, DataTable dataTable)
        {
            var jColumns = new JArray();
            foreach (DataColumn column in dataTable.Columns)
            {
                var ColData = new JObject();
                string DataTypeName = column.DataType.Name;

                ColData.Add("Name", column.ColumnName);
                ColData.Add("DataType", DataTypeName);

                if (column.MaxLength > 0)
                    ColData.Add("Size", column.MaxLength);

                if (column.AutoIncrement == true)
                    ColData.Add("AutoInc", column.AutoIncrement);

                if (column.AllowDBNull == false)
                    ColData.Add("AllowDBNull", column.AllowDBNull);

                jColumns.Add(ColData);
            }

            jDataTable.Add("Columns", jColumns);
        }

        private static void WriteRows(JObject jDataTable, DataTable dataTable)
        {
            var jRows = new JArray();
            foreach (DataRow Row in dataTable.Rows)
            {
                var jRowData = new JArray();
                jRowData.Add(Row.ItemArray);
                jRows.Add(jRowData);
            }
            jDataTable.Add("Rows", jRows);
        }

        public static void ToJObject(JObject jObj, DataTable dataTable)
        {
          WriteColumns(jObj, dataTable);
          WriteRows(jObj, dataTable);
        }



    }
}
