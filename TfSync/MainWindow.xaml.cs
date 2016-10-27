using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TfSync
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        string _tfFileName;
        string _svnFileName;
        string _tortoiseSvnFileName;

        string _syncPath;
        string[] _workingPaths;

        object _stringLock = new object();

        int _isBusy = 0;

        public MainWindow()
        {
            InitializeComponent();

            _tfFileName = @"C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\TF.exe";
            _svnFileName = @"C:\Program Files\SlikSvn\bin\svn.exe";
            _tortoiseSvnFileName = @"C:\Program Files\TortoiseSVN\bin\TortoiseProc.exe";

            _syncPath = @"D:\Work\CITSECOS";
            _workingPaths = new string[] { @"C:\Users\Administrator\Desktop\新建文件夹", @"C:\Users\Administrator\Desktop\Service\CITSERP" };
        }

        void SyncToSvn()
        {
            // tf update
            // tf get /overwrite
            Log("tf update", true, true);
            Execute(_tfFileName, "get /overwrite");

            // svn commit
            // svn commit -m "s" D:\Work\CITSECOS
            //Log("svn commit", true, true);
            //Execute(_svnFileName, "commit -m \"s\" " + "\"" + _syncPath + "\"");

            // tortoiseSvn commit
            // tortoiseproc /command:commit /path:"" /logmsg:"" /closeonend:0
            Log("tortoise svn commit", true, true);
            Execute(_tortoiseSvnFileName, string.Format("/command:commit /logmsg:\"s\" /path:\"{0}\"", _syncPath));

            // svn update
            // svn up C:\Users\Administrator\Desktop\新建文件夹
            foreach (var path in _workingPaths)
            {
                Log("svn update " + path, true, true);
                Execute(_svnFileName, "up " + "\"" + path + "\"");
            }
        }

        void SyncToTf()
        {
            // svn update
            // svn up D:\Work\CITSECOS
            Log("svn update " + _syncPath, true, true);
            var updateResult = Execute(_svnFileName, "up " + "\"" + _syncPath + "\"");

            //var updateResult = "";
            //updateResult =
            //@"正在升级 'D:\Work\CITSECOS':
            //D    D:\Work\CITSECOS\test\新建文件夹\新建文本文档.txt
            //A    D:\Work\CITSECOS\test\新建文件夹\新建文本文档 (2).txt
            //A    D:\Work\CITSECOS\test\新建文件夹\新建文本文档(Updated File Name).txt
            //U    D:\Work\CITSECOS\test\新建文本文档.txt
            //更新到版本 132。";
            Debug.WriteLine(updateResult);

            var lines = updateResult.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            var fileName = "";
            foreach (var line in lines)
            {
                fileName = GetFileName(line);
                switch (line[0])
                {
                    case 'A':
                        // tf add D:\Work\CITSECOS\test\新建文件夹
                        Log("add " + fileName);
                        Execute(_tfFileName, "add " + "\"" + fileName + "\"");
                        break;
                    case 'U':
                        // TF.exe checkout D:\Work\CITSECOS\test\新建文本文档.txt
                        Log("checkout " + fileName);
                        Execute(_tfFileName, "checkout " + "\"" + fileName + "\"");
                        break;
                    case 'D':
                        // TF.exe delete D:\Work\CITSECOS\test\新建文件夹\新建文本文档.txt
                        Log("delete " + fileName);
                        Execute(_tfFileName, "delete " + "\"" + fileName + "\"");
                        break;
                }
            }

            // TF.exe checkin /comment:"<<< Committed By TFSync >>>" /notes:"代码审阅者"="zhaoxf" /noprompt
            //Execute(_tfFileName, "checkin /comment:\" <<< Committed By TFSync >>> \" /notes:\"代码审阅者\"=\"zhaoxf\" /noprompt");
            Execute(_tfFileName, "checkin /notes:\"代码审阅者\"=\"zhaoxf\" /noprompt");

        }

        string Execute(string fileName, string arguments)
        {
            var process = new Process();
            var processStartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            process.StartInfo = processStartInfo;
            process.Start();

            var outputStream = process.StandardOutput;

            var builder = new StringBuilder();
            var resultBuilder = new StringBuilder();

            var timer = new Timer(state =>
            {
                var stringBuilder = (StringBuilder)state;
                lock (_stringLock)
                {
                    if (stringBuilder.Length > 0)
                    {
                        Log(stringBuilder.ToString());
                        stringBuilder.Clear();
                    }
                }


            }, builder, 0, 500);

            var value = 0;
            while ((value = outputStream.Read()) != -1)
            {
                lock (_stringLock)
                {
                    builder.Append((char)value);
                }

                resultBuilder.Append((char)value);
            }

            lock (_stringLock)
            {
                timer.Dispose();
                Log(builder.ToString());
            }

            return resultBuilder.ToString();
        }

        void Log(string message, bool withTimeStamp = false, bool withTitleSeparateLine = false)
        {
            var builder = new StringBuilder();
            builder.AppendFormat("{0}{1}{2}", withTimeStamp ? Environment.NewLine + DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss.fff] ") : "", message,
                withTitleSeparateLine ?
                Environment.NewLine + "========================================================================" + Environment.NewLine :
                "");

            Dispatcher.BeginInvoke(new Action(() =>
            {
                TxtResult.AppendText(builder.ToString());
                TxtResult.ScrollToEnd();
            }));
        }

        string GetFileName(string logString)
        {
            if (!string.IsNullOrEmpty(logString))
            {
                if (char.IsLetter(logString[0]))
                {
                    for (int i = 1; i < logString.Length; i++)
                    {
                        if (!char.IsWhiteSpace(logString[i]))
                        {
                            return logString.Substring(i);
                        }
                    }
                }
                else
                {
                    return "";
                }
            }

            return "";
        }

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (Interlocked.CompareExchange(ref _isBusy, 1, 0) == 0)
            {
                ThreadPool.QueueUserWorkItem(state =>
                {
                    SyncToSvn();

                    //Execute(@"C:\Users\Administrator\Desktop\ConsoleApplication1.exe", "");
                    Interlocked.Exchange(ref _isBusy, 0);
                }, null);
            }

        }

        private void BtnCommit_Click(object sender, RoutedEventArgs e)
        {
            if (Interlocked.CompareExchange(ref _isBusy, 1, 0) == 0)
            {
                ThreadPool.QueueUserWorkItem(state =>
                {
                    Debug.WriteLine("starting! " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                    SyncToTf();
                    Debug.WriteLine("completed! " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));

                    Interlocked.Exchange(ref _isBusy, 0);
                }, null);
            }

        }
    }
}
