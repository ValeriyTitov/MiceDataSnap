using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;


namespace DAC.ObjectModels
{
    public class TDataSetRow
    {
        public string KeyField { get; set; }
        public string KeyFieldValue { get; set; }
        public string UpdateRequest { get; set; }
        public int KeyFieldValueInt()
        {
            int IntValue;
            if (int.TryParse(KeyFieldValue, out IntValue) == true)
                return IntValue;
            else
                throw new System.Exception("Unable to convert key field value from {KeyFieldValue} to int");

        }
        public Dictionary<string, string> Values { get; set; }
        public TDataSetRow()
        {
            Values = new Dictionary<string, string>();
        }
    }

    public class TMiceApllyContent
    {
        private void InternalDeleteRowsList()
        {
            for (int i = DeletedRows.Count-1; i >= 0; i--)
            {
                DeletedRows[i].Delete();
            }
        }

        private Dictionary<TDataSetRow, DataRow> InsertedRows;

        private List<DataRow> DeletedRows;

        private string InternalToJson()
        {
            var jOwner = new JArray();
            foreach (var row in this.InsertedRows.Keys)
            {
                var NewValue = this.InsertedRows[row][row.KeyField].ToString();

                if (NewValue != String.Empty)
                {
                    var jObj = new JObject();
                    jObj.Add("OldID", row.KeyFieldValue);
                    jObj.Add("NewID", NewValue);
                    jOwner.Add(jObj);
                }
            }
            return jOwner.ToString();
        }

        private void UpdateRow(DataTable Table, TDataSetRow Row)
        {
            int AKeyFieldValueValue = Row.KeyFieldValueInt();

            var ARow = Table.AsEnumerable().Single(r => r.Field<int>(Row.KeyField) == Row.KeyFieldValueInt());
            foreach (var FieldName in Row.Values.Keys)
            {
                if (string.IsNullOrEmpty(Row.Values[FieldName]))
                    ARow[FieldName] = DBNull.Value;
                else
                {
                    if (Table.Columns[FieldName].DataType == typeof(Byte[]))
                        ARow[FieldName] = Convert.FromBase64String(Row.Values[FieldName]);
                    else
                        ARow[FieldName] = Row.Values[FieldName];
                }
            }
        }

        private void DeleteRow(DataTable Table, TDataSetRow Row)
        {
            int AKeyFieldValueValue = Row.KeyFieldValueInt();

            foreach (DataRow CurrentRow in Table.Rows)
            {
                if (CurrentRow.Field<int>(Row.KeyField) == AKeyFieldValueValue)
                    this.DeletedRows.Add(CurrentRow);
            }
        }

        public void InsertRow(DataTable Table, TDataSetRow Row)
        {
            var ARow = Table.NewRow();
            foreach (var FieldName in Row.Values.Keys)
            {
                if (Row.Values[FieldName] == string.Empty)
                    ARow[FieldName] = DBNull.Value;
                else
                {
                    if (Table.Columns[FieldName].DataType == typeof(Byte[]))
                    {
                        var RowValue = Row.Values[FieldName];
                        if (String.IsNullOrEmpty(RowValue)==false)
                         ARow[FieldName] = Convert.FromBase64String(RowValue);
                    }
                    else
                        ARow[FieldName] = Row.Values[FieldName];
                }
            }
            Table.Rows.Add(ARow);
            InsertedRows.Add(Row, ARow);
        }

        public List<TDataSetRow> Rows { get; set; }

        public TMiceApllyContent()
        {
            Rows = new List<TDataSetRow>();
            DeletedRows = new List<DataRow>();
            InsertedRows = new Dictionary<TDataSetRow, DataRow>();

        }

        public void ApplyToDataTable(DataTable Table)
        {
            DeletedRows.Clear();
            InsertedRows.Clear();

            foreach (var CurrentRow in Rows)
            {
                if (CurrentRow.UpdateRequest.Equals("Update"))
                    UpdateRow(Table, CurrentRow);
                else
                 if (CurrentRow.UpdateRequest.Equals("Insert"))
                    InsertRow(Table, CurrentRow);
                else
                 if (CurrentRow.UpdateRequest.Equals("Delete"))
                    DeleteRow(Table, CurrentRow);
            }

            InternalDeleteRowsList();
        }

        public string ToJson()
        {
            if (this.InsertedRows != null && this.InsertedRows.Count>0)
            {
                return this.InternalToJson();
            }
            else
                return "[]";

        }
    }


}