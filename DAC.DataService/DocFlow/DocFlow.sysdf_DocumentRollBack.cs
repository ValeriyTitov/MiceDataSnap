using DAC.ObjectModels;
using DAC.XDataSet;
using System;

namespace DAC.DataService.DocFlow
{
    class sysdf_DocumentRollBack: TBasicDataClass
    {
        public override TxDataSet Run(TMiceDataRequest MiceRequest, TMiceUser MiceUser)
        {
            var Document = new TDocFlowDocument(null);
            Document.DocumentsId = Convert.ToInt32(MiceRequest.ExecutionContext.Params["DocumentsId"]);
            Document.dfPathFoldersId = Convert.ToInt32(MiceRequest.ExecutionContext.Params["dfPathFoldersIdSource"]);
            if (MiceRequest.ExecutionContext.Params.ContainsKey("dfEventsId") == true)
                Document.dfEventsId = Convert.ToInt32(MiceRequest.ExecutionContext.Params["dfEventsId"]);
            Document.DBName = MiceRequest.ExecutionContext.DBName;
            int dfMethodsIdTarget = Convert.ToInt32(MiceRequest.ExecutionContext.Params["dfMethodsIdTarget"]);
            Document.Rollback(dfMethodsIdTarget);
           
            var Result = new TxDataSet();
            Result.LoadFromExecutionContext(MiceRequest.ExecutionContext);
            return Result;
        }
    }
}
