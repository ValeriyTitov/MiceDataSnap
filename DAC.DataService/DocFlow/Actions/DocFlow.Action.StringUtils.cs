using System;
using System.Collections.Generic;
using System.Data;

namespace DAC.DataService.DocFlow.Actions
{
    class TDocFlowStringUtils
    {
        public static string FormatValue(object Value, Type DataType)
        {
            if (DataType == typeof(DateTime) && Value != DBNull.Value)
                return (String.Format("{0:yyyy-MM-dd}", Value));
            //return (String.Format("{0:yyyy-MM-dd}T00:00:00.000Z", Value));
            else
                return (Value.ToString());
        }
        public static string ReplaceVariablesInText(string Text, DataRow Row)
        {
            string Result, Pattern, Value;
            Result = Text;
            for (int x = 0; x < Row.Table.Columns.Count; x++)
            {
                Pattern = "<" + Row.Table.Columns[x].ColumnName + ">";
                Value = FormatValue(Row[x], Row.Table.Columns[x].DataType);

                Result = Result.Replace(Pattern, Value);
            }
            return Result;
        }
        public static string ReplaceVariablesInText(string Text, Dictionary<string, string> Params)
        {
            string Result, Pattern, Value;
            Result = Text;
            foreach (var s in Params.Keys)
            {
                Pattern = "<" + s + ">";
                Value = Params[s].ToString();

                Result = Result.Replace(Pattern, Value);
            }
            return Result;
        }
    }
}
