using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using LogProcessorService;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls;

namespace UIServiceInWPF.Factory
{
    class WPFElementFactory
    {
#region constructor
        protected WPFElementFactory()
        {

        }

        static WPFElementFactory()
        {
            s_factory = new WPFElementFactory();
        }
#endregion

#region method
        public TextBlock CreateTextBlock( string argContent,
                                          string argFontFamily,
                                          int    argFontSize,
                                          string argFg,
                                          string argBg )
        {
            FontFamily family = QueryFontFamily(argFontFamily);
            if ( null == family )
            {
                return null;
            }
            Brush fgBrush = Brushes.Transparent;
            if ( !string.IsNullOrEmpty(argFg) )
            {
                fgBrush = QueryBrush(argFg);
            }
            Brush bgBrush = Brushes.Transparent;
            if ( !string.IsNullOrEmpty(argBg) )
            {
                bgBrush = QueryBrush(argBg);
            }

            TextBlock txt = null;
            if ( m_listTextBlocks.Count > 0 )
            {
                txt = m_listTextBlocks[0];
                m_listTextBlocks.RemoveAt(0);   
            }
            else
            {
                txt = new TextBlock();
               // m_listTextBlocks.Add(txt);
            }

            txt.Text = argContent;
            txt.FontFamily = family;
            txt.FontSize = argFontSize;
            txt.Foreground = fgBrush;
            txt.Background = bgBrush;

            return txt; 
        }

        public Image CreateImage( string argSource )
        {
            if ( string.IsNullOrEmpty(argSource) )
            {
                return null;
            }

            ImageSource src = QueryImageSource(argSource);
            if ( null == src )
            {
                return null;
            }

            Image img = null;
            if ( m_listImages.Count > 0 )
            {
                img = FindImageWithSource(src);
                if (null == img)
                {
                    img = m_listImages[0];
                    img.Source = src;
                    m_listImages.RemoveAt(0);
                }
                //img = m_listImages[0];
                //m_listImages.RemoveAt(0);

                return img;
            }
            else
            {
                img = new Image();
                img.Source = src;
            }
//            img.Source = src;

            return img;
        }

        private Image FindImageWithSource( ImageSource argSource )
        {
            int total = m_listImages.Count;
            Image img = null;
            for (int i = 0; i < total; ++i)
            {
                img = m_listImages[i];
                if (img.Source == argSource)
                {
                    m_listImages.RemoveAt(i);
                    return img;
                }
            }

            return null;
        }

        public ImageSource QueryImageSource( string argSource ) 
        {
            Debug.Assert(!string.IsNullOrEmpty(argSource));

            if ( m_dicImages.ContainsKey( argSource ) )
            {
                return m_dicImages[argSource];
            }
            else
            {
                try
                {
                    BitmapImage bmp = new BitmapImage();

                    bmp.BeginInit();
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.UriSource = new Uri(argSource, UriKind.Absolute);
                    bmp.EndInit();
                    bmp.Freeze();

                    m_dicImages.Add(argSource, bmp);

                    return bmp;
                }
                catch (System.Exception ex)
                {
                    Log.UIService.LogError(ex.Message, ex); 
                    return null;
                }
            }
        }

        public FontFamily QueryFontFamily(string argFontFamily)
        {
            Debug.Assert(!string.IsNullOrEmpty(argFontFamily));

            if ( m_dicFontFamilies.ContainsKey(argFontFamily) )
            {
                return m_dicFontFamilies[argFontFamily];
            }
            else
            {
                try
                {
                    FontFamily family = new FontFamily(argFontFamily);
                    m_dicFontFamilies.Add(argFontFamily, family);

                    return family;
                }
                catch (System.Exception ex)
                {
                    Log.UIService.LogError(ex.Message, ex); 
                    return null;
                }
            }
        }

        public Brush QueryBrush( string argBrush )
        {
            Debug.Assert(!string.IsNullOrEmpty(argBrush));

            if ( m_dicBrushes.ContainsKey( argBrush ) )
            {
                return m_dicBrushes[argBrush];
            }
            else
            {
                try
                {
                    SolidColorBrush brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(argBrush));
                    m_dicBrushes.Add(argBrush, brush);

                    return brush;
                }
                catch (System.Exception ex)
                {
                    Log.UIService.LogError(ex.Message, ex); 
                	return null;
                }
            }
        }

        public void ReclaimObject( object argobj )
        {
            Debug.Assert(null != argobj);

            if ( argobj is Image )
            {
                m_listImages.Add((Image)argobj);
            }
            else if ( argobj is TextBlock )
            {
                m_listTextBlocks.Add((TextBlock)argobj);
            }
        }
#endregion

#region property
        public static WPFElementFactory Instance
        {
            get
            {
                return s_factory;
            }
        }
#endregion

#region field
        private static WPFElementFactory s_factory;

        private Dictionary<string, ImageSource> m_dicImages = new Dictionary<string, ImageSource>();

        private Dictionary<string, FontFamily> m_dicFontFamilies = new Dictionary<string, FontFamily>();

        private Dictionary<string, Brush> m_dicBrushes = new Dictionary<string, Brush>();

        private List<TextBlock> m_listTextBlocks = new List<TextBlock>();

        private List<Image> m_listImages = new List<Image>();

        private List<WebBrowser> m_listWeb = new List<WebBrowser>();

        private List<MediaPlayer> m_listMediaPlayers = new List<MediaPlayer>();
#endregion
    }
}
