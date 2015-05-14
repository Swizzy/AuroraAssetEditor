// 
// 	AuroraDbManager.cs
// 	AuroraAssetEditor
// 
// 	Created by Swizzy on 14/05/2015
// 	Copyright (c) 2015 Swizzy. All rights reserved.

namespace AuroraAssetEditor.Classes {
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SQLite;
    using System.IO;
    using System.Linq;
    using System.Threading;

    internal static class AuroraDbManager {
        private static SQLiteConnection _content;

        private static void ConnectToContent(string path) {
            if(_content != null)
                _content.Close();
            _content = new SQLiteConnection("Data Source=\"" + path + "\";Version=3;");
            _content.Open();
        }

        private static DataTable GetContentDataTable(string sql) {
            var dt = new DataTable();
            try {
                var cmd = new SQLiteCommand(sql, _content);
                using(var reader = cmd.ExecuteReader())
                    dt.Load(reader);
            }
            catch(Exception ex) {
                MainWindow.SaveError(ex);
            }
            return dt;
        }

        public static XboxUnity.XboxUnityTitle[] GetDbTitles(string path) {
            ConnectToContent(path);
            var ret = GetContentItems().Select(item => new XboxUnity.XboxUnityTitle(item.TitleId, item.TitleName)).ToArray();
            _content.Close();
            GC.Collect();
            while(true) {
                try {
                    File.Delete(path);
                    break;
                }
                catch(IOException) {
                    Thread.Sleep(100);
                }
            }
            return ret;
        }

        private static IEnumerable<ContentItem> GetContentItems() { return GetContentDataTable("SELECT * FROM ContentItems").Select().Select(row => new ContentItem(row)).ToArray(); }

        private class ContentItem {
            public ContentItem(DataRow row) {
                TitleId = (int)((long)row["TitleId"]);
                TitleName = (string)row["TitleName"];
            }

            public int TitleId { get; private set; }

            public string TitleName { get; private set; }
        }
    }
}