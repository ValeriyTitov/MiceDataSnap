using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAC.XDataSet
{

    class TxDataSetAppender
    {
        public List<DataColumn> ColumnsList { get; set; }
        public DataTable FromTable { get; set; }
        public DataTable ToTable { get; set; }

        public void FindColumnsByName()
        {
            foreach (DataColumn C in ToTable.Columns)
            {
                if (FromTable.Columns.Contains(C.ColumnName))
                    ColumnsList.Add(C);
            }

        }

        public void AppendByFieldName()
        {
            ColumnsList.Clear();
            FindColumnsByName();
            if (ColumnsList.Count > 0)
                CopyFieldValues();
        }
        public void CopyFieldValues()
        {
            foreach (DataRow Row in FromTable.Rows)
            {
                var NewRow = ToTable.NewRow();
                foreach (DataColumn C in ColumnsList)
                {
                    NewRow[C.ColumnName] = Row[C.ColumnName];
                }
                ToTable.Rows.Add(NewRow);
            }
        }

        public TxDataSetAppender()
        {
            ColumnsList = new List<DataColumn>();
        }

    }
    class TxDataSetHelper
    {
        public static SqlCommand CreateInsertCommand(SqlCommand BuilderCommand, SqlConnection FConnection, string KeyField)
        {
            var Result = new SqlCommand(BuilderCommand.CommandText + ";SELECT SCOPE_IDENTITY() as " + "[" + KeyField + "]");
            Result.Connection = FConnection;
            Result.UpdatedRowSource = UpdateRowSource.FirstReturnedRecord;

            foreach (object obj2 in BuilderCommand.Parameters)
            {
                Result.Parameters.Add((obj2 is ICloneable) ? (obj2 as ICloneable).Clone() : obj2);
            }
            return Result;
        }

        public static bool DataTableContainsAutoInc(DataTable Table)
        {
            foreach (DataColumn CurrentCol in Table.Columns)
            {
                if (CurrentCol.AutoIncrement == true)
                    return true;
            }
            return false;
        }

       
        public static CommandBehavior CommandBehaviorFromStringDef(string s, CommandBehavior DefaultValue)
        {
            int Behavior = 0;
            if (Int32.TryParse(s, out Behavior))
                switch (Behavior)
                {
                    case 0: return CommandBehavior.Default;
                    case 1: return CommandBehavior.SingleResult;
                    case 2: return  CommandBehavior.SchemaOnly; 
                    case 4: return  CommandBehavior.KeyInfo;
                    case 8: return  CommandBehavior.SingleRow;
                    case 16: return  CommandBehavior.SequentialAccess; 
                    case 32: return CommandBehavior.CloseConnection; ;
                }
            return DefaultValue;
        }

        public static int StrToIntDef(string AValue, int Default)
        {
            int Number;
            if (int.TryParse(AValue, out Number))
                return Number;
            else
                return Default;
        }

        
        public static bool ProviderIsStoredProc(string CommandName)
        {
            if (CommandName.IndexOf(" ") >= 1 || CommandName.IndexOf((char)160) >=1 || CommandName.IndexOf((char)9)>=1 || CommandName.IndexOf((char)13) >= 1)
             return false; 
            else
             return true; 
        }

        public static string FindProviderDBName(string ProviderName)
        {
            int AIndex;
            AIndex = ProviderName.IndexOf("].");
            if (AIndex >= 1)
                return ProviderName.Substring(1, AIndex - 1);
            else
                return "";
        }

        public static string FindProviderCommandName(string ProviderName)
        {
            return ProviderName.Trim();
            /*int AIndex, AIndex2;
            AIndex = ProviderName.IndexOf("].");
            AIndex2 = ProviderName.IndexOf(",");
            if ((AIndex > 0) && (AIndex2 > 0))
            {
                return ProviderName.Substring(AIndex + 2, (AIndex2 - AIndex - 2));
            }

            if ((AIndex > 0) && (AIndex2 <= 0))
            {
                return ProviderName.Substring(AIndex + 2, (ProviderName.Length - AIndex - 2));
            }
            else
            if ((AIndex <= 0) && (AIndex2 > 0))
            {
                return ProviderName.Substring(0, AIndex2);
            }
            return ProviderName;
            */

        }

        public static int FindProviderTimeOut(string ProviderName)
        {
            int AIndex;
            AIndex = ProviderName.IndexOf(",");
            if ((AIndex >= 1) && (AIndex + 1 <= ProviderName.Length))
            {
                return StrToIntDef(ProviderName.Substring(AIndex + 1, ProviderName.Length - AIndex - 1), 0);
            }
            else
                return 0;
        }

        public static string DBNameToStr(string Value)
        {
            if (String.IsNullOrEmpty(Value) == true)
                return "";
            else
                return "[" + Value + "].";
        }

        public static string CacheDurationToStr(int Value)
        {
            if (Value <= 0)
                return "";
            else
                return "," + Value.ToString();
        }

        public void AppendByFieldName()
        {
        
        
        }
    }
}
