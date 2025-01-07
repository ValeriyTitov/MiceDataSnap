using DAC.DataService.DocFlow;
using DAC.ObjectModels;
using DAC.XDataSet;
using Newtonsoft.Json;
using System;
using DAC.QueueManager;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAC.DataService
{
    public class TMiceDataProcess
    {
        public TMiceDataRequest MiceRequest;
        public TMiceDataResponse MiceResponse { get; private set; }

        public string StringResult;

        public TxDataSet Query;

        public TMiceUser User { get; set; }
        private void PublishDataScript()
        {
            var AScriptName = MiceRequest.ExecutionContext.Params["ScriptName"].ToString();
            var AScriptText = MiceRequest.ExecutionContext.Params["ScriptText"].ToString();
            TScriptRunner.PublishScript(AScriptName, AScriptText);
            var DataSet = new TxDataSet();
            DataSet.LoadFromExecutionContext(MiceRequest.ExecutionContext);
            this.MiceResponse = DataSet.ToNewMiceDataResponse();
            MiceResponse.ExecutionContext.Status = (int)TCommandStatus.etDataScript; ; //Source : DataScript
        }
        private void ProcessDataScript()
        {
            var Scripter = new TScriptRunner();
            Scripter.ScriptName = MiceRequest.ExecutionContext.CommandName();
            Scripter.MiceRequest = MiceRequest;
            Scripter.MiceUser = User;
            Scripter.LoadScriptText();
            Scripter.Compile(true);

            var TempResult = Scripter.Run(Scripter.ScriptName, "Run");
            if ((TempResult == null) || (TempResult is TxDataSet == false))
                throw new Exception("Datascript did not return valid data.");
            Query = TempResult as TxDataSet;

            MiceResponse = Query.ToNewMiceDataResponse();
            MiceResponse.ExecutionContext.Status = (int)TCommandStatus.etDataScript; ; //Source : DataScript
        }


        private void RunDataScript()
        {
            var Scripter = new TScriptRunner();
            Scripter.MiceRequest = MiceRequest;
            Scripter.MiceUser = User;
            Scripter.ScriptText = MiceRequest.ExecutionContext.Params["ScriptText"].ToString();
            Scripter.ScriptName = MiceRequest.ExecutionContext.Params["ScriptName"].ToString();
            Scripter.MiceRequest.ExecutionContext.ProviderName = Scripter.ScriptName;
            Scripter.Compile(false);

            var TempResult = Scripter.Run(Scripter.ScriptName, "Run");
            if ((TempResult == null) || (TempResult is TxDataSet == false))
                throw new Exception("Datascript did not return valid data.");
            Query = TempResult as TxDataSet;
            Query.ProviderName = "spds_DataScriptRun";
            MiceResponse = Query.ToNewMiceDataResponse();
            MiceResponse.ExecutionContext.Status = (int)TCommandStatus.etDataScript; ; //Source : DataScript
        }

        private void DocFlowDocumentPushQueue()
        {
            string Payload = JsonConvert.SerializeObject(MiceRequest);
            StringResult = QueueManagerClient.DefaultInstance.Call(Payload);
            TMiceException.Check2(StringResult);
            this.MiceResponse = JsonConvert.DeserializeObject<TMiceDataResponse>(StringResult); 
        }

        private void DocFlowDocumentPush()
        {
            var Document = new TDocFlowDocument(null);
            this.Query = Document.Result;
            Document.DBName = MiceRequest.ExecutionContext.DBName;

            TXParams Params;
            Params = MiceRequest.ExecutionContext.Params;
            Params.Check("DocumentsId");
            Params.Check("dfPathFoldersIdSource");
            Params.Check("dfMethodsIdTarget");


            Document.DocumentsId = Params.AsInteger("DocumentsId");
            Document.dfPathFoldersId = Params.AsInteger("dfPathFoldersIdSource");
            if (Params.ContainsKey("dfEventsId") == true)
                Document.dfEventsId = Params.AsInteger("dfEventsId");
            int dfMethodsIdTarget = Params.AsInteger("dfMethodsIdTarget");

            Document.Push(dfMethodsIdTarget);

            Document.Result.LoadFromExecutionContext(MiceRequest.ExecutionContext);

            MiceResponse = Query.ToNewMiceDataResponse();
            MiceResponse.ExecutionContext.Status= (int)TCommandStatus.etDataScript;
        }

        private void DocFloDocumentRollback()
        {
            var RollbackDocument = new sysdf_DocumentRollBack();
            MiceResponse = RollbackDocument.Run(MiceRequest, User).ToNewMiceDataResponse();
            MiceResponse.ExecutionContext.Status = (int)TCommandStatus.etDataScript;
        }

        private bool RunExtendedScript()
        {
            var CommandName = MiceRequest.ExecutionContext.CommandName();

            if (CommandName.Equals("spds_DataScriptPublish") == true)
            {
                PublishDataScript();
                return true;
            }
            else
            if (CommandName.Equals("spds_DataScriptRun") == true)
            {
                RunDataScript();
                return true;
            }
            else 
            if (CommandName.Equals("sysdf_DocumentPush") == true)
            {
                //DocFlowDocumentPush();
                DocFlowDocumentPushQueue();
                return true;
            }
            else
            if (CommandName.Equals("sysdf_DocumentRollback") == true)
            {
                DocFloDocumentRollback();
                return true;
            }
            else
            if (TScriptRunner.ContainsScript(CommandName))
            {
                ProcessDataScript();
                return true;
            }
            return false;
        }
        
        private void DirectQuery()
        {

            Query = new TxDataSet();
            Query.LoadFromExecutionContext(MiceRequest.ExecutionContext);
            if (MiceRequest.ExecutionContext.ApplicationContext.ContainsKey("CommandBehavior"))
                Query.Behavior = TxDataSetHelper.CommandBehaviorFromStringDef(MiceRequest.ExecutionContext.ApplicationContext["CommandBehavior"], Query.Behavior);
            Query.Open();

            MiceResponse = Query.ToNewMiceDataResponse();
        }


        private void InternalCreateResponse()
        {
            if (RunExtendedScript()==false)
                DirectQuery();

            if (MiceResponse == null)
                throw new Exception("Failed to process query. No data returned.");
        }

        private void InternalLoadFromCache(string Hash)
        {
            if (TSQLDataCache.LoadFromCacheJson(ref StringResult, Hash) == false)
                lock (TSQLDataCache.DefaultInstance.LockJsonCache)
                {
                    if (TSQLDataCache.LoadFromCacheJson(ref StringResult, Hash) == false)
                    {
                        InternalCreateResponse();
                        MiceResponse.ExecutionContext.Status = (int)TCommandStatus.etLoadedFromAppServerCache;
                        StringResult = MiceResponse.ToString();
                        TSQLDataCache.SaveToCacheJson(StringResult, Hash, 0, "");
                        MiceResponse.ExecutionContext.Status = (int)TCommandStatus.etJustAppServerCached;
                        StringResult = MiceResponse.ToString();
                    }
                }
        }

        public void ProceedRequest()
        {
            if (TSQLDataCache.LocalCacheRequired(MiceRequest.ExecutionContext.DBName, MiceRequest.ExecutionContext.CommandName()))
            {
                var Hash = MiceRequest.ExecutionContext.CalculateHash();
                InternalLoadFromCache(Hash);
            }
            else
            {
                InternalCreateResponse();
                if (String.IsNullOrEmpty(StringResult))
                 StringResult = MiceResponse.ToString();
            }
        }
    }
}
