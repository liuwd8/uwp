using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App1;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Runtime.Serialization;
using Windows.Storage.Streams;
using Windows.Storage;
using Windows.Foundation;
using Windows.Storage.FileProperties;
using SQLite.Net;
using SQLite.Net.Attributes;
using System.Diagnostics;

namespace App1.Model
{
    public class Class1
    {

        [PrimaryKey]
        public string id { get; set; }

        public string title { get; set; }
        public string detail { get; set; }
        public DateTimeOffset date  {get; set; }
        public double size { get; set; }
        public bool? ischecked { get; set; }
        public string picPath { get; set; }

        [Ignore]
        public BitmapImage pic { get; set; }
        public double len { get { return length; } }
        public static double length;

        public Class1() { }

        public Class1(string title,string detail, DateTimeOffset date,double size, string picPath = "Assets/document.jpg", BitmapImage pic = null, bool? ischecked = false)
        {
            this.id = Guid.NewGuid().ToString();
            this.title = title;
            this.detail = detail;
            this.date = date;
            this.size = size;
            this.picPath = picPath;
            this.ischecked = ischecked;
            if(pic == null)
            {
                SetBitmap(picPath);
            }
            else
            {
                this.pic = pic;
            }
        }
        public void Update(string title, string detail, DateTimeOffset date, double size, string picPath = "Assets/document.jpg", BitmapImage pic = null, bool? ischecked = false)
        {
            this.title = title;
            this.detail = detail;
            this.date = date;
            this.size = size;
            this.picPath = picPath;
            this.ischecked = ischecked;
            if (pic == null)
            {
                SetBitmap(picPath);
            }
            else
            {
                this.pic = pic;
            }
        }
        public async Task SetBitmap(string str)
        {
            if (str != "Assets/document.jpg")
            {
                StorageFile file = await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFileAsync(str);
                try
                {
                    using (IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read))
                    {
                        this.pic = new BitmapImage();
                        await this.pic.SetSourceAsync(fileStream);
                    }
                }
                catch (Exception)
                {
                    throw new Exception(file.ToString());
                }
            }
            else
            {
                this.pic = new BitmapImage(new Uri("ms-appx:///" + str));
                if (str == "" || str == null)
                    new MessageDialog("图片路径已丢失").ShowAsync();
            }
        }
    }
}
namespace App1.ViewModels
{
    class ItemViewModel
    {
        private ObservableCollection<Model.Class1> allItems;
        public ObservableCollection<Model.Class1> AllItems { get { return this.allItems; } }
        public Model.Class1 selectedItem { get; set; }

        public void AddItem(string title, string detail, DateTimeOffset date, double size, string picPath = "Assets/document.jpg", BitmapImage pic = null, bool? ischecked = false)
        {
            Model.Class1 a = new Model.Class1(title, detail, date, size, picPath, pic, ischecked);
            this.allItems.Add(a);
            this.selectedItem = null;
        }
        public void AddItem(Model.Class1 a)
        {
            this.allItems.Add(a);
            this.selectedItem = null;
        }
        public void Update(Model.Class1 a)
        {
            if (selectedItem != null)
            {
                a.id = selectedItem.id;
                App.db.Update(a);
                this.selectedItem.Update(a.title, a.detail, a.date, a.size,a.picPath, a.pic,a.ischecked);
            }
            this.selectedItem = null;
        }
        public void Remove()
        {
            App.db.Remove(selectedItem);
            if(selectedItem != null)
                this.allItems.Remove(selectedItem);
            selectedItem = null;
        }
        public ItemViewModel()
        {
            this.allItems = new ObservableCollection<Model.Class1> { };
            selectedItem = null;
        }
    }
}