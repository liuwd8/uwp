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
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.ViewManagement;
using Windows.UI.Core;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using Windows.System;
using Windows.ApplicationModel.DataTransfer;
using System.Diagnostics;
using SQLite.Net;
using System.Threading.Tasks;
using System.Text;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace App1
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    sealed partial class MainPage : Page
    {
        public ViewModels.ItemViewModel ViewModel { get { return one; } }
        public static ViewModels.ItemViewModel one;
        private bool isWork;
        public FileOpenPicker openPicker;
        public BitmapImage bitmapImage;
        public StorageFile file;
        public bool isDelete;
        private string path = "Assets/document.jpg";

        public MainPage()
        {
            this.InitializeComponent();
            Model.Class1.length = Window.Current.Bounds.Width;
            if (one == null)
            {
                one = new ViewModels.ItemViewModel();
                Service.TileService.Update();
            }
            this.SizeChanged += (s, e) =>
            {
                if (e.NewSize.Width > 800)
                    isWork = false;
                else
                    isWork = true;
                Model.Class1.length = Window.Current.Bounds.Width;
            };
            if (Window.Current.Bounds.Width > 800)
                isWork = true;
            else
                isWork = false;
            this.bitmapImage = pic.Source as BitmapImage;
            if (one.selectedItem == null)
            {
                this.isDelete = false;
            }
            pic.Width = picSize.Width * MySlider.Value;
            pic.Height = picSize.Height * MySlider.Value;
        }
        
        private async Task getAllItems()
        {
            one.AllItems.Clear();
            var items = App.db.GetSQLite().Table<Model.Class1>().ToList();
            foreach (var item in items)
            {
                await item.SetBitmap(item.picPath);
                one.AddItem(item);
            }
            Service.TileService.Update();
        }

        private void AddAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if(isWork)
            {
                Frame rootFrame = Window.Current.Content as Frame;
                rootFrame.Navigate(typeof(NewPage));
            }
        }
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            await getAllItems();
            if (e.NavigationMode != NavigationMode.New)
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
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey("selectItem"))
                {
                    string id = (string)(ApplicationData.Current.LocalSettings.Values["selectItem"]);
                    foreach (var item in one.AllItems)
                    {
                        if (item.id == id)
                        {
                            one.selectedItem = item;
                            break;
                        }
                    }
                }
            }
            if (one.selectedItem != null)
            {
                title.Text = one.selectedItem.title;
                detail.Text = one.selectedItem.detail;
                pic.Source = one.selectedItem.pic;
                date.Date = one.selectedItem.date;
                MySlider.Value = one.selectedItem.size;
                path = one.selectedItem.picPath;
                pic.Width = picSize.Width * MySlider.Value;
                pic.Height = picSize.Height * MySlider.Value;
                isDelete = true;
                create.Content = "Update";
                DeleteAppBarButton.Visibility = Visibility;
            }
            ApplicationData.Current.LocalSettings.Values.Remove("selectItem");
            ApplicationData.Current.LocalSettings.Values.Remove("NewPage");
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            DataTransferManager.GetForCurrentView().DataRequested += OnShareDataRequested;
        }
        private bool find(Model.Class1 a,Model.Class1 b)
        {
            return a.id == b.id;
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            bool issuspend = ((App)App.Current).issuspend;
            if (issuspend)
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
            if (one.selectedItem != null)
            {
                ApplicationData.Current.LocalSettings.Values["selectItem"] = one.selectedItem.id;
            }
            DataTransferManager.GetForCurrentView().DataRequested -= OnShareDataRequested;
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            one.selectedItem = e.ClickedItem as Model.Class1;
            if (Window.Current.Bounds.Width > 800)
            {
                title.Text = one.selectedItem.title;
                detail.Text = one.selectedItem.detail;
                pic.Source = one.selectedItem.pic;
                date.Date = one.selectedItem.date;
                MySlider.Value = one.selectedItem.size;
                pic.Width = picSize.Width * MySlider.Value;
                pic.Height = picSize.Height * MySlider.Value;
                isDelete = true;
                create.Content = "Update";
                DeleteAppBarButton.Visibility = Visibility;
                path = one.selectedItem.picPath;
                bitmapImage = one.selectedItem.pic;
            }
            else
            {
                Frame rootFrame = Window.Current.Content as Frame;
                rootFrame.Navigate(typeof(NewPage));
            }
        }

        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            one.selectedItem = (sender as FrameworkElement).DataContext as Model.Class1;
            if (Window.Current.Bounds.Width > 800)
            {
                title.Text = one.selectedItem.title;
                detail.Text = one.selectedItem.detail;
                pic.Source = one.selectedItem.pic;
                date.Date = one.selectedItem.date;
                MySlider.Value = one.selectedItem.size;
                pic.Width = picSize.Width * MySlider.Value;
                pic.Height = picSize.Height * MySlider.Value;
                isDelete = true;
                create.Content = "Update";
                DeleteAppBarButton.Visibility = Visibility;
                path = one.selectedItem.picPath;
            }
            else
            {
                Frame rootFrame = Window.Current.Content as Frame;
                rootFrame.Navigate(typeof(NewPage));
            }
        }

        private void MenuFlyoutItem_Click_1(object sender, RoutedEventArgs e)
        {
            one.selectedItem = (sender as FrameworkElement).DataContext as Model.Class1;
            one.Remove();
            Cancel(sender, e);
            Service.TileService.Update();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            DependencyObject parent = VisualTreeHelper.GetParent((CheckBox)sender);
            CheckBox ls = (CheckBox)VisualTreeHelper.GetChild(parent, 0);
            Line line = (Line)VisualTreeHelper.GetChild(parent, 3);
            line.Visibility = Visibility.Visible;
            Model.Class1 a = (sender as FrameworkElement).DataContext as Model.Class1;
            if (a == null) return;
            a.ischecked = true;
            one.selectedItem = a;
            one.Update(a);
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            DependencyObject parent = VisualTreeHelper.GetParent((CheckBox)sender);
            CheckBox ls = (CheckBox)VisualTreeHelper.GetChild(parent, 0);
            Line line = (Line)VisualTreeHelper.GetChild(parent, 3);
            line.Visibility = Visibility.Collapsed;
            Model.Class1 a = (sender as FrameworkElement).DataContext as Model.Class1;
            if (a == null) return;
            a.ischecked = false;
            one.selectedItem = a;
            one.Update(a);
        }

        private void Create(object sender, RoutedEventArgs e)
        {
            bool isReload = false;
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
                    title.Text = "";
                    detail.Text = "";
                    date.Date = DateTimeOffset.Now.AddDays(-1);
                    MySlider.Value = 1.0;
                    path = "Assets/document.jpg";
                    bitmapImage = new BitmapImage(new Uri("ms-appx:///" + path));
                    pic.Source = bitmapImage;
                }
                else
                {
                    MainPage.one.Update(new Model.Class1(title.Text, detail.Text, date.Date, MySlider.Value, path, bitmapImage));
                    isReload = true;
                    message += "更新成功";
                }
                Service.TileService.Update();
            }
            new MessageDialog(message).ShowAsync();
            if (isReload)
            {
                Frame rootFrame = Window.Current.Content as Frame;
                rootFrame.Navigate(typeof(MainPage));
            }
        }
        private void Cancel(object sender, RoutedEventArgs e)
        {
            if (one.selectedItem == null)
            {
                title.Text = "";
                detail.Text = "";
                date.Date = DateTimeOffset.Now.AddDays(-1);
                MySlider.Value = 1.0;
                path = "Assets/document.jpg";
                bitmapImage = new BitmapImage(new Uri("ms-appx:///" + path));
                pic.Source = bitmapImage;
            }
            else
            {
                title.Text = one.selectedItem.title;
                detail.Text = one.selectedItem.detail;
                date.Date = one.selectedItem.date;
                pic.Source = one.selectedItem.pic;
                path = one.selectedItem.picPath;
                MySlider.Value = one.selectedItem.size;
                pic.Width = picSize.Width * MySlider.Value;
                pic.Height = picSize.Height * MySlider.Value;
                bitmapImage = one.selectedItem.pic;
            }
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
            }
        }
        public void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            AppBarButton_ClickAsync(sender, e);
        }

        private void DeleteAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            one.Remove();
            Cancel(sender, e);
            Service.TileService.Update();
        }

        private void MySlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            pic.Width = picSize.Width * MySlider.Value;
            pic.Height = picSize.Height * MySlider.Value;
        }

        private async void MenuFlyoutItem_Click_2(object sender, RoutedEventArgs e)
        {
            one.selectedItem = (sender as FrameworkElement).DataContext as Model.Class1;
            DataTransferManager.ShowShareUI();
        }
        async void OnShareDataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            var request = args.Request;
            request.Data.Properties.Title = one.selectedItem.title;
            request.Data.Properties.Description = one.selectedItem.detail;
            request.Data.SetText("" + one.selectedItem.date.Year + "." + one.selectedItem.date.Month + "." + one.selectedItem.date.Day + "\n");
            var dp = args.Request.Data;
            var deferral = args.Request.GetDeferral();
            var photoFile = await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFileAsync(one.selectedItem.picPath);
            dp.Properties.Title = one.selectedItem.title;
            dp.Properties.Description = one.selectedItem.detail;
            dp.SetStorageItems(new List<StorageFile> { photoFile });
            dp.SetWebLink(new Uri("http://helloworld.com/helloworld"));
            deferral.Complete();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            List<Model.Class1> ListA = App.db.Query(searchCondition.Text);
            StringBuilder message = new StringBuilder();
            foreach(var l in ListA)
            {
                message.Append(l.title).Append(" ").Append(l.detail).Append(" ").Append(l.date.LocalDateTime.ToString("yyyy/MM/dd hh:mm:ss")).Append("\n");
            }
            new MessageDialog(message.ToString()).ShowAsync();
        }
    }
}