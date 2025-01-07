using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Reflection;
using DAC.ObjectModels;

namespace DAC.XDataSet
{
    public class TScriptRunner
    {
        public const string  DacDlllocation = @"c:\MiceDataSnap\WebServer\bin\DAC.DataService.dll";
        private CompilerResults CompiledResult { get; set; }

        private Type[] TypesList;

        private Type ClassByName(string Name)
        {
            foreach (Type type in TypesList)
            {
                if (type.Name.Equals(Name))
                    return type;
            }

            throw new Exception($"Class not found {Name}");
        }

        private CompilerResults CreateBinary()
        {
            var Provider = new CSharpCodeProvider();
            var CompilerParams = new CompilerParameters(new string[] { "System.dll", "System.Xml.Linq.dll", "System.Net.Http.dll", "System.Data.dll", "System.xml.dll", DacDlllocation })
            {
                GenerateInMemory = true,
                IncludeDebugInformation = true,
                //GenerateExecutable = true,
                //OutputAssembly = "",
            };

            var Result = Provider.CompileAssemblyFromSource(CompilerParams, new string[] { ScriptText }); // (несколько членов массива string[] рассматриваются как несколько разных файлов с исходниками)
            if (Result.Errors.HasErrors)
            {
                foreach (CompilerError Error in Result.Errors)
                {
                    throw new Exception($"Script compilation failed. Error at line {Error.Line} column {Error.Column}: {Error.ErrorText}");
                }
            }
            return Result;
        }

        private void Prepare()
        {
            var dll = CompiledResult.CompiledAssembly;
            TypesList = dll.GetTypes();
        }

        private MethodInfo MethodByName(Type type, string Name)
        {
            var Methods = type.GetMethods();
            foreach (MethodInfo Method in Methods)
            {
                if (Method.Name.Equals(Name))
                    return Method;
            }
            throw new Exception($"Method not found {type.Name}.{Name}");
        }

        private void MapParameters(object[] Params)
        {
            if (Params.Length > 1)
            {
                Params[0] = MiceRequest;
                Params[1] = MiceUser;
            }
        }

        public string ScriptText { get; set; }

        public string ScriptName { get; set; }

        public TMiceDataRequest MiceRequest { get; set; }

        public TMiceUser MiceUser { get; set; }

        public void LoadScriptText()
        {
          if (String.IsNullOrEmpty(ScriptText) && String.IsNullOrEmpty(ScriptName)==false)
                ScriptText = TDataScriptCache.LoadScript(ScriptName);
        }

        public void Compile(bool UseCache)
        {
            CompilerResults Result = null;

            if (UseCache == false)
            {
                this.CompiledResult = CreateBinary();
                Prepare();
            }
            else
            {
                if (TDataScriptCache.LoadCompilationFromCache(ref Result, ScriptName) == false)
                {
                    CompiledResult = CreateBinary();
                    Prepare();
                    TDataScriptCache.SaveCompilationToCache(ScriptName, CompiledResult);
                }
                else
                {
                    this.CompiledResult = Result;
                    Prepare();
                }
            }
        }

        public object Run(string AClassName, string AMethodName)
        {
            if (this.CompiledResult == null)
                throw new Exception("Script not compiled");

            var AClass = this.ClassByName(AClassName);
            var Instance = Activator.CreateInstance(AClass);

            var Method = this.MethodByName(AClass, AMethodName);

            object[] MethodParams = new object[Method.GetParameters().Length];
            this.MapParameters(MethodParams);

            return Method.Invoke(Instance, MethodParams);
        }

        public static bool ContainsScript(string ScriptName)
        {
            return TDataScriptCache.ContainsScript(ScriptName);
        }

        public static void ClearCache()
        {
            TDataScriptCache.Clear();
        }

        public static void PublishScript(string ScriptName, string ScriptText)
        {
            TDataScriptCache.RemoveCompilation(ScriptName);
            var Runner = new TScriptRunner();
            Runner.ScriptText=ScriptText;
            Runner.ScriptName = ScriptName;
            Runner.Compile(true);
            TDataScriptCache.SetStorageScriptText(ScriptName, ScriptText);
        }

        public static void SetStorageScriptText(string ScriptName, string ScriptText)
        {
           TDataScriptCache.SetStorageScriptText(ScriptName, ScriptText);
     
        }

    }

}
