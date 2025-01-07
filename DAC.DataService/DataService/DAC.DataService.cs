using System;
using DAC.ObjectModels;
using DAC.XDataSet;
using DAC.Authorization;


namespace DAC.DataService

{
    public class TDataService
    {

        public static string MiceApplyUpdatesRequest(TMiceUser User, TMiceApplyUpdatesRequest updatesRequest)
        {
            var ds = new TxDataSet();
            ds.UseDataAdapterToRead = true;
            ds.ProviderName = updatesRequest.ExecutionContext.ProviderName;
            ds.DBName = updatesRequest.ExecutionContext.DBName;
            ds.Open();
            updatesRequest.ApplyContext.ApplyToDataTable(ds);

            if (ds.Columns.Count > 0)
                ds.KeyField = ds.Columns[0].ColumnName;
            ds.ApplyUpdates();

            return updatesRequest.ApplyContext.ToJson();
        }

        public static string AbortQuery(string Token, string QueryToken)
        {
            try
            {
                var User = TAuthorization.UserFromToken(Token);
                throw new Exception("Abort query not implemented.");
            }
            catch (Exception e)
            {
                var ex = TMiceException.CreateException(e);
                return ex.ToString();
            }
        }

        public static void ClearCache()
        {
            TSQLDataCache.ClearEntireCache();
            TScriptRunner.ClearCache();
        }

    }


   

}
