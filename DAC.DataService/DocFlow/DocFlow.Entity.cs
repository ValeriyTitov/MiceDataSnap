using DAC.XDataSet;
using System;
using System.Data;
using System.Data.SqlClient;


namespace DAC.DataService.DocFlow
{

    public class TFolderType
    {
        public const int Process = 0;
        public const int Decision = 1;
        public const int SubProcess = 2;
        public const int Document = 3;
        public const int OnPageReference = 4;
        public const int OffPageReference = 5;
        public const int StartEvent = 6;
        public const int EndEvent = 7;
    }
    public class TDocFlowEntity
    {
        public int DocumentsId { get; set; }
        public int dfMethodsIdTarget { get; set; }
        public int dfPathFoldersId { get; set; }
        public int dfClassesId { get; set; }
        public int dfTypesId { get; set; }
        public int dfEventsId { get; set; }
        public int FolderType { get; set; }
        public string DBName { get; set; }
        public string MainTable { get; set; }
        public string DataView { get; set; }
        public string KeyField { get; set; }
        public bool EnableDfEvents { get; set; }
        public string LogProviderName { get; set; }
        public TxDataSet Document { get; private set; }
        public TxDataSet Result { get; private set; }
        public TDocFlowEntity Parent { get; private set; }
        public SqlTransaction CurrentTransaction { get; private set; }
        public SqlConnection Connection { get; private set; }
        public TDocFlowEntity(TDocFlowEntity AParent)
        {
            Parent = AParent;
            if (Parent == null)
                Result = this.CreateResultTable();
            else
                Result = Parent.Result;
         }
        protected void SaveLog()
        {
            if (String.IsNullOrEmpty(LogProviderName) == false )
            {
                var NewDocumentState = this.CreateDocumentRequest();
                if (NewDocumentState.Rows.Count > 0)
                {
                    var Tmp = new TxDataSet();
                    Tmp.ProviderName = this.LogProviderName;
                    Tmp.DBName = this.DBName;
                    Tmp.ExecutionContext.Params.LoadFromDataRow(NewDocumentState.Rows[0]);
                    Tmp.SetParameter("dfMethodsId", this.dfMethodsIdTarget);
                    Tmp.SetParameter("DocumentsId", this.DocumentsId);
                    Tmp.Open();
                }
            }
        }
        protected TxDataSet CreateDocumentRequest()
        {
            var NewResult = new TxDataSet();
            NewResult.ProviderName = "SELECT * FROM " + DataView + " WITH (NOLOCK) WHERE " + KeyField + " = " + this.DocumentsId.ToString();
            NewResult.DBName = this.DBName;
            NewResult.Open();
            return NewResult;
        }
        protected void EnsureDocumentCorrectStatus()
        {
            if (Document == null)
                Document = this.CreateDocumentRequest();
            
            if (Document.Rows.Count == 0)
                throw new Exception("Document not found");
            if (this.dfPathFoldersId != Convert.ToInt32(Document.Rows[0]["dfPathFoldersId"]))
                throw new Exception("Docflow Engine: Document changed it state");
            if (EnableDfEvents == true)
            {
                if (Document.Columns.Contains("dfEventsId")==false)
                    throw new Exception("Docflow Engine: Document class was market with dfEventsEnabled, but dfEventsId field was not found.");
                
                if (dfEventsId != Convert.ToInt32(Document.Rows[0]["dfEventsId"]))
                    throw new Exception("Docflow Engine: Document has changed it state");

            }
        }
        protected void CheckUserPermission()
        { 
        
        }
        protected void FindDocFlowInformationBydfPathFoldersId()
        {
            if (dfPathFoldersId<1)
                throw new Exception("Docflow Engine: dfPathFoldersId is empty");

            var Tmp = new TxDataSet();
            Tmp.ProviderName = "spui_dfPathFoldersInfo";
            Tmp.SetParameter("dfPathFoldersId", dfPathFoldersId);
            Tmp.DBName = this.DBName;
            Tmp.Open();
            if (Tmp.Rows.Count != 1)
                throw new Exception("Docflow Engine: Cannot find document or folder inactive");
            
            this.MainTable = Tmp.Rows[0]["MainTable"].ToString();
            this.DataView = Tmp.Rows[0]["DataView"].ToString();
            this.KeyField = Tmp.Rows[0]["KeyField"].ToString();
            this.dfClassesId = Convert.ToInt32(Tmp.Rows[0]["dfClassesId"]);
            this.dfTypesId = Convert.ToInt32(Tmp.Rows[0]["dfTypesId"]);
            this.EnableDfEvents = (bool)Tmp.Rows[0]["EnableDfEvents"];
            this.FolderType = Convert.ToInt32(Tmp.Rows[0]["FolderType"]);
            this.LogProviderName = Tmp.Rows[0]["LogProviderName"].ToString();
        }
        protected void CheckBasics()
        {
            if (this.DocumentsId <= 0)
                throw new Exception("Docflow Engine: DocumentsId not specified");
            else
             if (dfPathFoldersId<=0)
                throw new Exception("Docflow Engine: No document information specified");
            
            CheckRecursiveRun();
        }
        protected void CheckRecursiveRun()
        {
            TDocFlowEntity AParent = Parent;
            if (AParent!=null)
            do
            {
                if (DocumentsId == AParent.DocumentsId && dfPathFoldersId == AParent.dfPathFoldersId)
                    throw new Exception("DocFlow Engine: Recursive run detected");
                AParent = AParent.Parent;
            } 
            while (AParent!=null);
        }
        private TxDataSet CreateResultTable()
        {
            DataColumn c;
            var AResult = new TxDataSet();
            AResult.TableName = "@Result";
            AResult.Columns.Add(new DataColumn("ImageIndex", typeof(int)));
            c = new DataColumn("Caption", typeof(string));
            c.MaxLength = 255;
            AResult.Columns.Add(c);
            c = new DataColumn("UserMessage", typeof(string));
            c.MaxLength = 255;
            AResult.Columns.Add(c);
            return AResult;
        }
    }
}
