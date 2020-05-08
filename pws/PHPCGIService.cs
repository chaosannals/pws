using System;
using System.ServiceProcess;

namespace pws
{
    /// <summary>
    /// 
    /// </summary>
    public partial class PhpCgiService : ServiceBase
    {
        private PhpCgiServerProxy proxy;

        public PhpCgiService()
        {
            "服务初始化".Log();
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
                "PHP-CGI Start".Log();
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
            proxy.Stop();
            "PHP-CGI Stop".Log();
        }
    }
}
