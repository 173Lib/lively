﻿using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Interop;
using System.Windows.Threading;

namespace livelywpf.Core
{
    /// <summary>
    /// Built in Windws media foundation player.
    /// </summary>
    public class VideoPlayerWPF : IWallpaper
    {
        IntPtr HWND { get; set; }
        MediaElementWPF Player { get; set; }
        LibraryModel Model { get; set; }
        LivelyScreen Display { get; set; }
        public event EventHandler<WindowInitializedArgs> WindowInitialized;

        public VideoPlayerWPF(string filePath, LibraryModel model, LivelyScreen display, WallpaperScaler scaler = WallpaperScaler.fill)
        {
            Player = new MediaElementWPF(filePath, scaler);
            this.Model = model;
            this.Display = display;
        }

        public WallpaperType GetWallpaperType()
        {
            return WallpaperType.video;
        }
        public LibraryModel GetWallpaperData()
        {
            return Model;
        }
        public IntPtr GetHWND()
        {
            return HWND;
        }
        public void SetHWND(IntPtr hwnd)
        {
            this.HWND = hwnd;
        }
        public Process GetProcess()
        {
            return null;
        }
        public void Play()
        {
            Player.PlayMedia();
        }
        public void Pause()
        {
            Player.PausePlayer();
        }
        public void Stop()
        {
            Player.StopPlayer();
        }
        public void Close()
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new ThreadStart(delegate
            {
                Player.Close();
            }));
        }

        public LivelyScreen GetScreen()
        {
            return Display;
        }

        public void Show()
        {
            if(Player != null)
            {
                Player.Closed += Player_Closed;
                Player.Show();
                HWND = new WindowInteropHelper(Player).Handle;
                WindowInitialized?.Invoke(this, new WindowInitializedArgs() { Success = true, Error = null });
            }
        }

        private void Player_Closed(object sender, EventArgs e)
        {
            SetupDesktop.RefreshDesktop();
        }

        public void SendMessage(string msg)
        {
            //throw new NotImplementedException();
        }

        public string GetLivelyPropertyCopyPath()
        {
            return null;
        }

        public void SetScreen(LivelyScreen display)
        {
            this.Display = display;
        }

        public void Terminate()
        {
            Close();
        }

        public void Resume()
        {
            //throw new NotImplementedException();
        }

        public void SetVolume(int volume)
        {
            Player.SetVolume(volume);
        }

        public void SetPlaybackPos(int pos)
        {
            //todo
        }
    }
}
