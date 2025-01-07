using DAC.DataService.DocFlow.Actions;
using DAC.XDataSet;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAC.DataService.DocFlow
{
    class TDocFlowDocument : TDocFlowEntity
    {
        private int dfPathFoldersIdTarget { get; set; }
        private int FolderTypeTarget { get; set; }
        private int dfEventsIdTarget { get; set; }

        public TDocFlowDocument(TDocFlowEntity AParent) : base(AParent)
        { 
        
        }

        private bool ValidateTargetMethod(DataTable Table)
        {
            var Tmp = new TxDataSet();
            Tmp.DBName = this.DBName;
            Tmp.ProviderName = TDocFlowRuleValidator.CreateCheckSQL(Table, DataView, KeyField,DocumentsId);
            Tmp.Open();
            Result.AppendByFieldName(Tmp);
            return Tmp.Rows.Count == 0;

        }

        private bool ValidateIncomingRules()
        {
            var Rules = GetRuleList();
            if (Rules.Rows.Count > 0)
            {
                var Tmp = new TxDataSet();
                Tmp.ProviderName = TDocFlowRuleValidator.CreateCheckSQL(Rules, DataView, KeyField, DocumentsId);
                Tmp.DBName = this.DBName;
                Tmp.Open();
                Result.AppendByFieldName(Tmp);
                return (Tmp.Rows.Count == 0);
            }
            return true;
        }

        private TxDataSet FindMethodInfo(int dfMethodsIdTarget)
        {
            var Tmp = new TxDataSet();
            Tmp.ProviderName = "spui_dfMethodsInfo";
            Tmp.SetParameter("dfMethodsId", dfMethodsIdTarget);
            Tmp.DBName = this.DBName;
            Tmp.Open();
            if (Tmp.Rows.Count != 1)
                throw new Exception("Docflow Engine: Active method not found");
            return Tmp;
        }
        private bool FindTargetdfPathFoldersIdPush(int dfMethodsIdTarget)
        {
            var dfMethod = FindMethodInfo(dfMethodsIdTarget);
            dfPathFoldersIdTarget = Convert.ToInt32(dfMethod.Rows[0]["dfPathFoldersIdTarget"]);
            if (dfPathFoldersId != Convert.ToInt32(dfMethod.Rows[0]["dfPathFoldersIdSource"]))
                throw new Exception("Docflow Engine: Invalid route");

            FolderTypeTarget = Convert.ToInt32(dfMethod.Rows[0]["FolderType"]);
            if ((bool)dfMethod.Rows[0]["UseExpression"] == true)
                return ValidateTargetMethod(dfMethod);
            else
                return true;
                 
        }
        private void FindTargetdfPathFoldersIdRollback(int dfMethodsIdTarget)
        {
            var dfMethod = FindMethodInfo(dfMethodsIdTarget);
            dfPathFoldersIdTarget = Convert.ToInt32(dfMethod.Rows[0]["dfPathFoldersIdSource"]);
            if (dfPathFoldersId != Convert.ToInt32(dfMethod.Rows[0]["dfPathFoldersIdTarget"]))
                throw new Exception("Docflow Engine: Invalid route");
        }

      

        private void SetNewFolder()
        {
            if (MainTable == "" || DocumentsId <= 0 || dfPathFoldersId <= 0)
                throw new Exception("DocFlow Engine: Invalid document state");

            var UpdateClause = " SET dfPathFoldersId= " + this.dfPathFoldersIdTarget;
            if (this.EnableDfEvents)
             UpdateClause = UpdateClause+", dfEventsId = dfEvents+1";
            
            var Tmp = new TxDataSet();
            Tmp.ProviderName = "UPDATE " + MainTable +
                               UpdateClause+
                               " WHERE " + KeyField + " = " + this.DocumentsId.ToString() +
                               " AND dfPathFoldersId= " + this.dfPathFoldersId.ToString();
            Tmp.DBName = this.DBName;
            Tmp.Open();

          //  this.dfPathFoldersId = dfPathFoldersIdTarget;
            this.dfEventsIdTarget = dfEventsId + 1;
            // this.FolderType = -1;
            SaveLog();
        }

        private void ProcessNext()
        {
            var Document = new TDocFlowDocument(this);
            Document.DocumentsId = this.DocumentsId;
            Document.dfTypesId = this.dfTypesId;
            Document.dfPathFoldersId = this.dfPathFoldersIdTarget;
            Document.dfEventsId = this.dfEventsId;
            Document.DBName = this.DBName;
            Document.PushDefaultRoute();
        }


        private TxDataSet GetRuleList()
        {
            var Tmp = new TxDataSet();
            Tmp.ProviderName = "sysdf_PathFoldersRulesList";
            Tmp.SetParameter("dfPathFoldersId", dfPathFoldersIdTarget);
            Tmp.DBName = this.DBName;
            Tmp.Open();
            return Tmp;
        }
        

        private void ExecuteActions(bool IsRollBack)
        {
            var ActionList = new TDocFlowActionList(this);
            ActionList.DBName = this.DBName;
            ActionList.IsRollBack = IsRollBack;
            ActionList.Load(this.dfPathFoldersIdTarget);
            ActionList.ExecuteAll();
        }


        private void CheckInvalidRoute(DataTable Tmp)
        {
            if (Tmp.Rows.Count == 0)
                throw new Exception("Docflow Engine: Cannot push document, since there is no active methods for this folder.");

            if (Tmp.Rows.Count > 1 && (bool)Tmp.Rows[0]["IsDefault"]==false)
                throw new Exception("Docflow Engine: Cannot push document, since there are multiple outgoing methods with no default method specified");
        }
        public void PushDefaultRoute()
        {
            var Tmp = new TxDataSet();
            Tmp.ProviderName = "spui_dfMethodsListForPush";
            Tmp.SetParameter("dfTypesId", dfTypesId);
            Tmp.SetParameter("dfPathFoldersId", dfPathFoldersId);
            Tmp.DBName = this.DBName;
            Tmp.Open();
            CheckInvalidRoute(Tmp);
            int dfMethodsIdTarget = Convert.ToInt32(Tmp.Rows[0]["dfMethodsId"]);
            this.Push(dfMethodsIdTarget);
        }

        public void Push(int dfMethodsIdTarget)
        {
            this.dfMethodsIdTarget = dfMethodsIdTarget;
            CheckBasics();
            CheckUserPermission();
            FindDocFlowInformationBydfPathFoldersId();
            if (FindTargetdfPathFoldersIdPush(dfMethodsIdTarget))
            { 
                EnsureDocumentCorrectStatus();
                if (ValidateIncomingRules() == true)
                {
                    ExecuteActions(false);
                    SetNewFolder();
                    if (FolderTypeTarget == TFolderType.SubProcess || FolderTypeTarget == TFolderType.Document)
                    {
                        ProcessNext();
                    }
                }
            }
        }
        
        public void Rollback(int dfMethodsIdTarget)
        {
            this.dfMethodsIdTarget = dfMethodsIdTarget;
            CheckBasics();
            CheckUserPermission();
            FindDocFlowInformationBydfPathFoldersId();
            FindTargetdfPathFoldersIdRollback(dfMethodsIdTarget);
            EnsureDocumentCorrectStatus();
            if (ValidateIncomingRules() == true)
            {
                ExecuteActions(true);
                SetNewFolder();
            }
        }
    }
}
