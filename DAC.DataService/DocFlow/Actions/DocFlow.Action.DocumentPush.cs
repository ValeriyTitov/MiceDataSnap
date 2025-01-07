using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAC.DataService.DocFlow.Actions
{
    public class TDocumentPushAction : TAbstractDocFlowAction
    {
        public TDocumentPushAction(TDocFlowEntity AEntity, DataRow AAction) : base(AEntity, AAction)
        {


        }

        public override void Execute()
        {
            var Document = new TDocFlowDocument(Entity);
            Document.DocumentsId = Entity.DocumentsId;
            Document.dfPathFoldersId = Entity.dfPathFoldersId;
            Document.dfEventsId = Entity.dfEventsId;
            Document.DBName = Entity.DBName;
            Document.PushDefaultRoute();
        }
    }
}