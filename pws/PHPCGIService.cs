using System;
using System.ServiceProcess;

namespace Pws
{
    /// <summary>
    /// 服务
    /// </summary>
    public partial class PhpCgiService : ServiceBase
    {
        private PhpArchive archive;
        private PhpCgiServerProxy proxy;

        /// <summary>
        /// 服务初始化
        /// </summary>
        public PhpCgiService()
        {
            "PHP-CGI Server Initialize =======================================*".Log();
            archive = new PhpArchive("https://windows.php.net/downloads/releases/php-8.0.6-nts-Win32-vs16-x64.zip");
            archive.ArchiveCompleted += new ArchiveCompletedEventHandler(Ready);
            proxy = new PhpCgiServerProxy(archive);
        }

        /// <summary>
        /// 服务开始
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            try
            {
                if (archive.Validate())
                {
                    Ready();
                }
                else
                {
                    archive.Ensure();
                }
            }
            catch (Exception e)
            {
                e.Message.Log();
                e.StackTrace.Log();
            }
        }

        private void Ready()
        {
            "PHP-CGI Start =======================================+".Log();
            proxy.Start();
        }

        /// <summary>
        /// 服务关闭
        /// </summary>
        protected override void OnStop()
        {
            try
            {
                if (proxy.IsActive)
                {
                    proxy.Stop();
                    "PHP-CGI Stop =======================================-".Log();
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
