using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DAC.ObjectModels;
using DAC.XDataSet;

namespace DAC.DataService.DocFlow
{
    public class sysdf_DocumentPush : TBasicDataClass
    {
        public override TxDataSet Run(TMiceDataRequest MiceRequest, TMiceUser MiceUser)
        {
            var Document = new TDocFlowDocument(null);
            Document.DBName = MiceRequest.ExecutionContext.DBName;

            TXParams Params;
            Params = MiceRequest.ExecutionContext.Params;
            Params.Check("DocumentsId");
            Params.Check("dfPathFoldersIdSource");
            Params.Check("dfMethodsIdTarget");
            

            Document.DocumentsId = Params.AsInteger("DocumentsId");
            Document.dfPathFoldersId = Params.AsInteger("dfPathFoldersIdSource");
            if (Params.ContainsKey("dfEventsId")==true)
             Document.dfEventsId = Params.AsInteger("dfEventsId");
            int dfMethodsIdTarget = Params.AsInteger("dfMethodsIdTarget");
            
            Document.Push(dfMethodsIdTarget);
           
            Document.Result.LoadFromExecutionContext(MiceRequest.ExecutionContext);
            return Document.Result;
        }
    }
}
