using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Caching;
using System.Data;

namespace DAC.XDataSet
{
    /*    class TCacheChangeMonitor : ChangeMonitor
        {

        }
    */

    internal class TDataCacheItem
    {
        public string CacheRegion { get; set; }
        public byte[] Data;
        public byte[] Schema;
    }

    class TSQLDataCache
    {
        private List<string> CachableProceduresList { get; set; }

        private MemoryCache XmlCache { get; set; } //Обычный кэш хранится в Xml.

        private MemoryCache JsonCache { get; set; } //Кэшируем уже сериализованный в Json класс TMiceDataResponse. Не требуются затрата на повторную сериализацию.

        public object LockXmlCache { get; set; }

        public object LockJsonCache { get; set; }

        private TSQLDataCache()
        {
            XmlCache = new MemoryCache("C#XmlCache");
            JsonCache = new MemoryCache("C#JsonCache");
            CachableProceduresList = new List<string>();
            LockXmlCache = new object();
            LockJsonCache = new object();
            //CachableProceduresList.Add("spww_GetEmployeeDetails");
            //CachableProceduresList.Add("spww_EmployeeList");
            CachableProceduresList.Add("spsys_GetProcedureParams");

            CachableProceduresList.Add("spsys_LockTest");
           
    }

        public static TSQLDataCache DefaultInstance = new TSQLDataCache();

        public static void SaveToCache(DataTable Data, string AKey, int Duration, string ACacheRegion)
        {
            var DCI = new TDataCacheItem();
            var Item = new CacheItem(AKey, DCI, ACacheRegion);
            var Policy = new CacheItemPolicy();
            if (Duration > 0)
                Policy.AbsoluteExpiration = DateTime.Now.AddSeconds(Duration);

            var M1 = new MemoryStream();
            Data.TableName = AKey;
            Data.WriteXml(M1);
            DCI.Data = M1.ToArray();
            DCI.CacheRegion = ACacheRegion;

            var M2 = new MemoryStream();
            Data.WriteXmlSchema(M2);
            DCI.Schema = M2.ToArray();

            DefaultInstance.XmlCache.Add(Item, Policy);
        }

        public static bool LoadFromCache(DataTable Data, string AKey)
        {
            TDataCacheItem DCI;
            DCI = DefaultInstance.XmlCache.Get(AKey) as TDataCacheItem;
            if (DCI != null)
            {
                var M1 = new MemoryStream(DCI.Data);
                var M2 = new MemoryStream(DCI.Schema);
                Data.TableName = AKey;
                Data.ReadXmlSchema(M2);
                Data.ReadXml(M1);
                return true;
            }
            else
                return false;
        }

        public static void SaveToCacheJson(string Data, string AKey, int Duration, string ACacheRegion)
        {
            var Item = new CacheItem(AKey, Data, ACacheRegion);
            var Policy = new CacheItemPolicy();
            if (Duration > 0)
                Policy.AbsoluteExpiration = DateTime.Now.AddSeconds(Duration);

            DefaultInstance.JsonCache.Add(Item, Policy);
        }

        public static bool LoadFromCacheJson(ref string Data, string AKey)
        {
            var Item = DefaultInstance.JsonCache.Get(AKey);
            if (Item != null)
            {
                Data = Item.ToString();
                return true;
            }
            else
                return false;
        }

        public static bool LocalCacheRequired(string DBName, string ProcedureName)
        {
            return (DefaultInstance.CachableProceduresList.IndexOf(ProcedureName) >= 0);
        }

        public static void ClearEntireCache()
        {
            DefaultInstance.XmlCache = new MemoryCache("C#XmlCache");
            DefaultInstance.JsonCache = new MemoryCache("C#JsonCache");
        }

    }
}
