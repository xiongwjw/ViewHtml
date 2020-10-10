using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;

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
    public class XmlSerializerHelper<T> where T : class
    {
        private XmlSerializer xmlSer = null;
        private FileStream fs = null;
        public XmlSerializerHelper()
        {
            xmlSer = new XmlSerializer(typeof(T));
        }

        public bool Serialize(string xmlPath, T t)
        {
            try
            {
                fs = new FileStream(xmlPath, FileMode.Create);
                xmlSer.Serialize(fs, t); 
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {

                if (null != fs)
                {
                    fs.Close();
                    fs = null;
                }
            }
        }
        public T Deserialize(string xmlPath)
        {
            try
            {
                fs = new FileStream(xmlPath, FileMode.Open);
                T t = (T)xmlSer.Deserialize(fs);
                return t;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            { 
                if (null != fs)
                {
                    fs.Close();
                    fs = null;
                }
            }
        }
    }
}
