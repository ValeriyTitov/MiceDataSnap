using DAC.XDataSet;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAC.DataService.DocFlow.Actions
{
    public class TStoredProcedureAction : TAbstractDocFlowAction
    {
        public TStoredProcedureAction(TDocFlowEntity AEntity, DataRow AAction) : base(AEntity, AAction)
        {
            //return base(ADocument, AAction);

        }

        private void CreateUserMessage(TxDataSet Tmp)
        {
            StringBuilder Builder = new StringBuilder();
            foreach (DataRow Row in Tmp.Rows)
                Builder.AppendLine(Row["UserMessage"].ToString());
            
            AddToResultData(this.ImageIndex, this.Caption, Builder.ToString());
        }
        public override void Execute()
        {
            var Tmp = new TxDataSet();
            Tmp.DBName = this.DBName;
            Tmp.ProviderName = this.ProviderName;
            Tmp.ExecutionContext.Params.LoadFromDataRow(this.Entity.Document.Rows[0]);
            Tmp.Open();
            if (Tmp.Columns.Contains("UserMessage") == true && Tmp.Rows.Count>0)
            {
                this.CreateUserMessage(Tmp);
            }
        }
    }
}
