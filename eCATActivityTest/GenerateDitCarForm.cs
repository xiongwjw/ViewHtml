using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eCATActivityTest
{
    public class GenerateDitCarForm
    {
        public static void GenerateData(eCATContext context, ref Dictionary<string, string> dicPrintData)
        {
            
            dicPrintData.Add("txtName", context.FormInfo.m_CountPhoneNumbers);
            dicPrintData.Add("txtIDCardNumber", context.FormInfo.m_CountPhoneNumbers);
        }
    }
}
