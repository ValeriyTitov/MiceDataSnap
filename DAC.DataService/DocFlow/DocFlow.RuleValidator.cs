using DAC.XDataSet;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAC.DataService.DocFlow
{
    class TDocFlowRuleValidator
    {
        public static string CreateCheckSQL(DataTable Rules, string MainView, string KeyField, int DocumentsId)
        {
            StringBuilder Builder = new StringBuilder();
            Builder.AppendLine("WITH Violations AS (");
            string s, s1;
            for (int x = 0; x < Rules.Rows.Count; x++)
            {
                DataRow Row = Rules.Rows[x];
                s = Row["UserMessage"].ToString();
                s.Replace("'", "");
                s1 = Row["Caption"].ToString();
                s1.Replace("'", "");
                if (s1 == "" || s == "")
                    throw new Exception("Docflow Engine: Invalid rule format");
                s = "'" + s + "'";
                s1 = "'" + s1 + "'";

                Builder.AppendLine("SELECT " + s1 + " AS [Caption], " + s + " AS [UserMessage], 227 AS [ImageIndex]"); //Red imageindex: 150, 175, 177, 227, 228, 230
                Builder.AppendLine("FROM " + MainView + " WITH (NOLOCK)");
                Builder.AppendLine("WHERE " + KeyField + " = " + DocumentsId);
                Builder.AppendLine("AND " + Row["Expression"]);
                if (x < Rules.Rows.Count - 1)
                    Builder.AppendLine("UNION");
            }
            Builder.Append(")");
            Builder.Append("SELECT * FROM Violations");
            return Builder.ToString();
        }
     
    }
}
