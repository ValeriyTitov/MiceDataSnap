using System;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DAC.ObjectModels
{
    public class TMiceException
    {
        public string ErrorMessage { get; set; }
        public string ExceptionClassName { get; set; }
        public int LineNumber { get; set; }
        public int ColumnNumber { get; set; }
        public int SQLNativeError { get; set; }

        public TMiceDatasetMessageList Messages;
        public void ToJsonObject(JObject jObject)
        {
            jObject.Add("ErrorMessage", ErrorMessage);
            jObject.Add("ExceptionClass", ExceptionClassName);
            jObject.Add("LineNumber", LineNumber);
            jObject.Add("SQLNativeError", SQLNativeError);
            jObject.Add("ColumnNumber", ColumnNumber);
            if (Messages != null)
                Messages.ToJsonObject(jObject);
        }

        public override string ToString()
        {
            var Result = new JObject();
            var Content = new JObject();
            this.ToJsonObject(Content);
            Result.Add("Exception", Content);
            return Result.ToString();
        }

        public static TMiceException CreateException(System.Exception e)
        {
            var Result = new TMiceException();
            if (e.InnerException == null)
                Result.ErrorMessage = e.Message;
            else
                Result.ErrorMessage = e.InnerException.Message; 
            StackTrace trace = new System.Diagnostics.StackTrace(e, true);
            Result.ExceptionClassName = trace.GetFrame(0).GetMethod().ReflectedType.FullName;
            Result.LineNumber = trace.GetFrame(0).GetFileLineNumber();
            Result.ColumnNumber =  trace.GetFrame(0).GetFileColumnNumber();
            return Result;
        }

        public static bool ContainsException(in string JsonString)
        {

            JToken JOut;
            var J = JObject.Parse(JsonString);

            return ((J is JObject) && (J as JObject).TryGetValue("Exception", out JOut)==true);
        }

        public static void Check2(in string JsonString)
        {
            if (ContainsException(JsonString))
            {
              TMiceException Ex = JsonConvert.DeserializeObject<TMiceException>(JsonString);
              if (Ex != null)
               throw new Exception(Ex.ErrorMessage);
            }
        }
        
        public static string CreateExceptionJsonString(System.Exception e)
        {
            return CreateException(e).ToString();
        }

    }
}