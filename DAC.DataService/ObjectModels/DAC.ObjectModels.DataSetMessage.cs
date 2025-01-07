using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace DAC.ObjectModels
{
    public class TMiceDataSetMessage
    {
        public string AMessage { get; set; }
        public int LineNumber { get; set; }
        public int Code { get; set; }
        //public string Source { get; set; };
        public JObject ToNewJObject()
        {
            var Result = new JObject();
            Result.Add("AMessage", AMessage);
            //  Result.Add("Source", Source);
            Result.Add("LineNumber", LineNumber);
            Result.Add("Code", Code);
            return Result;
        }
    }


    public class TMiceDatasetMessageList : List<TMiceDataSetMessage>
    {
        public void AddMessage(string MessageText)
        {
            Add(new TMiceDataSetMessage { AMessage = MessageText });
        }

        public void ToJsonObject(JObject jObject)
        {
            if (Count > 0)
            {
                var jMessages = new JArray();
                foreach (TMiceDataSetMessage Entry in this)
                {
                    jMessages.Add(Entry.ToNewJObject());
                }

                jObject.Add("Messages", jMessages);
            }

        }

    }
}