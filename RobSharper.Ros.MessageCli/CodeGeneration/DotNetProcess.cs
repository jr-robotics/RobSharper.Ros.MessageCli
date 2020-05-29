using System;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using CommandLine;

namespace RobSharper.Ros.MessageCli.CodeGeneration
{
    public class DotNetProcess
    {
        const string ProgramName = "dotnet";
        
        public static Process Execute(string command)
        {
            var proc = new Process
            {
                StartInfo =
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    FileName = ProgramName,
                    Arguments = command
                }
            };
            
            var procOutput = new StringBuilder();
            try
            {
                proc.OutputDataReceived += (s, e) => WriteOutput(e);
                proc.ErrorDataReceived += (s, e) => Colorful.Console.Error.WriteLine($"  {e.Data}");
                
                proc.Start();
                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();
                proc.WaitForExit();
            }
            catch (Exception e)
            {
                throw NewProcessFailedException(proc, procOutput, e);
            }
            
            if (!proc.HasExited || proc.ExitCode != 0)
            {
                throw NewProcessFailedException(proc, procOutput, null);
            }
            
            return proc;
        }

        private static void WriteOutput(DataReceivedEventArgs e)
        {
            if (e.Data == null)
                return;

            var color = Color.Gray;
            if (e.Data.Trim().StartsWith("error:", StringComparison.CurrentCultureIgnoreCase))
            {
                color = Color.Orange;
            }
            
            Colorful.Console.WriteLine($"  {e.Data}", color);
        }

        private static ProcessFailedException NewProcessFailedException(Process proc, StringBuilder procOutput,
            Exception exception)
        {
            var exitCode = proc.HasExited ? proc.ExitCode : 0;
            var processFailedException = new ProcessFailedException(proc.StartInfo.FileName, proc.StartInfo.Arguments,
                proc.HasExited, exitCode, procOutput.ToString(), exception);
            
            return processFailedException;
        }

        public static Process Build(string projectFilePath)
        {
            var command = $"build \"{projectFilePath}\" -c Release -v minimal";
            return Execute(command);
        }
    }
}