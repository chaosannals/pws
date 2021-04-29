using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.IO.Compression;

namespace Pws.Aphp
{
    public class PhpArchiveManager
    {
        private static string folder;

        private WebClient client;

        public static string Folder
        {
            get
            {
                if (folder == null)
                {
                    folder = Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        "php"
                    );
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }
                }
                return folder;
            }
        }

        public string FileName { get; private set; }
        public Uri FileUri { get; private set; }
        public string FileFolder { get; private set; }


        public PhpArchiveManager(string uri)
        {
            FileUri = new Uri(uri);
            FileName = Path.Combine(Folder, Path.GetFileName(FileUri.AbsolutePath));
            FileFolder = Path.GetFileNameWithoutExtension(FileName);
            client = new WebClient();
        }

        public void Download()
        {
            client.DownloadFileCompleted += new AsyncCompletedEventHandler(Downloaded);
            client.DownloadFileAsync(FileUri, FileName);
        }

        public void Downloaded(object sender, AsyncCompletedEventArgs e)
        {
            if (!Directory.Exists(FileFolder))
            {
                Directory.CreateDirectory(FileFolder);
            }
            ZipStorer zip = ZipStorer.Open(FileName, FileAccess.Read);
            List<ZipStorer.ZipFileEntry> dir = zip.ReadCentralDir();
            foreach (ZipStorer.ZipFileEntry entry in dir)
            {
                string p = Path.Combine(FileFolder, entry.FilenameInZip);
                string d = Path.GetDirectoryName(p);
                if (!Directory.Exists(d))
                {
                    Directory.CreateDirectory(d);
                }
                zip.ExtractFile(entry, p);
            }
            zip.Close();
        }
    }
}
