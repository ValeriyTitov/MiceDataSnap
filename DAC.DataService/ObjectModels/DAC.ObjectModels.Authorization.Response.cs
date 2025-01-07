using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using DAC.XDataSet;


namespace DAC.ObjectModels
{


    public class TMiceAuthorizationResponse
    {
        public string Message { get; set; }
        public TMiceToken Token { get; set; }
        public List<string> DBNameList {get;set;}
        public TMiceUser MiceUser { get; set; }
        public bool PasswordChangeRequired { get; set; }
        public bool PasswordExpired { get; set; }

        public void ToJObject(JObject jObj)
        {
            jObj.Add("Message", Message);

            var jToken = new JObject();
            Token.ToJObject(jToken);
            jObj.Add("Token", jToken);

            var jUser = new JObject();
            MiceUser.ToJObject(jUser);
            jObj.Add("MiceUser", jUser);
            
            AddDBNamesList(jObj);
        }
        public string ToJsonString()
        {
            var jObj = new JObject();
            this.ToJObject(jObj);
            return jObj.ToString();
        }

        public TMiceAuthorizationResponse()
        {
            MiceUser = new TMiceUser();
            DBNameList = new List<string>();

            ConnectionManager.DefaultInstance.ConnectionsToList(DBNameList);
        }

        private void AddDBNamesList(JObject jObj)
        {
            JArray jDbNames = new JArray();
            foreach (string s in this.DBNameList)
            {
                jDbNames.Add(s);
            }
            jObj.Add("DBNameList", jDbNames);
        }

    }

  
}