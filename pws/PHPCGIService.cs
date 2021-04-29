using System;
using System.ServiceProcess;

namespace Pws
{
    /// <summary>
    /// 服务
    /// </summary>
    public partial class PhpCgiService : ServiceBase
    {
        private PhpCgiServerProxy proxy;

        /// <summary>
        /// 服务初始化
        /// </summary>
        public PhpCgiService()
        {
            "PHP-CGI Server Initialize =======================================*".Log();
            proxy = new PhpCgiServerProxy();
        }

        /// <summary>
        /// 服务开始
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            try
            {
                "PHP-CGI Start =======================================+".Log();
                proxy.Start();
            }
            catch (Exception e)
            {
                e.ToString().Log();
            }
        }

        /// <summary>
        /// 服务关闭
        /// </summary>
        protected override void OnStop()
        {
            try
            {
                proxy.Stop();
                "PHP-CGI Stop =======================================-".Log();
            }
            catch (Exception e)
            {
                e.ToString().Log();
            }
            LogExtends.Finally();
        }
    }
}
