using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace SVNGetTheLastRes
{
    public partial class Form1
    {
        /// <summary>
        /// 
        /// </summary>
        public Form1()
        {
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                //查找最近更新文件，并将命令返回结果输出至txt文件
                Execute("svnlook changed D:/subversion/project1 > D:/Subversion/project1/hooks/test.txt");

                //读取生成的文件
                string strPath = ResumeTxt("D:/Subversion/project1/hooks/test.txt");

                //文件内容处理：按换行符将读取的字符串转换成字符串数组
                string[] aryPath = strPath.Split('\n');

                //循环更新文件
                for (int i = 0; i < aryPath.Length; i++)
                {
                    //处理掉回车符
                    aryPath[i].Replace('\r', ' ');

                    //经测试，文件中最后一行是空行，但为了避免遗漏，用非空判断，而不是循环的length-1
                    if (!aryPath[i].Trim().Equals(""))
                    {
                        //根据文件中的数据格式，从第五个字符开始才是文件路径
                        string strFile = aryPath[i].Trim().Substring(4);
                        //组织命令并执行，其中D:/是项目所在文件夹，根据自己的情况组织
                        string strCmd = "svn update D:/" + strFile + " --username *** --password ***";
                        Execute(strCmd);
                    }
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {

            }
        }

        public string ResumeTxt(string path)
        {
            string str = string.Empty;

            StreamReader reader = new StreamReader(path, System.Text.Encoding.Default);
            str = reader.ReadToEnd();

            //再通过查询解析出来的的字符串有没有GB2312的字段，来判断是否是GB2312格式的，如果是，则重新以GB2312的格式解析
            Regex reGB = new Regex("GB2312", RegexOptions.IgnoreCase);
            Match mcGB = reGB.Match(str);
            if (mcGB.Success)
            {
                StreamReader reader2 = new StreamReader(path, System.Text.Encoding.GetEncoding("GB2312"));
                str = reader2.ReadToEnd();
            }

            return str;
        }

        /// <summary>
        /// 执行DOS命令并返回结果
        /// </summary>
        /// <param name="dosCommand">Dos命令语句</param>
        /// <returns>DOS命令返回值</returns>
        public string Execute(string dosCommand)
        {
            return Execute(dosCommand, 0);
        }

        /// <summary> 
        /// 执行DOS命令，返回DOS命令的输出
        /// </summary> 
        /// <param name="dosCommand">dos命令</param> 
        /// <param name="milliseconds">等待命令执行的时间（单位：毫秒），如果设定为0，则无限等待</param> 
        /// <returns>返回DOS命令的输出</returns> 
        public static string Execute(string dosCommand, int seconds)
        {
            string output = ""; //输出字符串
            if (dosCommand != null && dosCommand != "")
            {
                Process process = new Process();//创建进程对象
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = "cmd.exe";//设定需要执行的命令
                startInfo.Arguments = "/C " + dosCommand;//设定参数，其中的“/C”表示执行完命令后马上退出
                startInfo.UseShellExecute = false;//不使用系统外壳程序启动
                startInfo.RedirectStandardInput = false;//不重定向输入
                startInfo.RedirectStandardOutput = true; //重定向输出
                startInfo.CreateNoWindow = true;//不创建窗口
                process.StartInfo = startInfo;
                try
                {
                    if (process.Start())//开始进程
                    {
                        if (seconds == 0)
                        {
                            process.WaitForExit();//这里无限等待进程结束
                        }
                        else
                        {
                            process.WaitForExit(seconds); //这里等待进程结束，等待时间为指定的毫秒
                        }
                        output = process.StandardOutput.ReadToEnd();//读取进程的输出
                    }
                }
                catch
                {

                }
                finally
                {
                    if (process != null)
                        process.Close();
                }
            }
            return output;
        }
    }
}