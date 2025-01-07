using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace DAC.XDataSet
{

    class TParamUtils
    {
        public static string RemoveEcho(string ParamName)
        {
            if (ParamName.StartsWith("@") == true)
                return ParamName.Substring(1);
            else
                return ParamName;
        }

        public static string ParamToSrting(SqlParameter AParam)
        {
            if (AParam == null || AParam.Value == null)
                return "";
            else
                return "@" + AParam.ParameterName + "=" + AParam.Value.ToString();
        }

        public static string ParamsToSrting(SqlParameterCollection AParams)
        {
            string s = "";
            for (int x = 0; x < AParams.Count; x++)
            {
                if (x == AParams.Count - 1)
                {
                    s = s + ParamToSrting(AParams[x]);
                }
                else
                {
                    s = s + ParamToSrting(AParams[x]) + ", ";
                }
            }
            return s;
        }

    }

}