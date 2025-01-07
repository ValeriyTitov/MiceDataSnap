using System.Collections.Generic;
using System.Data;

namespace DAC.ObjectModels
{
    public class TMiceUserList : Dictionary<string, TMiceUser>
    {
        private object LockObject;
        private void CreateTestUsers()
        {

            var User = new TMiceUser();
            User.FullName = "Админов Админ Админович";
            User.LoginName = "1";
            User.PasswordHash = "1";
            User.UserId = 1;
            User.RoleList.Add("sa");
            User.RoleList.Add("admin");
            this.Add(User.LoginName, User);

            User = new TMiceUser();
            User.FullName = "Иванов Иван ИванЫч";
            User.LoginName = "2";
            User.PasswordHash = "2";
            User.UserId = 2;
            User.RoleList.Add("User ");
            this.Add(User.LoginName, User);
        }
        public void LoadFromDataSet(DataTable ATable)
        {
            foreach (DataRow row in ATable.Rows)
            {
                var AUser = new TMiceUser();
                AUser.LoadFromDataRow(row);
                if (ContainsKey(AUser.LoginName)==false)
                 Add(AUser.LoginName, AUser);
            }
        }
        public TMiceUser FindUser(string UserName)
        {
            if (ContainsKey(UserName) == true)
                return this[UserName];
            else
            lock(LockObject)
             {
                if (ContainsKey(UserName) == true)
                return this[UserName];
                 else
                {
                  Populate(UserName);
                  if (ContainsKey(UserName) == true)
                   return this[UserName];
                    else
                   return null;
                  }
             }
                
        }
        public void Populate(string UserName = "")
        {
            if (Count==0)
             CreateTestUsers();
            return;
            /*
            var DataSet = new TxDataSet();
            DataSet.ProviderName = "";
            if (!String.IsNullOrEmpty(UserName))
                DataSet.SetParameter("UserName", UserName);
            DataSet.Open();
            this.LoadFromDataSet(DataSet);
            */
        }
        public TMiceUserList()
        {
            LockObject = new object();
        }

        public static TMiceUserList DefaultInstance = new TMiceUserList();
    }
}
