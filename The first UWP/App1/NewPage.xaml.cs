using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage;
using Windows.UI.Core;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板
namespace App1
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    sealed partial class NewPage : Page
    {
        public FileOpenPicker openPicker;
        public BitmapImage bitmapImage;
        public StorageFile file;
        private Model.Class1 ori;
        private string path = "Assets/document.jpg";
        public bool isDelete;
        public NewPage()
        {
            this.InitializeComponent();
            this.bitmapImage = pic.Source as BitmapImage;
            if(this.ori == null)
            {
                this.ori = new Model.Class1("", "", DateTimeOffset.Now,MySlider.Value, path, this.bitmapImage);
                this.isDelete = false;
            }
            pic.Width = picSize.Width * MySlider.Value;
            pic.Height = picSize.Height * MySlider.Value;
        }
        private void Create(object sender, RoutedEventArgs e)
        {
            string message = "";
            if (title.Text == "")
                message += "title不能为空\n";
            if (detail.Text == "")
                message += "detail不能为空\n";
            if (date.Date <= System.DateTimeOffset.Now.AddDays(-1))
                message += "无效日期";
            if (message == "")
            {
                if (isDelete == false)
                {
                    Model.Class1 a = new Model.Class1(title.Text, detail.Text, date.Date, MySlider.Value, path, bitmapImage);
                    App.db.insert(a);
                    MainPage.one.AddItem(a);
                    message += "创建成功";
                }
                else
                {
                    MainPage.one.Update(new Model.Class1(title.Text, detail.Text, date.Date, MySlider.Value, path, bitmapImage));
                    message += "更新成功";
                }
                title.Text = "";
                detail.Text = "";
                date.Date = DateTimeOffset.Now.AddDays(-1);
                MySlider.Value = 1.0;
                path = "Assets/document.jpg";
                bitmapImage = new BitmapImage(new Uri("ms-appx:///" + path));
                Service.TileService.Update();
                Frame rootFrame = Window.Current.Content as Frame;
                rootFrame.Navigate(typeof(MainPage));
            }
            new MessageDialog(message).ShowAsync();
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            if (MainPage.one.selectedItem == null)
                return;
            ori = MainPage.one.selectedItem;
            title.Text = ori.title;
            detail.Text = ori.detail;
            date.Date = ori.date;
            pic.Source = ori.pic;
            path = ori.picPath;
            MySlider.Value = ori.size;
            pic.Width = picSize.Width * MySlider.Value;
            pic.Height = picSize.Height * MySlider.Value;
            bitmapImage = ori.pic;
        }

        private async void AppBarButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".gif");
            openPicker.FileTypeFilter.Add(".png");
            file = await openPicker.PickSingleFileAsync();
            try
            {
                using (IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read))
                {
                    bitmapImage = new BitmapImage();
                    await bitmapImage.SetSourceAsync(fileStream);
                    pic.Source = bitmapImage;
                    path = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(file);
                }
            }
            catch (Exception)
            {
                throw new Exception(file.ToString());
            }
        }
        public void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            AppBarButton_ClickAsync(sender, e);
        }
        protected override async void OnNavigatedTo (NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.New)
            {
                if (MainPage.one.selectedItem != null)
                {
                    ori = MainPage.one.selectedItem;
                    title.Text = ori.title;
                    detail.Text = ori.detail;
                    pic.Source = ori.pic;
                    date.Date = ori.date;
                    MySlider.Value = ori.size;
                    path = ori.picPath;
                    pic.Width = picSize.Width * MySlider.Value;
                    pic.Height = picSize.Height * MySlider.Value;
                    isDelete = true;
                    create.Content = "Update";
                    bitmapImage = ori.pic;
                    DeleteAppBarButton.Visibility = Visibility;
                }
            }
            else
            {
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey("NewPage"))
                {
                    ApplicationDataCompositeValue composite = ApplicationData.Current.LocalSettings.Values["NewPage"] as ApplicationDataCompositeValue;
                    title.Text = (string)composite["title"];
                    detail.Text = (string)composite["detail"];
                    path = (string)composite["Path"];
                    if (path != "Assets/document.jpg")
                    {
                        file = await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFileAsync(path);
                        try
                        {
                            using (IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read))
                            {
                                bitmapImage = new BitmapImage();
                                await bitmapImage.SetSourceAsync(fileStream);
                                pic.Source = bitmapImage;
                            }
                        }
                        catch (Exception)
                        {
                            throw new Exception(file.ToString());
                        }
                    }
                    else
                    {
                        pic.Source = bitmapImage = new BitmapImage(new Uri("ms-appx:///" + path));
                    }
                    date.Date = (DateTimeOffset)composite["date"];
                    MySlider.Value = (double)composite["MySlider"];
                    pic.Width = picSize.Width * MySlider.Value;
                    pic.Height = picSize.Height * MySlider.Value;
                    isDelete = (bool)composite["isDelete"];
                    if (isDelete)
                    {
                        create.Content = "Update";
                        DeleteAppBarButton.Visibility = Visibility;
                    }
                }
            }
            ApplicationData.Current.LocalSettings.Values.Remove("NewPage");
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
        }

        private void DeleteAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            MainPage.one.Remove();
            Frame rootFrame = Window.Current.Content as Frame;
            rootFrame.Navigate(typeof(MainPage));
        }
        private void AddAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if(Window.Current.Bounds.Width < 800)
            {
                Frame rootFrame = Window.Current.Content as Frame;
                rootFrame.Navigate(typeof(NewPage));
            }
        }

        private void MySlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            pic.Width = picSize.Width * MySlider.Value;
            pic.Height = picSize.Height * MySlider.Value;
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            bool issuspend = ((App)App.Current).issuspend;
            if(issuspend)
            {
                ApplicationDataCompositeValue composite = new ApplicationDataCompositeValue();
                composite["title"] = title.Text;
                composite["detail"] = detail.Text;
                composite["Path"] = path;
                composite["date"] = date.Date;
                composite["MySlider"] = MySlider.Value;
                composite["isDelete"] = isDelete;
                ApplicationData.Current.LocalSettings.Values["NewPage"] = composite;
            }
        }
    }
}
