using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Data;

namespace DAC.ObjectModels
{
    public class TMiceUser
    {
        public string FullName { get; set; }
        public string LoginName { get; set; }
        public string PasswordHash { get; set; }
        public int UserId { get; set; }
        public bool Active { get; set; }
        public string Salt { get; set; }

        public void LoadFromDataRow(DataRow row)
        {
            FullName = row["FullName"].ToString();
            UserId = (int)row["UserId"];
        }
        public List<string> RoleList { get; set; }
        public bool IsInRole(string Role)
        {
            return RoleList.IndexOf(Role) >= 0;
        }
        public TMiceUser()
        {
            RoleList = new List<string>();
            Active = true;
        }
        public void ToJObject(JObject jObj)
        {
            jObj.Add("FullName", FullName);
            jObj.Add("LoginName", LoginName);
            jObj.Add("UserId", UserId);

            var jRoles = new JArray();
            foreach (string s in this.RoleList)
            {
                jRoles.Add(s);
            }
            jObj.Add("RoleList", jRoles);
        }
        public string ToJsonString()
        {
            var jObj = new JObject();
            this.ToJObject(jObj);
            return jObj.ToString();

        }
    }


}
