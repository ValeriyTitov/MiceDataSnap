using System.Data;
using Newtonsoft.Json.Linq;
using DAC.XDataSet;
using System;

namespace DAC.ObjectModels
{

    public class TMiceDataResponse
    {

        public TMiceExecutionContext ExecutionContext { get; set; }

        public DataTable DataSet { get; set; }

        public string QueryToken { get; set; }

        public TMiceDataResponse()
        {
 
        }

        public void ToJObject(JObject jObj)
        {
            if (ExecutionContext!=null)
             jObj.Add("ExecutionContext", ExecutionContext.ToNewJObject());

            if (DataSet != null)
             TDataTableSerializer.ToJObject(jObj, DataSet);

            if (String.IsNullOrEmpty(QueryToken) == false)
                jObj.Add("QueryToken", QueryToken);
        }

        public override string ToString()
        {
            var jObj = new JObject();
            this.ToJObject(jObj);
            return jObj.ToString();
        }



    }
}
