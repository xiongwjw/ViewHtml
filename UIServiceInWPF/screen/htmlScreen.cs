using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIServiceInWPF;
using UIServiceProtocol;
using System.Diagnostics;

namespace UIServiceInWPF.screen
{
    public class htmlScreen : Screen
    {
#region constructor
        public htmlScreen()
        {
        }
#endregion

#region override methods
        public override bool GetPropertyValue(string strElement, string strProperty, out object objValue)
        {
            return HtmlRender.SingleInstance.GetPropertyValueOfElement(strElement, strProperty, out objValue);
        }

        public override bool SetPropertyValue(string strElement, string strProperty, object objValue)
        {
            return HtmlRender.SingleInstance.SetPropertyValueOfElement(strElement, strProperty, objValue);
        }

        public override object ExecuteCommand(UIServiceProtocol.UICommandType type, string name, params object[] args)
        {
            if ( type == UICommandType.Script )
            {
                return HtmlRender.SingleInstance.ExecuteScriptCommand(name, args);
            }
            else
            {
                return HtmlRender.SingleInstance.ExecuteCustomCommand(name, args);
            }
        }

        public override void EnumElements( UIServiceProtocol.EnumElementHandler Handler, 
                                           UIServiceProtocol.ElementType Type, 
                                           UIServiceProtocol.EnumFlag Flag, 
                                           string NameOfElement, 
                                           object Param )
        {
            HtmlRender.SingleInstance.EnumElements(Handler, Type, Flag, NameOfElement, Param);
        }

        public override bool ShowScreen()
        {
            //if (!m_bIsRendered)
            //{
            //    string htmlPath = Path + Url;
            //    m_bIsRendered = true;
            //    return HtmlRender.SingleInstance.Render(htmlPath);
            //}
            HtmlRender.SingleInstance.OwnerScreen = this;

            if ( string.IsNullOrEmpty(m_fullPath) )
            {
                return HtmlRender.SingleInstance.Render(Path + Url);
            }
            else
            {
                return HtmlRender.SingleInstance.Render(m_fullPath);
            }           
        }

        public override void Present()
        {
            HtmlRender.SingleInstance.Present();
            base.Present();
        }

        public override void ChangeLanguage(string argLanguage)
        {
            //HtmlRender.SingleInstance.ChangeLanguage(argLanguage);
            base.ChangeLanguage(argLanguage);
        }

        public override void Redraw()
        {
            //Reset();
            HtmlRender.SingleInstance.Render(Path + Url);
        }

        public override void clear()
        {
            base.clear();
            m_fullPath = null;
        }
        #endregion

        public bool HandleLButtonDown(System.Drawing.Point argPoint)
        {
            return false;
        }
        public override void SetDatacontext(object obj)
        {
            HtmlRender.SingleInstance.SetDataContext(obj);
        }

        public bool HandleLButtonUp( System.Drawing.Point argPoint )
        {
            if ( m_handleHotArea )
            {
                foreach (var item in m_app.HotAreas)
                {
                    if (item.HitTest(argPoint.X,
                                     argPoint.Y))
                    {
                        m_app.HandleHotArea(item);
                        return true;
                    }
                }

                return true;
            }
            else
            {
                return false;
            }
        }

#region override property
        public override System.Windows.Controls.Panel Content
        {
            get 
            {                
                //return HtmlRender.SingleInstance.HtmlRenderPage;
                return null;
            }
            set
            {

            }
        }

        public bool HandleHotArea
        {
            get
            {
                return m_handleHotArea;
            }
            set
            {
                m_handleHotArea = value;
            }
        }
#endregion

#region field
        protected bool m_handleHotArea = false;
#endregion
    }
}
