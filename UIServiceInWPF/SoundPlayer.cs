/********************************************************************
	FileName:   SoundPlayer
    purpose:	

	author:		huang wei
	created:	2013/01/13

    revised history:
	2013/01/13  

================================================================
    Copyright (C) 2013, Grgbanking CO,. Ltd. All rights reserved.
================================================================
********************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Diagnostics;
using LogProcessorService;
using System.IO;
using System.Runtime.InteropServices;

//using WMPLib;

namespace UIServiceInWPF
{
    static class mediaAPI
    {
        [DllImport("Winmm.dll")]
        public static extern bool PlaySound( string argSound,
                                             IntPtr argModule,
                                             int    argFlag );

        public const int SND_FILENAME = 0x00020000;

        public const int SND_LOOP = 0x0008;

        [DllImport("Winmm.dll",CharSet = CharSet.Auto)]
        public static extern int mciSendString( string argCommand,
                                                string argReturnString,
                                                uint argReturnLength,
                                                IntPtr argHwndCallback );
    }

    public enum Playstate : byte
    {
        Stopped = 1,
        Playing = 2,
        Pause = 3
    } 

    class SoundPlayer : IDisposable
    {
#region constructor
        private SoundPlayer()
        {
            //m_player = new WindowsMediaPlayer();
            //m_player.PlayStateChange += new _WMPOCXEvents_PlayStateChangeEventHandler(m_player_PlayStateChange);
            //m_player.MediaError += new _WMPOCXEvents_MediaErrorEventHandler(m_player_MediaError);
            //m_player = new MediaPlayer();

            //m_player.MediaOpened += new EventHandler(m_player_MediaOpened);
            //m_player.MediaEnded += new EventHandler(m_player_MediaEnded);
            //m_player.MediaFailed += new EventHandler<ExceptionEventArgs>(m_player_MediaFailed);
        }

        static SoundPlayer()
        {
            s_player = new SoundPlayer();
            Debug.Assert(null != s_player);
        }
#endregion

#region method
        public void Dispose()
        {
            //m_player.MediaFailed -= m_player_MediaFailed;
            //m_player.MediaEnded -= m_player_MediaEnded;
            //m_player.MediaOpened -= m_player_MediaOpened;
            //m_player.PlayStateChange -= m_player_PlayStateChange;
            //m_player.MediaError -= m_player_MediaError;

            //m_player.close();
            //m_player = null;
            //m_player.Close();
            //m_player = null;
        }

        public bool Play( string argFilePath,
                          bool argLoop = false,
                          bool argSkipEnable = false)
        {
 //           return true;

            Debug.Assert(!string.IsNullOrEmpty(argFilePath));
            if ( !File.Exists(argFilePath) )
            {
                return true;
            }

            try
            {
                m_isLoop = argLoop;
  //              m_isStop = false;
                m_skipEnable = argSkipEnable;

                Log.UIService.LogDebugFormat("Prepare for open and play a sound [{0}]", argFilePath);
#if USE_PLAYSOUND
                if ( m_isLoop )
                {
                    mediaAPI.PlaySound(argFilePath, IntPtr.Zero, mediaAPI.SND_FILENAME | mediaAPI.SND_LOOP);
                }
                else
                {
                    mediaAPI.PlaySound(argFilePath, IntPtr.Zero, mediaAPI.SND_FILENAME);
                }
#else
                if (!m_skipEnable || (m_skipEnable && Playstate.Playing != GetPlayState()))
                {
                    mediaAPI.mciSendString("close all", string.Empty, 0, IntPtr.Zero);
                    mediaAPI.mciSendString(string.Format("open {0} alias media", argFilePath), string.Empty, 0, IntPtr.Zero);
                    if (m_isLoop)
                    {
                        mediaAPI.mciSendString("play media repeat", string.Empty, 0, IntPtr.Zero);
                    }
                    else
                    {
                        mediaAPI.mciSendString("play media", string.Empty, 0, IntPtr.Zero);
                    }
                }
#endif

                //                IWMPControls iControl = m_player.controls;
                //if (argFilePath.Equals(m_mediaFile, StringComparison.OrdinalIgnoreCase))
                //{
                //    Log.UIService.LogDebugFormat("Prepare for play a sound [{0}]", m_mediaFile);
                //    iControl.stop();
                //    iControl.play();
                //    //m_player.Position = m_beginPos;
                //    //m_player.Play();
                //}
                //else
                //{
                //    Log.UIService.LogDebugFormat("Prepare for open and play a sound [{0}]", argFilePath);
                //    m_player.close();
                //    m_player.URL = argFilePath;
                //    iControl.play();
                //    //m_player.Close();
                //    //m_player.Open(new Uri(argFilePath));
                //    m_mediaFile = argFilePath;
                //}
                //iControl = null;
            }
            catch (System.Exception ex)
            {
                Log.UIService.LogWarn(string.Format("Failed to play a sound file [{0}]", argFilePath), ex);
                return false;            	
            }

            return true;
        }

        public void Stop()
        {
//            Log.UIService.LogDebug("Prepare for stopping playing");

            //         m_isStop = true;
#if USE_PLAYSOUND
            mediaAPI.PlaySound(null, IntPtr.Zero, 0);
#else
            mediaAPI.mciSendString("close all", string.Empty, 0, IntPtr.Zero);
#endif

            //         m_mediaFile = null;
 //           m_player.controls.stop();
            //m_player.close();
          //  m_player.Stop();
           // m_player.Close();
        }

        /// <summary>   
        /// 获得当前媒体的播放状态
        /// </summary>   
        /// <returns></returns>   
        public Playstate GetPlayState()
        {
            
            Playstate status = Playstate.Stopped;
            durLength = "";
            durLength = durLength.PadLeft(128, Convert.ToChar(" "));
            mediaAPI.mciSendString("status media mode", durLength, (uint)durLength.Length, IntPtr.Zero);
            durLength = durLength.Trim();
            if (!string.IsNullOrWhiteSpace(durLength) && durLength != "\0")
            {
                if (durLength.Substring(0, 7) == "playing" || durLength.Substring(0, 2) == "播放")
                {
                    status = Playstate.Playing;
                }
                else if (durLength.Substring(0, 7) == "stopped" || durLength.Substring(0, 2) == "停止")
                {
                    status = Playstate.Stopped;
                }
                else
                {
                    status = Playstate.Pause;
                }
            }

            return status;

        } 

        //void m_player_MediaError(object pMediaObject)
        //{
        //    //Debug.WriteLine("Opening media is fail");
        //    Log.UIService.LogDebug("Failed to open media file");
        //    m_mediaFile = null;
        //}

        //void m_player_PlayStateChange(int NewState)
        //{
        //    if ( (WMPPlayState)NewState == WMPPlayState.wmppsReady )
        //    {

        //    }
        //    else if ((WMPPlayState)NewState == WMPPlayState.wmppsMediaEnded)
        //    {
        //        //Debug.WriteLine("Media is end");
        //        Log.UIService.LogDebug("Playing media file is end");
        //        if (m_isStop)
        //        {
        //            return;
        //        }
        //        //m_player.Stop();
        //        IWMPControls iControl = m_player.controls;
        //        if (m_isLoop)
        //        {
        //            iControl.stop();
        //            iControl.play();
        //        }
        //        iControl = null;
        //    }
        //}

        //private void m_player_MediaEnded(object sender, EventArgs e)
        //{
        //    Debug.WriteLine("Media is end");
        //    if ( m_isStop )
        //    {
        //        return;
        //    }
        //    //m_player.Stop();
        //    if ( m_isLoop )
        //    {
        //   //     m_player.Position = m_beginPos;
        //   //     m_player.Play();
        //    }
        //}

        //private void m_player_MediaFailed(object sender, ExceptionEventArgs e)
        //{
        //    Debug.WriteLine("Opening media is fail");
        //    m_mediaFile = null;
        //}

        //private void m_player_MediaOpened(object sender, EventArgs e)
        //{
        //    Debug.WriteLine("Opening media is success");
        //    if ( m_isStop )
        //    {
        //        return;
        //    }
        ////    m_player.Play();
        //}
#endregion

#region property
        public static SoundPlayer Instance
        {
            get
            {
                return s_player;
            }
        }
#endregion

#region field
        private static SoundPlayer s_player = null;

  //      private WindowsMediaPlayer m_player = null;
 //       private MediaPlayer m_player = null;

  //      private TimeSpan m_beginPos = new TimeSpan(0);

 //       private string m_mediaFile = null;

//        private bool m_isStop = true;

        private bool m_isLoop = false;

        private bool m_skipEnable = false;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        private string durLength = "";
#endregion
    }
}
