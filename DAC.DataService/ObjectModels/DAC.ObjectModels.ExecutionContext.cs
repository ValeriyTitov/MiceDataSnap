using System;
using System.Collections.Generic;
using DAC.XDataSet;
using Newtonsoft.Json.Linq;

namespace DAC.ObjectModels
{


    public class TMiceExecutionContext
    {

        public string ProviderName { get; set; }

        public string DBName { get; set; }

        public string KeyField { get; set; }

        public int Status { get; set; }

        public int CacheDuraion { get; set; }

        public string CacheRegion { get; set; }

        public TXParams Params { get; set; }

        public Dictionary<string, string> ApplicationContext { get; private set; }

        public TMiceDatasetMessageList Messages { get; private set; }

        public JObject ToNewJObject()
        {
            var Result = new JObject();
            this.ToJsonObject(Result);
            return Result;
        }



        public void ToJsonObject(JObject jObject)
        {
            jObject.Add("ProviderName", ProviderName);
            jObject.Add("DBName", DBName);
            jObject.Add("Status", Status);

            // this.Messages.Add(new TMiceDataSetMessage { AMessage = "Hello world"});
            // this.Messages.Add(new TMiceDataSetMessage { AMessage = "This is sample messages" });
            // this.Messages.Add(new TMiceDataSetMessage { AMessage = "Constructed in DAC.XDataSet.ObjectModels.cs" });

            this.Messages.ToJsonObject(jObject);
                
            
            this.Params.ToJsonObjectParams(jObject);


        }

        public string CommandName()
        {
            return TxDataSetHelper.FindProviderCommandName(ProviderName).Trim();
        }

        public TMiceExecutionContext()
        {
            Params = new TXParams();
            Messages = new TMiceDatasetMessageList();
            ApplicationContext = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        }

        public string CalculateHash()
        {
            return DBName+ProviderName + Params.ToString();
        }
    }
}