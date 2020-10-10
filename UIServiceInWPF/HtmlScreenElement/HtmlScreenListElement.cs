/********************************************************************
	FileName:   HtmlScreenListElement
    purpose:	

	author:		huang wei
	created:	2013/03/29

    revised history:
	2013/03/29  

================================================================
    Copyright (C) 2013, Grgbanking CO,. Ltd. All rights reserved.
================================================================
********************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIServiceInWPF;
using UIServiceInWPF.screen;
using System.Diagnostics;
using LogProcessorService;
//using mshtml;
using System.Reflection;
using UIServiceInWPF.Resource;
using System.Windows.Forms;
using UIServiceProtocol;
using mshtml;
using System.ComponentModel;

namespace UIServiceInWPF.HtmlScreenElement
{
    class HtmlScreenListElement : HtmlScreenElementBase
    {
#region constructor
        public HtmlScreenListElement( System.Windows.Forms.HtmlElement argElement,
                                      HtmlRender argRender)
            : base(argElement, argRender)
        {

        }
#endregion

#region method
        public override bool Open()
        {
            Debug.Assert(null != m_ihostedElement && null != m_parent);

            //Load template attribute
            string templateName = m_ihostedElement.GetAttribute(UIPropertyKey.s_itemTemplateKey);
            if ( string.IsNullOrEmpty(templateName) )
            {
                Log.UIService.LogWarn("The list element must be have a itemTemplate attribute");
                return false;
            }
            ResourceItem resItem = m_parent.FindResourceInHtml(templateName);
            if ( null == resItem ||
                 !(resItem is ResourceDataTemplate) )
            {
                Log.UIService.LogWarnFormat("Failed to find a datatemplate[{0}]", templateName);
                return false;
            }
            m_dataTemplate = (ResourceDataTemplate)resItem;
            //Load visible item count
            string visibleCount = m_ihostedElement.GetAttribute(UIPropertyKey.s_visibleItemCountKey);
            if ( string.IsNullOrEmpty(visibleCount) )
            {
                Log.UIService.LogWarn("The list element need to have a visibleItemCount attribute");
                return false;
            }
            if ( !int.TryParse( visibleCount, out m_visibleCount ) ||
                 m_visibleCount <= 0 )
            {
                Log.UIService.LogWarn("The visibleItemCount's value isn't valid");
                return false;
            }

            try
            {
                //prepare for creating rows
                ListItemElement rowElement = null;
                HtmlElement trElement = null;
                for (int i = 0; i < m_visibleCount; ++i)
                {
                    trElement = m_ihostedElement.Document.CreateElement("tr");
                    if ( null != trElement )
                    {
                        rowElement = new ListItemElement( trElement );
                        if ( rowElement.Initialize( m_dataTemplate ) )
                        {
                            HtmlElement firstChildElement = m_ihostedElement.FirstChild;
                            if (firstChildElement != null)
                            {
                                firstChildElement.AppendChild(trElement);
                            }
                            else
                            {
                                var m_htmlElementCol = m_ihostedElement.GetElementsByTagName("tbody");
                                foreach (HtmlElement ele in m_htmlElementCol)
                                {
                                    if (ele.CanHaveChildren)
                                    {
                                        ele.AppendChild(trElement);
                                    }
                                }
                            }
                            m_listRows.Add(rowElement);
                        }
                        else
                        {
                            rowElement.Dispose();
                            rowElement = null;
                        }
                        trElement = null;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Log.UIService.LogError("Failed to open a list element",ex);
                return false;
            }

            return base.Open();
        }

        public override bool GetPropertyValue(string argProperty,
                                              out object argValue)
        {
            Debug.Assert(!string.IsNullOrEmpty(argProperty));

            switch ( argProperty )
            {
                case UIPropertyKey.s_currentPageIndexKey:
                    {
                        argValue = m_currentPageIndex;
                    }
                    break;

                case UIPropertyKey.s_totalPageCountKey:
                    {
                        argValue = m_totalPageCount;
                    }
                    break;

                case UIPropertyKey.s_canFirstPageKey:
                    {
                        argValue = m_firstPage;
                    }
                    break;

                case UIPropertyKey.s_canLastPageKey:
                    {
                        argValue = m_lastPage;
                    }
                    break;

                case UIPropertyKey.s_canBackwardKey:
                    {
                        argValue = m_canBackward;
                    }
                    break;

                case UIPropertyKey.s_canForwardKey:
                    {
                        argValue = m_canForward;
                    }
                    break;

                default:
                    {
                        return base.GetPropertyValue(argProperty, out argValue);
                    }
            }

            return true;
        }

        public override void UpdateBindingData(bool argSave)
        {
            base.UpdateBindingData(argSave);

            foreach ( var item in m_listRows )
            {
                item.UpdateBindingValue(argSave);
            }
        }

        public override bool SetPropertyValue(string argProperty, 
                                              object argValue)
        {
            Debug.Assert(!string.IsNullOrEmpty(argProperty));
            switch ( argProperty )
            {
                case UIPropertyKey.s_ItemSourceKey:
                    {
                        if ( null == argValue ||
                             !(argValue is UIListItemCollection) )
                        {
                            return false;
                        }

                        m_itemSource = (UIListItemCollection)argValue;
                        if ( m_itemSource.Items.Count == 0 )
                        {
                            m_itemSource = null;
                            foreach ( var item in m_listRows )
                            {
                                item.DataObject = null;
                            }

                            m_totalPageCount = 1;
                            m_currentPageIndex = 1;
                        }
                        else
                        {
                            m_totalPageCount = (m_itemSource.Items.Count + m_visibleCount - 1) / m_visibleCount;
                            m_parent.NotifyPropertyChanged(m_ihostedElement.Id, UIPropertyKey.s_totalPageCountKey, m_totalPageCount.ToString());
                            m_currentPageIndex = 0;
                            ChangeIndex( 1 );
                            UpdateBindingExpress(UIPropertyKey.s_totalPageCountKey, true);
                        }
                    }
                    break;

                case UIPropertyKey.s_currentPageIndexKey:
                    {
                        if ( null == argValue )
                        {
                            return false;
                        }

                        if ( argValue is int )
                        {
                            int index = (int)argValue;
                            return ChangeIndex( index );
                        }
                        else if ( argValue is string )
                        {
                            string index = (string)argValue;
                            index.Trim();
                            int currIndex = m_currentPageIndex;
                            if (string.Equals(index, UIPropertyKey.s_addValueKey, StringComparison.Ordinal))
                            {
                                currIndex += 1;
                                return ChangeIndex(currIndex);
                            }
                            else if ( string.Equals( index, UIPropertyKey.s_removeValueKey, StringComparison.Ordinal ) )
                            {
                                currIndex -= 1;
                                return ChangeIndex(currIndex);
                            }
                            else if ( string.Equals( index, UIPropertyKey.s_firstPageKey, StringComparison.Ordinal ) )
                            {
                                currIndex = 1;
                                return ChangeIndex(currIndex);
                            }
                            else if ( string.Equals( index, UIPropertyKey.s_lastPageKey, StringComparison.Ordinal ) )
                            {
                                currIndex = m_totalPageCount;
                                return ChangeIndex(currIndex);
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                    break;

                default:
                    {
                        return base.SetPropertyValue(argProperty, argValue);
                    }
            }

            return true;
        }

        public override void InnerDispose()
        {
            base.InnerDispose();

            //
            foreach ( var item in m_listRows )
            {
                if ( null != item.DataObject )
                {
                    ((UIListItem)item.DataObject).PropertyChanged -= OnPropertyChanged;
                }
                item.Dispose();
            }
            m_listRows.Clear();
            m_listRows = null;

            //m_dataTemplate.Close();
            //m_dataTemplate = null;
          
            m_visibleCount = 0;
            m_currentPageIndex = 1;
            m_totalPageCount = 1;

            m_itemSource = null;
        }

        public override void ParseBindingExpress()
        {
            base.ParseBindingExpress();

            //check itemSource property
            BindingMode mode = BindingMode.OneWay;
            string propValue = ParseBindedProperty(UIPropertyKey.s_ItemSourceKey, m_ihostedElement,ref mode);
            if (!string.IsNullOrEmpty(propValue))
            {
                m_dicBindingExpress.Add(propValue, new BindingExpress(propValue, UIPropertyKey.s_ItemSourceKey, this)
                    {
                        Mode = mode
                    });
            }

            propValue = ParseBindedProperty(UIPropertyKey.s_currentPageIndexKey, m_ihostedElement, ref mode);
            if (!string.IsNullOrEmpty(propValue))
            {
                m_dicBindingExpress.Add(propValue, new BindingExpress(propValue, UIPropertyKey.s_currentPageIndexKey, this)
                {
                    Mode = mode
                });
            }

            propValue = ParseBindedProperty(UIPropertyKey.s_totalPageCountKey, m_ihostedElement, ref mode);
            if (!string.IsNullOrEmpty(propValue))
            {
                m_dicBindingExpress.Add(propValue, new BindingExpress(propValue, UIPropertyKey.s_totalPageCountKey, this)
                {
                    Mode = mode
                });
            }

            propValue = ParseBindedProperty(UIPropertyKey.s_canFirstPageKey, m_ihostedElement, ref mode);
            if (!string.IsNullOrEmpty(propValue))
            {
                m_dicBindingExpress.Add(propValue, new BindingExpress(propValue, UIPropertyKey.s_canFirstPageKey, this)
                {
                    Mode = mode
                });
            }

            propValue = ParseBindedProperty(UIPropertyKey.s_canLastPageKey, m_ihostedElement, ref mode);
            if (!string.IsNullOrEmpty(propValue))
            {
                m_dicBindingExpress.Add(propValue, new BindingExpress(propValue, UIPropertyKey.s_canLastPageKey, this)
                {
                    Mode = mode
                });
            }

            propValue = ParseBindedProperty(UIPropertyKey.s_canBackwardKey, m_ihostedElement, ref mode);
            if (!string.IsNullOrEmpty(propValue))
            {
                m_dicBindingExpress.Add(propValue, new BindingExpress(propValue, UIPropertyKey.s_canBackwardKey, this)
                {
                    Mode = mode
                });
            }

            propValue = ParseBindedProperty(UIPropertyKey.s_canForwardKey, m_ihostedElement, ref mode);
            if (!string.IsNullOrEmpty(propValue))
            {
                m_dicBindingExpress.Add(propValue, new BindingExpress(propValue, UIPropertyKey.s_canForwardKey, this)
                {
                    Mode = mode
                });
            }
        }

        private bool ChangeIndex( int argIndex )
        {
            if (argIndex < 1 ||
                argIndex > m_totalPageCount)
            {
                return false;
            }
            if (m_currentPageIndex == argIndex)
            {
                return true;
            }

            m_currentPageIndex = argIndex;

            UpdateBindData();
            UpdateNavStatus();

            m_parent.NotifyPropertyChanged(m_ihostedElement.Id, UIPropertyKey.s_currentPageIndexKey, m_currentPageIndex.ToString());

            return true;
        }

        public override HtmlScreenElementBase FindHtmlScreenElement(HtmlElement argElement)
        {
            Debug.Assert( null != argElement );
            HtmlScreenElementBase element = null;
            foreach ( var item in m_listRows )
            {
                element = item.FindHtmlScreenElement(argElement);
                if ( null != element )
                {
                    return element;
                }
            }

            return null;
        }

        private void UpdateBindData()
        {
            int index = (m_currentPageIndex - 1) * m_visibleCount;
            int endIndex = m_itemSource.Items.Count;
            int visibleIndex = 0;
            //
            foreach ( var item in m_listRows )
            {
                if ( null != item.DataObject )
                {
                    ((UIListItem)item.DataObject).PropertyChanged -= OnPropertyChanged;
                }
            }

            for (int i = index; i < endIndex; ++i,++visibleIndex)
            {
                if ( visibleIndex >= m_visibleCount )
                {
                    break;
                }

                m_listRows[visibleIndex].DataObject = m_itemSource.Items[i];
                m_listRows[visibleIndex].Visible = true;
                m_itemSource.Items[i].PropertyChanged += OnPropertyChanged;
            }

            if (visibleIndex < m_visibleCount)
            {
                for (int i = visibleIndex; i < m_visibleCount; ++i)
                {
                    m_listRows[i].Visible = false;
                    m_listRows[i].DataObject = null;                   
                }
            }
        }

        private void UpdateNavStatus()
        {
            if ( 1 < m_currentPageIndex &&
                 m_currentPageIndex < m_totalPageCount )
            {
                m_firstPage = true;
                m_lastPage = true;
                m_canBackward = true;
                m_canForward = true;
            }
            else if ( m_totalPageCount == 1 )
            {
                m_canBackward = false;
                m_canForward = false;
                m_firstPage = false;
                m_lastPage = false;
            }
            else if (m_currentPageIndex == m_totalPageCount)
            {
                m_lastPage = false;
                m_firstPage = true;
                m_canBackward = true;
                m_canForward = false;
            }
            else if ( m_currentPageIndex == 1 )
            {
                m_firstPage = false;
                m_lastPage = true;
                m_canForward = true;
                m_canBackward = false;
            }

            UpdateBindingExpress(UIPropertyKey.s_canFirstPageKey, true);
            UpdateBindingExpress(UIPropertyKey.s_canLastPageKey, true);
            UpdateBindingExpress(UIPropertyKey.s_canBackwardKey, true);
            UpdateBindingExpress(UIPropertyKey.s_canForwardKey, true);
        }

        private void OnPropertyChanged( object argSender,
                                        PropertyChangedEventArgs argEvt)
        {
            if ( m_listRows.Count == 0 ||
                 string.IsNullOrEmpty( argEvt.PropertyName ) )
            {
                return;
            }

            HtmlRender.SingleInstance.Browser.BeginInvoke(m_propChangedHandler,
                                                           argSender,
                                                           argEvt.PropertyName);
        }

        private void OnPropDataChanged( object argSender,
                                        string argName )
        {
            if ( m_listRows.Count == 0 )
            {
                return;
            }

            foreach ( var item in m_listRows )
            {
                if ( item.DataObject == argSender )
                {
                    item.OnDataChanged(argName);
                    break;
                }
            }
        }
#endregion

#region field
        protected List<ListItemElement> m_listRows = new List<ListItemElement>();

        protected ResourceDataTemplate m_dataTemplate = null;

        protected UIListItemCollection m_itemSource = null;

        protected Action<object, string> m_propChangedHandler = null;

        protected int m_visibleCount = 0;

        protected int m_currentPageIndex = 1;

        protected int m_totalPageCount = 1;

        protected bool m_firstPage = false;

        protected bool m_canForward = false;

        protected bool m_canBackward = false;

        protected bool m_lastPage = false;
#endregion
    }

    class ListItemElement : IDisposable
    {
#region constructor
        public ListItemElement( HtmlElement argRow )
        {
            Debug.Assert(null != argRow);
            m_tableRow = argRow;
        }
#endregion

#region method
        public void Dispose()
        {
            m_dataObject = null;
            //m_getMethodInfo = null;
            m_tableRow = null;

            foreach ( var item in m_listCells )
            {
                item.Dispose();
            }
            m_listCells.Clear();
            m_listCells = null;
        }

        public void UpdateBindingValue( bool argSave )
        {
            foreach ( var item in m_listCells )
            {
                item.UpdateBindingValue(argSave);
            }
        }

        public bool Initialize( ResourceDataTemplate argTemplate )
        {
            Debug.Assert(null != argTemplate && null != m_tableRow );

            try
            {
                HtmlElement tdElement = null;
                ListItemCellElement cellElement = null;
                foreach ( var item in argTemplate.TDItems )
                {
                    tdElement = m_tableRow.Document.CreateElement("td");
                    if ( null == tdElement )
                    {
                        continue;
                    }
                    cellElement = new ListItemCellElement(tdElement);
                    if ( cellElement.Open( item.ClassName,
                                           item.StyleValue,
                                           item.InnerHtml ) )
                    {
                        m_listCells.Add(cellElement);
                        m_tableRow.AppendChild(tdElement);
                    }
                    else
                    {
                        cellElement.Dispose();
                    }

                    tdElement = null;
                    cellElement = null;
                }
            }
            catch (System.Exception ex)
            {
                Log.UIService.LogError("Failed to initlialize table row element", ex);
                return false;
            }

            return true;
        }

        public void OnDataChanged( string argName )
        {
            Debug.Assert(null != argName);
            if ( m_listCells.Count == 0 )
            {
                return;
            }

            foreach ( var item in m_listCells )
            {
                item.OnDataChanged(argName);
            }
        }

        public HtmlScreenElementBase FindHtmlScreenElement( HtmlElement argElement )
        {
            Debug.Assert(null != argElement);
            HtmlScreenElementBase element = null;
            foreach ( var item in m_listCells )
            {
                element = item.FindHtmlScreenElement(argElement);
                if ( null != element )
                {
                    return element;
                }
            }

            return null;
        }

        //private void UpdateBindingValue()
        //{
        //    if ( null == m_dataObject )
        //    {
        //        //m_getMethodInfo = null;
        //        //foreach ( var item in m_listCells )
        //        //{
        //        //    //item.SetBindValue(null, null);
        //        //}
        //    }
        //    else
        //    {
        //        //m_getMethodInfo = m_dataObject.GetType().GetMethod("GetBindData", BindingFlags.Public | BindingFlags.Instance);
        //        //if ( null != m_getMethodInfo )
        //        //{
        //        //    foreach ( var item in m_listCells )
        //        //    {
        //        //       // item.SetBindValue( m_getMethodInfo, m_dataObject );
        //        //    }
        //        //}
        //        //else
        //        //{
        //        //    m_dataObject = null;
        //        //    foreach ( var item in m_listCells )
        //        //    {
        //        //        //item.SetBindValue( null, null );
        //        //    }
        //        //}
        //    }
       // }
#endregion

#region property
        public object DataObject
        {
            get
            {
                return m_dataObject;
            }
            set
            {
                if ( m_dataObject != value )
                {
                    m_dataObject = value;

                    foreach ( var item in m_listCells )
                    {
                        item.DataContext = m_dataObject;
                    }
                    //UpdateBindingValue();
                }
            }
        }

        public bool Visible
        {
            get
            {
                return m_visible;
            }
            set
            {
                if ( null == m_tableRow )
                {
                    return;
                }

                if ( m_visible != value )
                {
                    m_visible = value;
                    try
                    {
                        IHTMLElement element = (IHTMLElement)m_tableRow.DomElement;
                        IHTMLStyle iStyle = element.style;
                        if ( !m_visible )
                        {
                            if (null != iStyle)
                            {
                                if (!string.Equals(iStyle.display, "none", StringComparison.OrdinalIgnoreCase))
                                {
                                    iStyle.display = "none";
                                }

                                iStyle = null;
                            }
                            else
                            {
                                m_tableRow.SetAttribute("style", "display:none");
                            }
                        }
                        else
                        {
                            if (null != iStyle)
                            {
                                iStyle.display = string.Empty;
                            }
                        }

                        element = null;
                        iStyle = null;
                    }
                    catch (System.Exception ex)
                    {
                        Trace.TraceError(ex.Message);
                    }
                }
            }
        }
#endregion

#region field
        protected object m_dataObject = null;

        protected HtmlElement m_tableRow = null;

        protected bool m_visible = true;

        //protected MethodInfo m_getMethodInfo = null;

        protected List<ListItemCellElement> m_listCells = new List<ListItemCellElement>();
#endregion
    }

    class ListItemCellElement : IDisposable
    {
#region constructor
        public ListItemCellElement( HtmlElement argCell )
        {
            Debug.Assert(null != argCell);
            m_cell = argCell;
        }
#endregion

#region method
        public void UpdateBindingValue( bool argSave )
        {
            foreach ( var item in m_dicElements )
            {
                if ( (item.Value is HtmlScreenInputElement) ||
                     (item.Value is HtmlScreenSelectElement) ||
                     (item.Value is HtmlScreenButtonElement) ||
                     (item.Value is HtmlScreenListElement) )
                {
                    item.Value.UpdateBindingData(argSave);
                }
            }
        }

        public HtmlScreenElementBase FindHtmlScreenElement( HtmlElement argElement )
        {
            Debug.Assert(null != argElement);
            HtmlScreenElementBase element = null;
            foreach ( var item in m_dicElements )
            {
                element = item.Value.FindHtmlScreenElement(argElement);
                if ( null != element )
                {
                    return element;
                }
            }

            return null;
        }

        public void Dispose()
        {
            m_cell = null;
            //m_bindValue = null;
            //m_arrParams = null;
            if ( null != m_dicElements )
            {
                foreach ( var item in m_dicElements )
                {
                    item.Value.Dispose();
                }
                m_dicElements.Clear();
                m_dicElements = null;
            }

            m_dataContext = null;
        }

        //public bool Open( string argClass,
        //                  string argStyle,
        //                  string argBind )
        //{
        //    Debug.Assert(!string.IsNullOrEmpty(argBind) && null != m_cell );

        //    try
        //    {
        //        IHTMLElement iElement = (IHTMLElement)m_cell.DomElement;
        //        if (!string.IsNullOrEmpty(argClass))
        //        {
        //            iElement.className = argClass;
        //        }
        //        if (!string.IsNullOrEmpty(argStyle))
        //        {
        //            iElement.style.cssText = argStyle;
        //        }
        //        iElement = null;
        //        m_bindValue = argBind;

        //        m_arrParams = new object[] { m_bindValue };
        //    }
        //    catch (System.Exception ex)
        //    {
        //        Log.UIService.LogError("Failed to open cell item", ex);
        //        return false;	
        //    }

        //    return true;
        //}

        public bool Open( string argClass,
                          string argStyle,
                          string argInnerHtml )
        {
            Debug.Assert(null != m_cell);

            try
            {
                IHTMLElement iElement = (IHTMLElement)m_cell.DomElement;
                if (!string.IsNullOrEmpty(argClass))
                {
                    iElement.className = argClass;
                }
                if (!string.IsNullOrEmpty(argStyle))
                {
                    iElement.style.cssText = argStyle;
                }
                iElement = null;

                if (!string.IsNullOrEmpty(argInnerHtml))
                {
                    m_cell.InnerHtml = argInnerHtml;
                    HtmlRender.SingleInstance.ExtractAllElements(m_cell, m_dicElements);
                }
            }
            catch (System.Exception ex)
            {
                Log.UIService.LogError("Failed to open cell item", ex);
                return false;	
            }

            return true;
        }

        public void OnDataChanged( string argName )
        {
            Debug.Assert(!string.IsNullOrEmpty(argName));

            if ( m_dicElements.Count == 0 )
            {
                return;
            }

            bool result = false;
            foreach ( var item in m_dicElements )
            {
                result = item.Value.OnPropertyChanged( argName );
                if ( result )
                {
                    break;
                }
            }
        }
           
        //public void SetBindValue( MethodInfo argGetMethod,
        //                          object argDataObject ) 
        //{
        //    Debug.Assert(null != m_cell);

        //    try
        //    {
        //        if ( null == argGetMethod ||
        //             null == argDataObject)
        //        {
        //            m_cell.InnerHtml = "";
        //        }
        //        else
        //        {
        //            string value = (string)argGetMethod.Invoke(argDataObject, m_arrParams);
        //            if ( string.IsNullOrEmpty( value ) )
        //            {
        //                m_cell.InnerHtml = "";
        //            }
        //            else
        //            {
        //                m_cell.InnerHtml = value;
        //            }
        //        }
        //    }
        //    catch (System.Exception ex)
        //    {
        //        Log.UIService.LogError("Failed to set table cell's value", ex);
        //    }

        //}
#endregion

#region property
        //public string BindValue
        //{
        //    get
        //    {
        //        return m_bindValue;
        //    }
        //}

        public object DataContext
        {
            get
            {
                return m_dataContext;
            }
            set 
            {
                m_dataContext = value;
                foreach ( var item in m_dicElements )
                {
                    item.Value.SetBindingTarget(value);
                }
            }
        }
#endregion

#region field
        protected HtmlElement m_cell = null;

        protected object m_dataContext = null;

        //protected string m_bindValue = null;

        //protected object[] m_arrParams = null;

        protected Dictionary<string, HtmlScreenElementBase> m_dicElements = new Dictionary<string, HtmlScreenElementBase>();
#endregion
    }
}
