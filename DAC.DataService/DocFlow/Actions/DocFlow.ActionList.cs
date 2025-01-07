using DAC.XDataSet;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAC.DataService.DocFlow.Actions
{

    public class TDocFlowActionList : List<TAbstractDocFlowAction>
    {
        
        private void HandleException(string ExceptionMessage, TAbstractDocFlowAction DocFlowAction)
        {
            this.Entity.Result.ExecutionContext.Messages.AddMessage("Error: "+DocFlowAction.Caption);
            const int IMAGEINDEX_ERROR = 229;
            switch (DocFlowAction.OnError)
            {
                case 1:; break;
                case 2: DocFlowAction.AddToResultData(IMAGEINDEX_ERROR, DocFlowAction.Caption, ExceptionMessage); break;
                default: throw new Exception(ExceptionMessage);
            
            }
        }
 

        public string DBName { get; set; }
        public bool IsRollBack { get; set; }
        public TDocFlowEntity Entity { get; private set; }

        public TDocFlowActionList(TDocFlowEntity AEntity)
        {
            Entity = AEntity;
        }

        
        private void CreateSingleAction(DataRow Row, int ActionType)
        {
            TAbstractDocFlowAction AAction;
            switch (ActionType)
            {
                case 0: AAction = new TSendMessageAction(Entity, Row); break;

                case 1: AAction = new TStoredProcedureAction(Entity, Row); break;

                default: throw new Exception($"Doc flow: Unknow action type: {ActionType}"); 
            }
            Add(AAction);
        }
        private void CreateActions(TxDataSet DataSet)
        {
            int ActionType;
            foreach (DataRow Row in DataSet.Rows)
            {
               ActionType = (int)Row["ActionType"];
               CreateSingleAction(Row, ActionType);
            }
        
        }
        public void Load(int dfPathFoldersId)
        {
            var Tmp = new TxDataSet();
            Tmp.ProviderName = "sysdf_PathFoldersActionList";
            Tmp.SetParameter("dfPathFoldersId", dfPathFoldersId);
            Tmp.DBName = this.DBName;
            Tmp.Open();
            if (Tmp.Rows.Count>0)
             CreateActions(Tmp);
        }

        private bool NeedExecute(TAbstractDocFlowAction Action)
        {
            if (Action.Active == false)
                return false;
            
            bool Allowed;
            Allowed = ((Action.PushOrRollback == 0 && this.IsRollBack == false) //only for push
                || (Action.PushOrRollback == 1 && this.IsRollBack == true) //only for rollback
                || (Action.PushOrRollback == 2)); // always execute

            return Allowed;
        }
        public void ExecuteAll()
        {
            foreach (var DocFlowAction in this)
            {
                DocFlowAction.DBName = this.DBName;
                try
                {
                    if (NeedExecute(DocFlowAction) == true)
                    {
                        DocFlowAction.Execute();
                        this.Entity.Result.ExecutionContext.Messages.AddMessage("Done: "+DocFlowAction.Caption);
                    }
                }
                catch (Exception e)
                {
                    this.HandleException(e.Message, DocFlowAction);
                }

            }
        
        }
    }
}
