using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Newtonsoft.Json.Linq;
using System.Text;

namespace DAC.XDataSet
{

    public class TXParams : Dictionary<string, object>
    {
        public int AsInteger(string ParamName)
        {
            Check(ParamName);
            return Convert.ToInt32(this[ParamName]);
        }

        public string AsString(string ParamName)
        {
            Check(ParamName);
            return this[ParamName].ToString();
        }

        public DateTime AsDateTime(string ParamName)
        {
            throw new Exception("TXParams.AsDateTime not implemented");
            //Check(ParamName);
            //return  DateTime.Fro this[ParamName].T
        }


        public void SetParameter(string Name, object Value)
        {
            object AValue = Value;
            if (Value==null || Value.ToString().ToLower().Equals("null") == true)
                AValue = DBNull.Value;
               

            var AName = TParamUtils.RemoveEcho(Name);
            if (this.ContainsKey(AName) == true)
                this[AName] = AValue;
            else
                this.Add(AName, AValue);
        }

        public void CopyFrom(TXParams Params)
        {
            if (Params!=null)
            foreach (var s in Params.Keys)
                SetParameter(s, Params[s]);
        }

        public void LoadFromDataRow(DataRow Row)
        {
            for (int x = 0; x < Row.Table.Columns.Count; x++)
            {
                SetParameter(Row.Table.Columns[x].ColumnName, Row[x]);
            }
        }

        public TXParams(): base(StringComparer.OrdinalIgnoreCase) 
        {

        }


        public void CloneToSqlParameterCollection(SqlParameterCollection Dest)
        {
            foreach (var s in this.Keys)
            {
                if (Dest.Contains(s))
                    Dest[s].Value = this[s];
                else
                    Dest.AddWithValue(s, this[s]);
            }
                
        }

        public void Check(string ParamName)
        {
            if (this.ContainsKey(ParamName) == false)
                throw new Exception("Cannot find parameter with name "+ParamName);
        }

        public static void InsertToSqlParameterCollection(TXParams Source, SqlParameterCollection Dest, DataRowCollection Rows)
        {
            foreach (DataRow Row in Rows)
            {
                var ParamName = TParamUtils.RemoveEcho(Row["PARAMETER_NAME"].ToString());

                if (Source.ContainsKey(ParamName) ==true)
                {
                    var ParamValue = Source[ParamName];

                    if (Dest.Contains(ParamName) == false)
                    {
                        if (Row["DATA_TYPE"].ToString().Equals("varbinary")==true)
                            Dest.Add(ParamName, SqlDbType.VarBinary).Value = ParamValue;
                        else
                            Dest.AddWithValue(ParamName, ParamValue);
                    }
                    else
                        Dest[ParamName].Value = ParamValue;
                }

            }
        }

        public void ToJsonObject(JObject jObj)
        {
          foreach (var s in Keys)
           jObj.Add(s, this[s].ToString());
        }

        public void ToJsonObjectParams(JObject jObj)
        {
            var jParams = new JObject();
            this.ToJsonObject(jParams);

            jObj.Add("Params", jParams);
            
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            
            foreach (var Key in this.Keys)
            {
                sb.Append(Key+this[Key].ToString());
            }

            return sb.ToString();
        }


    }



}
