using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eCATActivityTest
{
    public class eCatBankInterface
    {
        public eCATContext context = null;

        public virtual emRetCode AfterPackRestMessage(string argType, ref string argRequest)
        {
            
                
            return emRetCode.Default;
        }

        public virtual emRetCode AfterUnpackRestMessage(string argType, string argResponseCode, string argResponse)
        {
            return emRetCode.Default;
        }

        public virtual emRetCode BeforePackRestMessage(string argType)
        {
            context.FaceUserInformation.FullName = "test";
            object obj = context.GetBindData("test");
            context.TransactionDataCache.Set("test", "test", GetType());
            return emRetCode.Default;
        }

        public virtual emRetCode BeforeUnpackRestMessage(string argType, ref string argResponse)
        {
            context.CardHolderDataCache.Set("test", context.FaceUserInformation.FullName, GetType());
            return emRetCode.Default;
        }

    }
}
