using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace UIServiceInWPF
{
    [ComVisible(true)]
    public class JavaScriptObject
    {
#region constructor
        public JavaScriptObject( HtmlRender argRender )
        {
            Debug.Assert(null != argRender);
            m_render = argRender;
        }
#endregion

#region method
        public bool SetData( string argKey,
                             object argValue )
        {
            Debug.Assert(null != m_render);
            return m_render.SetBindedData( argKey, argValue );
        }

        public object GetData( string argKey )
        {
            Debug.Assert(null != m_render);
            return m_render.GetBindedData(argKey);
        }
#endregion

#region field
        private HtmlRender m_render = null;
#endregion
    }
}
