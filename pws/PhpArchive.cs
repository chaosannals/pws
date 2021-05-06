using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.IO.Compression;

namespace Pws
{
    public delegate void ArchiveCompletedEventHandler();

    public class PhpArchive
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
        public Uri FileUri { get; private set; }
        public string FileName { get; private set; }
        public string FileFolder { get; private set; }
        public bool IsCompleted { get; private set; }

        public event ArchiveCompletedEventHandler ArchiveCompleted;


        public PhpArchive(string uri)
        {
            FileUri = new Uri(uri);
            FileName = Path.Combine(Folder, Path.GetFileName(FileUri.AbsolutePath));
            FileFolder = Path.Combine(Folder, Path.GetFileNameWithoutExtension(FileName));
            IsCompleted = false;
            client = new WebClient();
        }

        /// <summary>
        /// 验证
        /// </summary>
        /// <returns></returns>
        public bool Validate()
        {
            if (!Directory.Exists(FileFolder))
            {
                return false;
            }

            string iniPath = Path.Combine(FileFolder, "php.ini");
            return File.Exists(iniPath);
        }

        /// <summary>
        /// 确保
        /// </summary>
        public void Ensure()
        {
            if (File.Exists(FileName))
            {
                Unzip();
            }
            else
            {
                Download();
            }
        }

        /// <summary>
        /// 下载
        /// </summary>
        public void Download()
        {
            IsCompleted = false;
            client.Headers.Set(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.93 Safari/537.36");
            client.DownloadFileCompleted += new AsyncCompletedEventHandler(Downloaded);
            client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
            client.DownloadFileAsync(FileUri, FileName);
            "开始下载: {0} 到 {1}".Log(FileUri, FileName);
        }

        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            "{0} 下载完成：{1}%".Log(FileUri, e.ProgressPercentage);
        }

        private void Downloaded(object sender, AsyncCompletedEventArgs e)
        {
            "{0} 下载完成".Log(FileUri);
            Unzip();
        }

        /// <summary>
        /// 解压
        /// </summary>
        public void Unzip()
        {
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
            "{0} 解压到：{1}".Log(FileName, FileFolder);
            string devIniPath = Path.Combine(FileFolder, "php.ini-development");
            string iniPath = Path.Combine(FileFolder, "php.ini");
            File.Copy(devIniPath, iniPath);
            IsCompleted = true;
            ArchiveCompleted.Invoke();
        }

        public static void UninstallAll()
        {
            if (Directory.Exists(Folder))
            {
                Directory.Delete(folder, true);
            }
        }
    }
}
