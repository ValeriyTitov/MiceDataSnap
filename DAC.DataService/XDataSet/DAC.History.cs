namespace DAC.XDataSet
{
    public enum TCommandStatus { etUnknown, etError, etJustCreated, etRunning, etDirectQuery, etJustLocallyCached, etLoadedFromLocalCache, etJustAppServerCached, etLoadedFromAppServerCache, etDataScript, etCanceling, etCanceled };

    class TDACHistory
    {
        


        /*
        public string StatusToStr(int Status)
        {
            switch (Status)
            {
                case DATASET_CREATED: return "DATASET_CREATED";
                case DATASET_DIRECT_OPEN: return "DATASET_DIRECT_OPEN";
                case DATASET_JUST_CACHED: return "DATASET_JUST_CACHED";
                case DATASET_LOADED_FROM_L1CACHE: return "DATASET_LOADED_FROM_L1CACHE";
                case DATASET_LOADED_FROM_L2CACHE: return "DATASET_LOADED_FROM_L2CACHE";
                case DATASET_ERROR: return "DATASET_ERROR";
                case DATASET_PROCESSED_BY_APP_SERVER: return "DATASET_PROCESSED_BY_APP_SERVER";
                default: return "UNKNOWN";
            }

        }*/

    }
}
