using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace HelperService
{


    
    /// <summary>
    /// ------------------------------------------------------------------------------
    /// Copyright:Copyright (c) 2013,Grgbanking CO,. Ltd. All rights reserved.
    /// description：
    /// version：1.0.0.1
    /// author：liu tengfei (ltfei1@kingpoint.com)
    /// date created：2013年12月13日
    /// modifier：
    /// reason：
    /// ------------------------------------------------------------------------------
    /// </summary>
    /// <typeparam name="T">类参数</typeparam>
    public class ConfigHelper<T> where T : class
    {
        private XmlSerializerHelper<T> xmlHelp;
        public ConfigHelper()
        {
            xmlHelp = new XmlSerializerHelper<T>();
        }
        /// <summary>
        /// 保存CONFIG
        /// </summary>
        /// <param name="xmlPath">xml地址</param>
        /// <param name="t">xml对象</param>
        /// <returns>是否成功</returns>
        public bool SaveConfig(string xmlPath, T t)
        {
            try
            {
                return xmlHelp.Serialize(xmlPath, t);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        /// <summary>
        /// 获得CONFIG
        /// </summary>
        /// <param name="xmlPath">xml地址</param>
        /// <returns>config对象</returns>
        public T GetConfig(string xmlPath)
        {

            try
            {
                return xmlHelp.Deserialize(xmlPath);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}
