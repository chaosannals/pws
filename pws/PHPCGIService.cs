using System;
using System.ServiceProcess;

namespace Pws
{
    /// <summary>
    /// 服务
    /// </summary>
    public partial class PhpWindowsService : ServiceBase
    {
        private PhpArchive archive80;
        private PhpArchive archive74;
        private PhpArchive archive73;
        private PhpCgiServerProxy proxy80;
        private PhpCgiServerProxy proxy74;
        private PhpCgiServerProxy proxy73;

        /// <summary>
        /// 服务初始化
        /// </summary>
        public PhpWindowsService()
        {
            "PHP-CGI 8.0.6 Server Initialize =======================================*".Log();
            archive80 = new PhpArchive("https://windows.php.net/downloads/releases/php-8.0.6-nts-Win32-vs16-x64.zip");
            archive80.ArchiveCompleted += new ArchiveCompletedEventHandler(Ready);
            proxy80 = new PhpCgiServerProxy(archive80, 9080);

            "PHP-CGI 7.4.24 Server Initialize =======================================*".Log();
            archive74 = new PhpArchive("https://windows.php.net/downloads/releases/php-7.4.24-nts-Win32-vc15-x64.zip");
            archive74.ArchiveCompleted += new ArchiveCompletedEventHandler(Ready);
            proxy74 = new PhpCgiServerProxy(archive74, 9074);

            "PHP-CGI 7.3.31 Server Initialize =======================================*".Log();
            archive73 = new PhpArchive("https://windows.php.net/downloads/releases/php-7.3.31-nts-Win32-VC15-x64.zip");
            archive73.ArchiveCompleted += new ArchiveCompletedEventHandler(Ready);
            proxy73 = new PhpCgiServerProxy(archive73, 9073);
        }

        /// <summary>
        /// 服务开始
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            try
            {
                // 8.0.6
                if (archive80.Validate())
                {
                    Ready();
                }
                else
                {
                    archive80.Ensure();
                }

                // 7.4.24
                if (archive74.Validate())
                {
                    Ready();
                }
                else
                {
                    archive74.Ensure();
                }

                // 7.3.31
                if (archive73.Validate())
                {
                    Ready();
                }
                else
                {
                    archive73.Ensure();
                }
            }
            catch (Exception e)
            {
                e.Message.Log();
                e.StackTrace.Log();
            }
        }

        /// <summary>
        /// 启动
        /// </summary>
        private void Ready()
        {
            "PHP-CGI 8.0.6 Start =======================================+".Log();
            proxy80.Start();
            "PHP-CGI 7.4.24 Start =======================================+".Log();
            proxy74.Start();
            "PHP-CGI 7.2.31 Start =======================================+".Log();
            proxy73.Start();
        }

        /// <summary>
        /// 服务关闭
        /// </summary>
        protected override void OnStop()
        {
            try
            {
                if (proxy80.IsActive)
                {
                    proxy80.Stop();
                    "PHP-CGI 8.0.6 Stop =======================================-".Log();
                }
                if (proxy74.IsActive)
                {
                    proxy74.Stop();
                    "PHP-CGI 7.4.24 Stop =======================================-".Log();
                }
                if (proxy73.IsActive)
                {
                    proxy73.Stop();
                    "PHP-CGI 7.2.31 Stop =======================================-".Log();
                }
            }
            catch (Exception e)
            {
                e.Message.Log();
                e.StackTrace.Log();
            }
            LogExtends.Finally();
        }
    }
}
