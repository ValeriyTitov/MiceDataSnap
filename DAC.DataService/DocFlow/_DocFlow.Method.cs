using DAC.XDataSet;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAC.DataService.DocFlow
{
    class TDocFlowMethod
    {
        public string DBName { get; set; }
        public int dfMethodsId { get; private set; }
        public int dfClassesId { get; private set; }
        public int dfTypesId { get; private set; }
        public int dfPathFoldersIdSource { get; private set; }
        public int dfPathFoldersIdTarget { get; private set; }
        public string Caption { get; private set; }
        public string CodeName { get; private set; }
        public bool AllowDesktop { get; private set; }
        public bool AllowRollback { get; private set; }
        public bool IsDefault { get; private set; }
        public bool RunDaily { get; private set; }
        public bool FullAccess { get; private set; }
        public bool Active { get; private set; }
        public bool UseExpression { get; private set; }
        public string Expression { get; private set; }
        public string UserMessage { get; private set; }
        private void Map(DataRow Row)
        {
            dfPathFoldersIdSource = Convert.ToInt32(Row["dfPathFoldersIdSource"]);
            dfPathFoldersIdTarget = Convert.ToInt32(Row["dfPathFoldersIdTarget"]);
            dfMethodsId = Convert.ToInt32(Row["dfMethodsId"]);
            dfClassesId = Convert.ToInt32(Row["dfClassesId"]);
            dfTypesId = Convert.ToInt32(Row["dfTypesId"]);
            Caption = Row["Caption"].ToString();
            CodeName = Row["CodeName"].ToString();
            AllowDesktop = (bool)Row["AllowDesktop"];
            AllowRollback = (bool)Row["AllowRollback"];
            IsDefault = (bool)Row["IsDefault"];
            RunDaily = (bool)Row["RunDaily"];
            FullAccess = (bool)Row["FullAccess"];
            Active = (bool)Row["Active"];
            UseExpression = (bool)Row["UseExpression"];
            Expression = Row["Expression"].ToString();
            UserMessage= Row["UserMessage"].ToString();
        }
        public void FindMethodInfo(int dfMethodsIdTarget)
        {
            var Tmp = new TxDataSet();
            Tmp.ProviderName = "spui_dfMethodsInfo";
            Tmp.SetParameter("dfMethodsId", dfMethodsIdTarget);
            Tmp.DBName = this.DBName;
            Tmp.Open();
            if (Tmp.Rows.Count != 1)
                throw new Exception("Docflow Engine: Method not found");
            Map(Tmp.Rows[0]);
        }
    }
}
