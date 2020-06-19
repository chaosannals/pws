using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Threading;

namespace Pws
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        static void Main()
        {
            ThreadPool.SetMaxThreads(10, 10); // 线程数量
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                "捕获了漏掉的异常".Log();
                e.ExceptionObject.ToString().Log();
            };
            AppDomain.CurrentDomain.ProcessExit += (sender, e) => {
                LogExtends.Write();
            };
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new PhpCgiService()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
