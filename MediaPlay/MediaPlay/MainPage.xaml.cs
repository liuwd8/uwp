using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace MediaPlay
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        MediaPlayer mediaPlayer = new MediaPlayer();
        MediaTimelineController timelineController = new MediaTimelineController();
        TimeSpan timeSpan;
        StorageFile file = null;
        string str = "";

        public MainPage()
        {
            this.InitializeComponent();
            MediaSource mediaSource = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/1.mp3"));
            mediaSource.OpenOperationCompleted += openOperationCompleted;
            mediaPlayer.Source = mediaSource;
            mediaPlayer.CommandManager.IsEnabled = false;
            mediaPlayer.AudioCategory = MediaPlayerAudioCategory.Media;
            mediaPlayer.TimelineController = timelineController;
            myMedia.SetMediaPlayer(mediaPlayer);
        }

        private async void openOperationCompleted(MediaSource sender, MediaSourceOpenOperationCompletedEventArgs args)
        {
            timeSpan = sender.Duration.GetValueOrDefault();
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                timeLine.Minimum = 0;
                timeLine.Maximum = timeSpan.TotalSeconds;
                str = "/" + Math.Floor(timeLine.Maximum/3600).ToString("00") + ":" + (Math.Floor(timeLine.Maximum / 60) % 60).ToString("00") + ":" + (Math.Floor(timeLine.Maximum) % 60).ToString("00");
                displayTime.Text = "00:00:00" + str;
                timeLine.StepFrequency = 1;
            });
        }

        private void pause_Click(object sender, RoutedEventArgs e)
        {
            timelineController.Pause();
            if(IsAudio())
                EllStoryboard.Pause();
            pause.Visibility = Visibility.Collapsed;
            play.Visibility = Visibility.Visible;
        }

        private void stop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                timelineController.Position = TimeSpan.FromSeconds(0);
                timelineController.Pause();
                EllStoryboard.Stop();
            }
            catch
            {

            }
        }

        private void display_Click(object sender, RoutedEventArgs e)
        {
            ApplicationView view = ApplicationView.GetForCurrentView();
            if (view.IsFullScreenMode)
            {
                ImageBrush imageBrush = new ImageBrush
                {
                    ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/music.jpg", UriKind.Absolute))
                };
                grid.Background = imageBrush;
                view.ExitFullScreenMode();
            }
            else
            {
                if (!IsAudio())
                {
                    grid.Background = new SolidColorBrush(Colors.Black);
                    rotatePic.Visibility = Visibility.Collapsed;
                    EllStoryboard.Stop();
                }
                view.TryEnterFullScreenMode();
            }
        }

        private async void openfile_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".mp3");
            openPicker.FileTypeFilter.Add(".mp4");
            openPicker.FileTypeFilter.Add(".wma");
            openPicker.FileTypeFilter.Add(".mkv");
            file = await openPicker.PickSingleFileAsync();
            try
            {
                if(file != null)
                {
                    var mediaSource = MediaSource.CreateFromStorageFile(file);
                    mediaSource.OpenOperationCompleted += openOperationCompleted;
                    mediaPlayer.Source = mediaSource;
                    Debug.WriteLine(mediaPlayer.AudioCategory);
                    if (!IsAudio())
                    {
                        rotatePic.Visibility = Visibility.Collapsed;
                        EllStoryboard.Stop();
                    }
                    else
                    {
                        rotatePic.Visibility = Visibility.Visible;
                        StorageItemThumbnail thumbnail = await file.GetThumbnailAsync(ThumbnailMode.SingleItem);
                        if (thumbnail == null)
                        {
                            thumb.ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/1.jpg"));
                        }
                        else
                        {
                            BitmapImage image = new BitmapImage();
                            image.SetSource(thumbnail);
                            thumb.ImageSource = image;
                        }
                    }
                    if(timelineController.State == MediaTimelineControllerState.Running)
                    {
                        timeLine.Value = 0;
                        timelineController.Start();
                    }
                }
            }
            catch
            {

            }
        }

        private void play_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DispatcherTimer timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(1);
                timer.Tick += TimerTick;
                timer.Start();
                if(IsAudio())
                    EllStoryboard.Begin();
                timelineController.Resume();
                play.Visibility = Visibility.Collapsed;
                pause.Visibility = Visibility.Visible;
            }
            catch
            {

            }
        }

        private void TimerTick(object sender, object e)
        {
            timeLine.Value = ((TimeSpan)timelineController.Position).TotalSeconds;
            displayTime.Text = timelineController.Position.ToString().Substring(0, 8) + str;
            if (timeLine.Value == timeLine.Maximum)
            {
                timelineController.Position = TimeSpan.FromSeconds(0);
                timelineController.Pause();
                EllStoryboard.Stop();
            }
        }
        private bool IsAudio()
        {
            return file==null || file.FileType == ".mp3" || file.FileType == ".wma";
        }
    }
    class TimeLineConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return ((TimeSpan)value).TotalSeconds;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return TimeSpan.FromSeconds((double)value);
        }
    }
}
