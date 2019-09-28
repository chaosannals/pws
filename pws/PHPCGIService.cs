using System;
using System.ServiceProcess;
using System.IO;
using System.Diagnostics;
using System.Timers;

namespace pws
{
    public partial class PHPCGIService : ServiceBase
    {
        private Process process;
        private Timer timer;

        public PHPCGIService()
        {
            InitializeComponent();
            string here = AppDomain.CurrentDomain.BaseDirectory;
            process = new Process();
            process.StartInfo.FileName = Path.Combine(here, "php-cgi.exe");
            process.StartInfo.WorkingDirectory = here;
            process.StartInfo.Arguments = "-b 9000";
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.EnableRaisingEvents = true;
            process.OutputDataReceived += new DataReceivedEventHandler(Output);
            timer = new Timer();
            timer.Elapsed += new ElapsedEventHandler(Ensure);
            timer.Interval = 2000;
            timer.Enabled = true;
        }

        protected override void OnStart(string[] args)
        {
            WriteLog("PHP-CGI Start");
            process.Start();
            process.BeginOutputReadLine();
            timer.Start();
        }

        protected override void OnStop()
        {
            timer.Stop();
            process.Kill();
            process.Close();
            WriteLog("PHP-CGI Stop");
        }

        private void Ensure(object sender, ElapsedEventArgs args)
        {
            if (process.HasExited)
            {
                WriteLog("PHP-CGI Restart");
                process.Start();
            }
        }

        private void Output(object sender, DataReceivedEventArgs args)
        {
            if (!string.IsNullOrEmpty(args.Data))
            {
                WriteLog(args.Data);
            }
        }

        private void WriteLog(string msg)
        {
            string here = AppDomain.CurrentDomain.BaseDirectory;
            string path = Path.Combine(here, "service.log");
            FileInfo file = new FileInfo(path);
            if (!file.Exists)
            {
                File.Create(path).Close();
            }
            using (FileStream stream = new FileStream(path, FileMode.Append, FileAccess.Write))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.WriteLine(DateTime.Now.ToString() + "   " + msg);
                }
            }
        }
    }
}
