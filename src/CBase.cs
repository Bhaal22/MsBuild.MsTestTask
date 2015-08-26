using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace msbuild.tasks.MsTest
{
    public class CBase
    {
        private string _pattern = "*";
        private IBuildEngine buildEngine;
        private ITaskHost hostObject;
        private static readonly char[] BAD_CHARS = new char[]{ ' ', '\t', '"' };

        #region MsBuild
        protected void Debug(string p)
        {
            this.GenericMSBuildLog(p, MessageImportance.Low);
        }
        protected void Log(string p)
        {
            this.GenericMSBuildLog(p, MessageImportance.Normal);
        }
        protected void Warn(string p)
        {
            this.GenericMSBuildLog(p, MessageImportance.High);
        }
        private void GenericMSBuildLog(string message, MessageImportance i)
        {
            this.BuildEngine.LogMessageEvent(new BuildMessageEventArgs(message, string.Empty, "mstest", i));
        }
        #endregion

        public string Pattern
        {
            get
            {
                return this._pattern;
            }
            set
            {
                this._pattern = value;
            }
        }
        public string SearchLocation { get; set; }

        [Output]
        public string ResultFile { get; private set; }

        public IBuildEngine BuildEngine
        {
            get
            {
                return this.buildEngine;
            }
            set
            {
                this.buildEngine = value;
            }
        }
        public ITaskHost HostObject
        {
            get
            {
                return this.hostObject;
            }
            set
            {
                this.hostObject = value;
            }
        }
        
        private string[] containers()
        {
            return Directory.GetFiles(this.SearchLocation, this._pattern);
        }
        
        private List<string> containersToArgs()
        {
            string[] array = this.containers();
            List<string> list = new List<string>();
            string[] array2 = array;
            for (int i = 0; i < array2.Length; i++)
            {
                string path = array2[i];
                list.Add(string.Format("/testcontainer:{0}", Path.Combine(this.SearchLocation, path)));
            }
            return list;
        }

        protected bool Run()
        {
            ResultFile = string.Format(@"TestResults\Gsx.Monitor.Unit.Tests.Report.{0}.trx", DateTime.Now.ToString());
            return this._Run("mstest.exe") == 0;
        }
        private int _Run(string processName)
        {
            int result;
            try
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo("mstest.exe");

                List<string> list = this.containersToArgs();
                list.Add(string.Format("/resultsfile:{0}", ResultFile));

                var args = string.Join(" ", list);

                processStartInfo.Arguments = args;
                processStartInfo.UseShellExecute = false;
                processStartInfo.RedirectStandardOutput = true;
                processStartInfo.RedirectStandardError = true;
                this.Debug(string.Format("running {0} with args {1}", processName, processStartInfo.Arguments));
                
                using (Process process = Process.Start(processStartInfo))
                {
                    char[] array = new char[Console.BufferWidth];
                    int i = 1;
                    while (i > 0)
                    {
                        i = process.StandardOutput.Read(array, 0, array.Length);
                        string p = (i > 0) ? new string(array, 0, i) : null;
                        this.Log(p);
                    }
                    process.WaitForExit();
                    result = process.ExitCode;
                }
            }
            catch (Exception ex)
            {
                this.Warn(string.Format("MsTest execution failed because of: '{0}'", ex.ToString()));
                result = -1;
            }
            return result;
        }
        private string MaybeQuote(string arg)
        {
            string result;
            if (arg.IndexOfAny(CBase.BAD_CHARS) >= 0)
            {
                if (arg.IndexOf('"') >= 0)
                {
                    result = "'" + arg + "'";
                }
                else
                {
                    result = "\"" + arg + "\"";
                }
            }
            else
            {
                result = arg;
            }
            return result;
        }
    }
}
