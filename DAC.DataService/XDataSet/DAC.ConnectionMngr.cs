using System;
using System.Collections.Generic;

namespace DAC.XDataSet
{
    public class ConnectionManager
    {
        public static ConnectionManager DefaultInstance = new ConnectionManager();
        private ConnectionManager()
        {
            ConnectionStringList = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            FMetaConnectionString = "Server = localhost; DataBase = Meta; Integrated Security = SSPI;";

            ConnectionStringList.Add("", FMetaConnectionString);
            ConnectionStringList.Add("Meta", FMetaConnectionString);
            ConnectionStringList.Add("SeqDB", "Server = localhost; DataBase = SeqDB; Integrated Security = SSPI;");
            ConnectionStringList.Add("DocFlow", "Server = localhost; DataBase = DocFlow; Integrated Security = SSPI;");
            ConnectionStringList.Add("WebSite", "Server=localhost;DataBase=WebSite;Integrated Security=SSPI;");
            ConnectionStringList.Add("ReportDB", "Server=localhost;DataBase=ReportDB;Integrated Security=SSPI;");
        }

        private string FMetaConnectionString { get; set; }

        private string FGetConnectionString(string DBName)
        {
            if (String.IsNullOrEmpty(DBName))
                return FMetaConnectionString;
            if (ConnectionStringList.ContainsKey(DBName))
                return ConnectionStringList[DBName];
            else
                throw new Exception(String.Format("Alias '{0}' is not defined", DBName));
        }

        public Dictionary<string, string> ConnectionStringList{get;set;}

        public static string PipeConnectionString()
        {
          return DefaultInstance.FMetaConnectionString;
        }

        public void ConnectionsToList(List<string> AList)
        {
            foreach (string s in this.ConnectionStringList.Keys)
                AList.Add(s);
        }
        public static string GetConnectionString(string DBName)
        {
          return DefaultInstance.FGetConnectionString(DBName);
        }
    }

}