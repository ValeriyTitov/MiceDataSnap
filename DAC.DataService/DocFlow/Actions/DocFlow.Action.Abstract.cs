using DAC.XDataSet;
using System;
using System.Collections.Generic;
using System.Data;

namespace DAC.DataService.DocFlow.Actions
{
    public abstract class TAbstractDocFlowAction
    {
        public TAbstractDocFlowAction(TDocFlowEntity AEntity, DataRow AAction)
        {
            Entity = AEntity;
            Map(AAction);
        }

        public int dfPathFolderActionsId { get; private set; }
        public int dfPathFoldersId { get; private set; }
        public int ActionType { get; private set; }
        public bool Active { get; private set; }
        public int OnError { get; private set; }
        public int PushOrRollback { get; private set; }
        public bool RunSynchro { get; private set; }
        public bool RequiresTransaction { get; private set; }
        public bool UseExpression { get; private set; }
        public string Expression { get; private set; }
        public string UserInformation { get; private set; }
        public string ProviderName { get; private set; }
        public string DBName { get; set; }
        public string Caption { get; private set; }
        public int ImageIndex { get; set; }
        public TDocFlowEntity Entity { get; private set; }
        private void Map(DataRow Row)
        {
            dfPathFolderActionsId = Convert.ToInt32(Row["dfPathFolderActionsId"]);
            ActionType = Convert.ToInt32(Row["ActionType"]);
            ImageIndex = Convert.ToInt32(Row["ImageIndex"]);
            PushOrRollback = Convert.ToInt32(Row["PushOrRollback"]);
            RunSynchro = (bool)Row["RunSynchro"];
            RequiresTransaction = (bool)Row["RequiresTransaction"];
            UseExpression = (bool)Row["UseExpression"];
            Expression = Row["Expression"].ToString();
            UserInformation = Row["UserInformation"].ToString();
            ProviderName = Row["ProviderName"].ToString();
            DBName = Row["DBName"].ToString();
            Caption = Row["Caption"].ToString();
            OnError= Convert.ToInt32(Row["OnError"]);
            Active = (bool)Row["Active"];
        }
        public abstract void Execute();

        public void AddToResultData(int ImageIndex, string Caption, string UserMessage)
        { 
            var Row = Entity.Result.NewRow();
            Row["ImageIndex"] = ImageIndex;
            Row["Caption"] = Caption;
            Row["UserMessage"] = UserMessage;
            Entity.Result.Rows.Add(Row);
        }

    }

}
