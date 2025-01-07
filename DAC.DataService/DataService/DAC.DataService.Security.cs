using System;
using System.Security.Cryptography;
using System.Text;
using DAC.ObjectModels;
using DAC.XDataSet;

namespace DAC.DataService
{
    public class TDataSecurity
    {

        public static bool IsValidDialogRequest(TMiceDataRequest miceRequest)
        {
            int AppDialogsID = Convert.ToInt32(miceRequest.ExecutionContext.ApplicationContext["AppDialogsID"]);
            int ID = Convert.ToInt32(miceRequest.ExecutionContext.ApplicationContext["ID"]);

            var DataSet = new TxDataSet();
            DataSet.ProviderName = "spui_AppDialogGetProperties";
            DataSet.SetParameter("appDialogsID", AppDialogsID);
            DataSet.Open();

            string TableName = DataSet.Rows[0]["TableName"].ToString();
            string KeyField = DataSet.Rows[0]["KeyField"].ToString();
            
            string s = String.Format("SELECT * FROM [{0}] WITH (NOLOCK) WHERE {1}={2}", TableName, KeyField, ID);
            return (miceRequest.ExecutionContext.ProviderName == s);
        }
        public static bool AllowedToExecute(TMiceDataRequest miceRequest, TMiceUser user)
        {
            var IsEmpty = (String.IsNullOrEmpty(miceRequest.ExecutionContext.ProviderName));
            var IsStoredProc = (IsEmpty == false && (TxDataSetHelper.ProviderIsStoredProc(TxDataSetHelper.FindProviderCommandName(miceRequest.ExecutionContext.ProviderName))));
            var UserIsAdmin = user.IsInRole("sa");
            var DialogRequest = miceRequest.ExecutionContext.ApplicationContext.ContainsKey("AppDialogsID") && miceRequest.ExecutionContext.ApplicationContext.ContainsKey("ID");
            var IsValidTableRequest = DialogRequest && (UserIsAdmin || IsValidDialogRequest(miceRequest));
            var IsAdminRequest = (miceRequest.ExecutionContext.ProviderName == "spui_AppMainTreeManager");

            var Result = ((IsStoredProc && IsAdminRequest==false) || (UserIsAdmin || IsValidTableRequest));
            
            return Result;
        }





        public static string Salt = "{9D227564-10AE-423F-AAFA-7D39F2729FF4}{1B739E81-B16A-4814-BED7-B58B3CB48EEF}{42A7C72E-A5E8-4A06-A788-9F3151729177}";
        public static string Hash(string Value)
        {
            StringBuilder Sb = new StringBuilder();

            using (SHA256 hash = SHA256Managed.Create())
            {
                Encoding enc = Encoding.UTF8;
                Byte[] result = hash.ComputeHash(enc.GetBytes(Value));

                foreach (Byte b in result)
                    Sb.Append(b.ToString("x2"));
            }

            return Sb.ToString();
        }
        public static string HashWithSalt(string Value)
        {
            return Hash(Value + Salt);
        }
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
    }
}
