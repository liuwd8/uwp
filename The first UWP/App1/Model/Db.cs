using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite.Net;
using SQLite.Net.Attributes;
using SQLite.Net.Interop;
using SQLite.Net.Platform.WinRT;

namespace App1.Model
{
    public class Db
    {
        private static SQLiteConnection db;
        public string DbName = "demo1.db";
        public string DbPath;
        public Db() { }
        public void Init()
        {
            DbPath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, DbName);
            if (db == null)
            {
                db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath);
                db.CreateTable<Class1>();
            }
        }
        public void insert(Class1 item)
        {
            db.Insert(item);
        }
        public void Remove(Class1 item)
        {
            db.Delete(item);
        }
        public int Update(Class1 item)
        {
            int i = db.Update(item);
            return i;
        }
        public SQLiteConnection GetSQLite()
        {
            return db;
        }
        public List<Class1> Query(string q)
        {
            return db.Table<Class1>().Where(v => (v.title.Contains(q) || v.detail.Contains(q) || (v.date + "").Contains(q))).ToList();
        }

        ~Db()
        {
            db.Close();
        }
    }
}
