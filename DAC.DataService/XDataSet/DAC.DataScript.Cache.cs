using System;
using System.CodeDom.Compiler;
using System.Runtime.Caching;

namespace DAC.XDataSet

{

    public class TDataScriptCache
    {
        private MemoryCache Cache { get; set; }

        private MemoryCache Scripts { get; set; }

        private static TDataScriptCache DefaultInstance = new TDataScriptCache();

        private TDataScriptCache()
        {
            Cache = new MemoryCache("C#CompileResults");
            Scripts = new MemoryCache("C#TextScripts");
        }

        public static bool ContainsScript(string ScriptName)
        {
            return DefaultInstance.Scripts.Contains(ScriptName);
        }

        public static bool LoadCompilationFromCache(ref CompilerResults CompiledResult, string AKey)
        {
            CacheItem Item;
            Item = DefaultInstance.Cache.Get(AKey) as CacheItem;
            if (Item != null)
            {
                CompiledResult = Item.Value as CompilerResults;
                return true;
            }
            else
                return false;
        }

        public static void SaveCompilationToCache(string AKey, CompilerResults CompiledResult)
        {
            if (DefaultInstance.Cache.Contains(AKey) == false)
            {
                var Item = new CacheItem(AKey, CompiledResult, "");
                var Policy = new CacheItemPolicy();
                DefaultInstance.Cache.Add(AKey,Item, Policy);
            }
        }

        public static void Clear()
        {
            DefaultInstance.Cache.Dispose();
            DefaultInstance.Cache = new MemoryCache("C#Scripts");

            DefaultInstance.Scripts.Dispose();
            DefaultInstance.Scripts = new MemoryCache("C#Scripts");
        }

        public static void RemoveCompilation(string ScriptName)
        {
            if (DefaultInstance.Cache.Contains(ScriptName) == true)
                DefaultInstance.Cache.Remove(ScriptName);
        }

        public static void SetStorageScriptText(string ScriptName, string ScriptText)
        {
            if (DefaultInstance.Scripts.Contains(ScriptName) == true)
                DefaultInstance.Scripts.Remove(ScriptName);

            if (DefaultInstance.Cache.Contains(ScriptName) == true)
                DefaultInstance.Cache.Remove(ScriptName);

            var Item = new CacheItem(ScriptName, ScriptText);
            var Policy = new CacheItemPolicy();
            DefaultInstance.Scripts.Add(ScriptName, Item, Policy);
        }

        public static string LoadScript(string ScriptName)
        {
            if (DefaultInstance.Scripts.Contains(ScriptName))
            {
                CacheItem Item;
                Item = DefaultInstance.Scripts.Get(ScriptName) as CacheItem;
                return Item.Value.ToString();
            }
            else
                throw new Exception($"Script not found '{ScriptName}'");
        }

     }
}
