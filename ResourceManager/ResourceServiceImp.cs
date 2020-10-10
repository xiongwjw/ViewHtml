/********************************************************************
	FileName:   ResourceServiceImp
    purpose:	

	author:		huang wei
	created:	2013/01/15

    revised history:
	2013/01/15  

================================================================
    Copyright (C) 2013, Grgbanking CO,. Ltd. All rights reserved.
================================================================
********************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ResourceManagerProtocol;
using System.Xml;
using LogProcessorService;
using System.Diagnostics;
using System.IO;

namespace ResourceManager
{
    class ResourceServiceImp : IResourceService
    {
#region constructor
        public ResourceServiceImp( string argLanguage )
        {
            Debug.Assert( !string.IsNullOrEmpty(argLanguage) );
            m_language = argLanguage;
        }
#endregion

#region methods of the IResourceService interface
        bool IResourceService.LoadString( string argKey,
                                          string argCategory,
                                          out string argResult)
        {
            argResult = null;
            if ( string.IsNullOrEmpty(argCategory) ||
                 string.IsNullOrEmpty(argKey) )
            {
#if DEBUG
                throw new ArgumentNullException("argCategory or argKey");
#else
                return false;
#endif
            }
            if ( null == m_dicTextCategories )
            {
                Log.ResourceManager.LogWarn("Text resource isn't exists");
                return false;
            }

            XmlDocument doc = null;
            if ( !m_dicTextCategories.TryGetValue( argCategory, out doc ) )
            {
                Log.ResourceManager.LogWarn("Text resource's category isn't exists");
                return false;
            }

            string textPath = string.Format("item[@key='{0}']", argKey);
            XmlNode node = doc.DocumentElement.SelectSingleNode(textPath);
            if ( null == node ||
                 node.NodeType != XmlNodeType.Element )
            {
                Log.ResourceManager.LogWarn(string.Format("Text resource's key[{0}] isn't exists", argKey));
                argResult = argKey;
                return false;
            }

            XmlAttribute valueAttri = node.Attributes[s_valueAttri];
            if ( null == valueAttri )
            {
                argResult = argKey;
                return false;
            }
            else
            {
                argResult = valueAttri.Value;
            }

            return true;
        }

        bool IResourceService.LoadString( string argKey,
                                          out string argResult)
        {
            Debug.Assert(!string.IsNullOrEmpty(argKey));
            argResult = null;
            if ( string.IsNullOrEmpty(argKey) )
            {
#if DEBUG
                throw new ArgumentNullException("argKey");
#else
                return false;
#endif            
            }

            string textPath = string.Format("item[@key='{0}']", argKey);
            XmlNode node = null;
            foreach ( var item in m_dicTextCategories )
            {
                node = item.Value.DocumentElement.SelectSingleNode(textPath);
                if ( null == node ||
                     node.NodeType != XmlNodeType.Element )
                {
                    continue;
                }

                XmlAttribute valueAttri = node.Attributes[s_valueAttri];
                if (null == valueAttri)
                {
                    argResult = argKey;
                    return false;
                }
                else
                {
                    argResult = valueAttri.Value;
                    return true;
                }
            }

            argResult = argKey;
            return false;
        }

        string IResourceService.LoadString( string argKey,
                                            string argCategory)
        {
            string result = null;
            if ( string.IsNullOrEmpty( argCategory ) )
            {
                ((IResourceService)this).LoadString(argKey, out result);
            }
            else
            {
                ((IResourceService)this).LoadString(argKey, argCategory, out result);
            }

            return result;
        }

        bool IResourceService.QueryVoicePath( string argKey,
                                              out string argResult)
        {
            argResult = null;
            if ( string.IsNullOrEmpty(argKey) )
            {
#if DEBUG
                throw new ArgumentNullException("argKey");
#else
                return false;
#endif
            }
            if ( null == m_voiceConfig )
            {
                Log.ResourceManager.LogWarn("voice resource isn't exist");
                return false;
            }

            string voicePath = string.Format("item[@key='{0}']", argKey);
            XmlNode node = m_voiceConfig.DocumentElement.SelectSingleNode(voicePath);
            if ( null == node ||
                 node.NodeType != XmlNodeType.Element )
            {
                Log.ResourceManager.LogWarn("Voice resource's key isn't exists");
                return false;
            }
            
            XmlAttribute valueAttri = node.Attributes[s_valueAttri];
            if ( null == valueAttri )
            {
                return false;
            }
            else
            {
                argResult = string.Format(@"{0}\Voice\{1}\{2}", m_basePath, m_language, valueAttri.Value );
            }

            return true;
        }

        bool IResourceService.QueryImagePath( string argKey,
                                              out string argResult)
        {
            argResult = null;
            if (string.IsNullOrEmpty(argKey))
            {
#if DEBUG
                throw new ArgumentNullException("argKey");
#else
                return false;
#endif
            }
            if (null == m_imageConfig)
            {
                Log.ResourceManager.LogWarn("image resource isn't exist");
                return false;
            }

            string imgPath = string.Format("item[@key='{0}']", argKey);
            XmlNode node = m_imageConfig.DocumentElement.SelectSingleNode(imgPath);
            if (null == node ||
                 node.NodeType != XmlNodeType.Element)
            {
                Log.ResourceManager.LogWarn("image resource's key isn't exists");
                return false;
            }

            XmlAttribute valueAttri = node.Attributes[s_valueAttri];
            if (null == valueAttri)
            {
                return false;
            }
            else
            {
                argResult = string.Format(@"{0}\Image\{1}\{2}", m_basePath, m_language, valueAttri.Value);
            }

            return true;
        }

        bool IResourceService.QueryVideoPath( string argKey,
                                              out string argResult)
        {
            argResult = null;
            if (string.IsNullOrEmpty(argKey))
            {
#if DEBUG
                throw new ArgumentNullException("argKey");
#else
                return false;
#endif
            }
            if (null == m_videoConfig)
            {
                Log.ResourceManager.LogWarn("video resource isn't exist");
                return false;
            }

            string videoPath = string.Format("item[@key='{0}']", argKey);
            XmlNode node = m_videoConfig.DocumentElement.SelectSingleNode(videoPath);
            if (null == node ||
                 node.NodeType != XmlNodeType.Element)
            {
                Log.ResourceManager.LogWarn("video resource's key isn't exists");
                return false;
            }

            XmlAttribute valueAttri = node.Attributes[s_valueAttri];
            if (null == valueAttri)
            {
                return false;
            }
            else
            {
                argResult = string.Format(@"{0}\Video\{1}\{2}", m_basePath, m_language, valueAttri.Value);
            }

            return true;
        }
#endregion

#region property of the IResourceService interface
        string IResourceService.Language
        {
            get
            {
                return m_language;
            }
        }

        private XmlDocument ImageConfig
        {
            get
            {
                if ( null == m_imageConfig )
                {
                    m_imageConfig = new XmlDocument();
                }

                return m_imageConfig;
            }
        }

        private XmlDocument VoiceConfig
        {
            get
            {
                if ( null == m_voiceConfig )
                {
                    m_voiceConfig = new XmlDocument();
                }
                return m_voiceConfig;
            }
        }

        private XmlDocument VideoConfig
        {
            get
            {
                if ( null == m_videoConfig )
                {
                    m_videoConfig = new XmlDocument();
                }

                return m_videoConfig;
            }
        }

        private Dictionary<string, XmlDocument> TextCategories
        {
            get
            {
                if ( null == m_dicTextCategories )
                {
                    m_dicTextCategories = new Dictionary<string, XmlDocument>();
                }

                return m_dicTextCategories;
            }
        }
#endregion

#region method
        public bool Open( XmlNode argNode,
                          string argBasePath,
                          bool   argIsRemote )
        {
            Debug.Assert(null != argNode && !string.IsNullOrEmpty(argBasePath));

            try
            {
                m_basePath = argBasePath;
                m_isRemote = argIsRemote;
                Log.ResourceManager.LogDebugFormat("Prepare for opening a resource service [{0}]", m_language);
                if ( !argIsRemote )
                {
                    XmlAttribute cfgAttri = null;
                    //Load image config
                    XmlNode imgNode = argNode.SelectSingleNode(s_ImageNode);
                    if ( null != imgNode &&
                         imgNode.NodeType == XmlNodeType.Element )
                    {
                        string imgCfg = null;
                        cfgAttri = imgNode.Attributes[s_cfgAttri];
                        if ( null != cfgAttri &&
                             !string.IsNullOrEmpty(cfgAttri.Value) )
                        {
                            imgCfg = string.Format(@"{0}\{1}\{2}", argBasePath, imgNode.Name, cfgAttri.Value);
                            ImageConfig.Load(imgCfg);
                        }
                    }
                    
                    //Load voice config
                    XmlNode voiceNode = argNode.SelectSingleNode(s_VoiceNode);
                    if (null != voiceNode &&
                         voiceNode.NodeType == XmlNodeType.Element)
                    {
                        string voiceCfg = null;
                        cfgAttri = voiceNode.Attributes[s_cfgAttri];
                        if (null != cfgAttri &&
                             !string.IsNullOrEmpty(cfgAttri.Value))
                        {
                            voiceCfg = string.Format(@"{0}\{1}\{2}", argBasePath, voiceNode.Name, cfgAttri.Value);
                            VoiceConfig.Load(voiceCfg);
                        }
                    }

                    //Load video config
                    XmlNode videoNode = argNode.SelectSingleNode(s_VideoNode);
                    if (null != videoNode &&
                         videoNode.NodeType == XmlNodeType.Element)
                    {
                        string videoCfg = null;
                        cfgAttri = videoNode.Attributes[s_cfgAttri];
                        if (null != cfgAttri &&
                             !string.IsNullOrEmpty(cfgAttri.Value))
                        {
                            videoCfg = string.Format(@"{0}\{1}\{2}", argBasePath, videoNode.Name, cfgAttri.Value);
                            VideoConfig.Load(videoCfg);
                        }
                    }

                    //load text content
                    string textNodePath = string.Format("{0}/{1}", s_textNode, s_categoryNode);
                    XmlNodeList listNodes = argNode.SelectNodes(textNodePath);
                    XmlAttribute nameAttri = null;
                    string textFilePath = null;
                    XmlDocument textDoc = null;
                    foreach ( XmlNode node in listNodes )
                    {
                        nameAttri = node.Attributes[s_nameAttri];
                        if ( null == nameAttri ||
                             string.IsNullOrEmpty(nameAttri.Value) )
                        {
                            continue;
                        }

                        try
                        {
                            textFilePath = string.Format(@"{0}\{1}\{2}\{3}.xml", argBasePath, s_textNode, m_language, nameAttri.Value);
                            if ( !File.Exists(textFilePath) )
                            {
                                Log.ResourceManager.LogWarnFormat("The file[{0}] isn't exist", textFilePath);
                                continue;
                            }
                            textDoc = new XmlDocument();
                            textDoc.Load(textFilePath);
                            TextCategories.Add(nameAttri.Value, textDoc);
                        }
                        catch (System.Exception ex)
                        {
                            Log.ResourceManager.LogWarn( string.Format("Failed to open text file[{0}]", textFilePath), ex );	
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                m_imageConfig = null;
                m_voiceConfig = null;
                m_videoConfig = null;
                if ( null != m_dicTextCategories )
                {
                    m_dicTextCategories.Clear();
                    m_dicTextCategories = null;
                }
                Log.ResourceManager.LogError("Failed to open a resource service", ex);
                return false;
            }

            return true;
        }

        public void Close()
        {
            m_imageConfig.RemoveAll();
            m_imageConfig = null;
            m_voiceConfig.RemoveAll();
            m_voiceConfig = null;
            m_videoConfig.RemoveAll();
            m_videoConfig = null;
            if ( null != m_dicTextCategories )
            {
                foreach ( var item in m_dicTextCategories )
                {
                    item.Value.RemoveAll();
                }
                m_dicTextCategories.Clear();
                m_dicTextCategories = null;
            }
        }
#endregion

#region field
        private string m_language = null;

        private string m_basePath = null;

        private bool m_isRemote = false;

        private Dictionary<string, XmlDocument> m_dicTextCategories = null;

        private XmlDocument m_imageConfig = null;

        private XmlDocument m_voiceConfig = null;

        private XmlDocument m_videoConfig = null;

        public const string s_categoryNode = "Category";

        public const string s_nameAttri = "name";

        public const string s_cfgAttri = "cfg";

        public const string s_textNode = "Text";

        public const string s_ImageNode = "Image";

        public const string s_VideoNode = "Video";

        public const string s_VoiceNode = "Voice";

        public const string s_valueAttri = "value";
#endregion
    }
}
