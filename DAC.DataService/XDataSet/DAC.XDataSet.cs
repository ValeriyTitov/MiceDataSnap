using System;
using System.Data;
using System.Data.SqlClient;
using DAC.ObjectModels;

namespace DAC.XDataSet

{
    public class TxDataSet : DataTable

    {
        // [CoreDB].spui_GetUserList,10 - Закэшировать на 10 секунд ХП с базы CoreDB
        public const string sp_ParamsProcedure = "spsys_GetProcedureParams";

        public const string ParamName_ProcedureName = "ProcedureName";

        public string DBName { get { return this.ExecutionContext.DBName; } set { ExecutionContext.DBName = value; } }
        public string CommandName { get; set; }
        public string CacheRegion { get; set; }
        public string KeyField { get; set; }
        public int CacheDuration { get; set; }
        public bool CacheEmptyRecords { get; set; }
        public TMiceExecutionContext ExecutionContext { get; private set; }
        public bool UseDataAdapterToRead { get; set; }

        public void AddMessage(string AMessage)
        {
            var Msg = new TMiceDataSetMessage();
            Msg.AMessage = AMessage;
            ExecutionContext.Messages.Add(Msg);
        }

        public CommandBehavior Behavior;

        private SqlDataAdapter InternalGetAdapter()
        {
            if (FAdapter == null)
                FAdapter = new SqlDataAdapter();
            return FAdapter;
        }

        private SqlDataAdapter FAdapter;
        private bool FLocalCacheRequired;
        public SqlConnection Connection;
        private SqlCommand FCommand;
        private SqlDataAdapter Adapter { get { return InternalGetAdapter(); } }
        private TXParams Params { get { return ExecutionContext.Params; } }
        private bool ParamsInitialized;

        public TxDataSet()
        {
            ExecutionContext = new TMiceExecutionContext();
            ExecutionContext.Status = (int)TCommandStatus.etJustCreated; //TDACHistory.DATASET_JUST_CREATED;
            CacheEmptyRecords = true;
            ParamsInitialized = false;
            FCommand = new SqlCommand();
            UseDataAdapterToRead = false;
            Behavior=CommandBehavior.KeyInfo;
        }

        private void CheckBeforeUpdate()
        {
            if (String.IsNullOrEmpty(KeyField) || KeyField.Contains("[") || KeyField.Contains("]") || KeyField.Contains(";"))
            {
                throw new Exception("Keyfield field is not defined in "+ProviderName);
            }
        }
        private void CreateUpdateCommand()
        {
            SqlCommandBuilder builder = new SqlCommandBuilder(this.Adapter);
            Adapter.UpdateCommand = builder.GetUpdateCommand();
            Adapter.DeleteCommand = builder.GetDeleteCommand();
            Adapter.InsertCommand = TxDataSetHelper.CreateInsertCommand(builder.GetInsertCommand(), Connection, KeyField);
        }
          


        public void ApplyUpdates()
        {
            // https://stackoverflow.com/questions/5868313/ado-net-commandbuilder-insertcommand-and-default-constraints
            // There is no way for the CommandBuilder to know that you want to use the default value.
            // All it does is generate an insert statement, if there is a column it is going to generate it for the insert statement.
            // If that column is part of the insert statement, then any value given for that column, even null, will be inserted.
            // The default value is for situations where you do not include it in the insert statement.
            // It does not convert null into the default value, no matter how you try to insert the items, CommandBuilder or not.

            this.CheckBeforeUpdate();
            try
            {
                InitializeConnection();
                this.CreateUpdateCommand();
                this.ExecutionContext.Status = (int)TCommandStatus.etRunning;
                Adapter.Update(this);
                this.ExecutionContext.Status = (int)TCommandStatus.etDirectQuery;
            }

            finally
            {
                if (Connection != null)
                    Connection.Close();
            }

        }

        private string GetProvider()
        {
            return TxDataSetHelper.DBNameToStr(this.DBName)
                   + CommandName
                   + TxDataSetHelper.CacheDurationToStr(CacheDuration);
        }

        private void SetProvider(string value)
        {
            if (TxDataSetHelper.ProviderIsStoredProc(value) == true)
            {
                FCommand.CommandType = CommandType.StoredProcedure;
                this.CommandName = TxDataSetHelper.FindProviderCommandName(value);
                FLocalCacheRequired = (TSQLDataCache.LocalCacheRequired(DBName, CommandName) || (CacheDuration != 0));
            }
            else
            {
                FCommand.CommandType = CommandType.Text;
                this.CommandName = value;
                FLocalCacheRequired = false; //запросы нельзя кэшировать
            }
            this.ExecutionContext.ProviderName = value;
            ParamsInitialized = false;
        }

        private void CollectInfoMessages(object sender, SqlInfoMessageEventArgs args) //это CallBack для SQL Connection
        {
            foreach (SqlError err in args.Errors)
            {
                {
                    var Msg = new TMiceDataSetMessage();
                    Msg.AMessage = err.Message;
                    Msg.LineNumber = err.LineNumber;
                    Msg.Code = err.Number;

                    this.ExecutionContext.Messages.Add(Msg);
                }

            }
        }

        public void InitializeParams()
        {
            if ((FCommand.CommandType == CommandType.StoredProcedure) && (ParamsInitialized == false))
            {
                if (CommandName.Equals(sp_ParamsProcedure) == false)
                {
                    var DataSet = new TxDataSet();
                    DataSet.ProviderName = sp_ParamsProcedure;
                    DataSet.DBName = this.DBName;
                    DataSet.FCommand.Parameters.AddWithValue(ParamName_ProcedureName, this.CommandName);
                    DataSet.Open();
                    TXParams.InsertToSqlParameterCollection(Params, FCommand.Parameters, DataSet.Rows);
                }
                else
                    this.Params.CloneToSqlParameterCollection(FCommand.Parameters);

                ParamsInitialized = true;
            }

        }

        private void InitializeConnection()
        {
            if (Connection == null)
                Connection = new SqlConnection(ConnectionManager.GetConnectionString(this.DBName));

            Connection.InfoMessage += this.CollectInfoMessages;
            FCommand.Connection = Connection;
            FCommand.CommandText = this.CommandName.Trim();

        }



        private void PutToCache(string Hash)
        {
            if (String.IsNullOrEmpty(CacheRegion))
                CacheRegion = CommandName;

            if ((this.Rows.Count > 0) || ((this.Rows.Count == 0) && (this.CacheEmptyRecords == true)))
            {
                TSQLDataCache.SaveToCache(this, Hash, CacheDuration, CacheRegion);
            }
        }

        private void TryLoadFromCache()
        {
            var Hash = HashData();
            if (TSQLDataCache.LoadFromCache(this, Hash) == false)
            lock(TSQLDataCache.DefaultInstance.LockXmlCache)
            {
              if (TSQLDataCache.LoadFromCache(this, Hash) == false)
               {
                 DirectOpen();
                 PutToCache(Hash);
                 this.ExecutionContext.Status = (int)TCommandStatus.etJustAppServerCached; //TDACHistory.DATASET_JUST_APPSERVER_CACHED;
               }
            }
            else
                this.ExecutionContext.Status = (int)TCommandStatus.etLoadedFromAppServerCache; //TDACHistory.DATASET_LOADED_FROM_L2CACHE;
        }

        private void DirectOpen()
        {
            try
            {
                InitializeConnection();
                InitializeParams();
                Connection.Open();
                this.ExecutionContext.Status = (int)TCommandStatus.etRunning; //  TDACHistory.DATASET_RUNNING;

                if (this.UseDataAdapterToRead == true)
                {
                    Adapter.SelectCommand = FCommand;
                    Adapter.Fill(this);
                }
                else
                {
                    var AReader = FCommand.ExecuteReader(Behavior);
                    this.Load(AReader);
                }

                this.ExecutionContext.Status = (int)TCommandStatus.etDirectQuery; // TDACHistory.DATASET_DIRECT_QUERY;
            }

            finally
            {
                if (Connection != null)
                    Connection.Close();
            }

        }

        public void Open()
        {
            this.Clear();
            ExecutionContext.Messages.Clear();

            if (FLocalCacheRequired == true)
                TryLoadFromCache();
            else
                DirectOpen();
        }

        public string HashData()
        {
            this.InitializeParams();
            return DBName + CommandName + TParamUtils.ParamsToSrting(this.FCommand.Parameters);
        }

        public void SetParameter(string ParamName, object ParamValue)
        {
            this.Params.SetParameter(ParamName, ParamValue);
        }

        public int Status() { return this.ExecutionContext.Status; }

        public string ProviderName
        {
            get { return GetProvider(); }
            set { SetProvider(value); }
        }

        public void LoadFromExecutionContext(TMiceExecutionContext objectModel)
        {
            if (String.IsNullOrEmpty(objectModel.ProviderName) == false)
                this.ProviderName = objectModel.ProviderName;
            else
                this.ProviderName = "";

            if (String.IsNullOrEmpty(objectModel.DBName) == false)
                this.DBName = objectModel.DBName;
            else
                this.DBName = "";

            this.Params.CopyFrom(objectModel.Params);
        }

        public TMiceDataResponse ToNewMiceDataResponse()
        {
            var Result = new TMiceDataResponse();
            Result.DataSet = this;
            Result.ExecutionContext = this.ExecutionContext;
            return Result;
        }

        public void AppendByFieldName(DataTable ATable)
        {
            var Appender = new TxDataSetAppender();
            Appender.FromTable = ATable;
            Appender.ToTable = this;
            if (ATable == this)
                throw new Exception("AppentByFieldName: Cannot copy to itself");
            Appender.AppendByFieldName();
        }
    }
}