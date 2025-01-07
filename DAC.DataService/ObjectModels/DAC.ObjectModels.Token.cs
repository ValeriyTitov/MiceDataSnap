using Newtonsoft.Json.Linq;
using System;


namespace DAC.ObjectModels
{
    public class TMiceToken
    {
        public const int DefaultExpirationMinutes = 030;
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        //public DateTime ValidThru { get; private set; }
        //SessionCreated.AddMinutes(ExpirationMinutes);
        public int ExpireInMinutes { get; set; }
        public DateTime CreatedOn { get; private set; }
        public TMiceToken()
        {
            CreatedOn = DateTime.Now;
            ExpireInMinutes = DefaultExpirationMinutes;
        }
        public bool Expired()
        {
            var Diff = DateTime.Now.Subtract(CreatedOn).TotalMinutes;
            return Diff > ExpireInMinutes;
        }
        public void ToJObject(JObject jObj)
        {
            jObj.Add("Token", Token);
            jObj.Add("RefreshToken", RefreshToken);
            jObj.Add("ExpireInMinutes", ExpireInMinutes);
        }
        public string ToJsonString()
        {
            var jObj = new JObject();
            this.ToJObject(jObj);
            return jObj.ToString();

        }

    }
}
