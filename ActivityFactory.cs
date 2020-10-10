using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics;
using Attribute4ECAT;
using LogProcessorService;
using eCATActivityTest;

namespace ViewHtml
{
    public class ActivityFactory
    {
        public void InitFactory()
        {
            Assembly objAssembly = Assembly.Load("eCATActivityTest");
            foreach (var objType in objAssembly.GetTypes())
            {
                if (!objType.IsClass ||
                    !objType.IsPublic)
                {
                    continue;
                }

                AddCreateMethodInAssembly(objType);
            }
        }
        private Dictionary<string, MethodInfo> m_dicCreateMethodofActivity = new Dictionary<string, MethodInfo>();
        private void AddCreateMethodInAssembly(Type assemblyType)
        {
            Debug.Assert(null != assemblyType);
            foreach (GrgActivityAttribute objAttri in assemblyType.GetCustomAttributes(typeof(GrgActivityAttribute), false))
            {
                foreach (var objMethod in assemblyType.GetMethods(BindingFlags.Static | BindingFlags.Public))
                {
                    foreach (GrgCreateFunctionAttribute objCreateAttri in objMethod.GetCustomAttributes(typeof(GrgCreateFunctionAttribute), false))
                    {
                        try
                        {
                            m_dicCreateMethodofActivity.Add(objAttri.NodeNameOfConfiguration, objMethod);
                        }
                        catch (System.Exception ex)
                        {
                            Log.BusinessService.LogWarn(string.Format("Failed to add node name[{0}]", objAttri.NodeNameOfConfiguration), ex);
                        }

                        goto _exit;
                    }
                }
            }

            _exit:
            return;
        }

        public List<string> GetActivityList()
        {
            List<string> activityList = new List<string>();

            foreach(var activity in m_dicCreateMethodofActivity)
            {
                activityList.Add(activity.Key.ToString());
            }

            return activityList;
        }

        public  bool CreateActivity(string strName,
                                                    out IBusinessActivity iBusiAct)
        {
            Debug.Assert(!string.IsNullOrEmpty(strName));

            //Log.BusinessService.LogDebugFormat("Prepare for creating a activity with name {0}", strName);

            iBusiAct = null;

            if (!m_dicCreateMethodofActivity.ContainsKey(strName))
            {
                Log.BusinessService.LogWarnFormat("The activity with name [{0}] isn't exist", strName);
                return false;
            }

            try
            {
                IBusinessActivity iActivity = null;
                MethodInfo objInfo = m_dicCreateMethodofActivity[strName];
                iActivity = (IBusinessActivity)objInfo.Invoke(null, null);
                if (null == iActivity)
                {
                    throw new Exception("failed to create a business activity");
                }

                iActivity.Name = strName;

                iBusiAct = iActivity;
            }
            catch (System.Exception ex)
            {
                Log.BusinessService.LogError("Failed to create a activity with error message", ex);
                return false;
            }
            Log.BusinessService.LogDebug("Success to create a activity");

            return true;
        }

    }
}
