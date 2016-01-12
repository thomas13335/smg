using SMG.Common;
using SMG.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMG.Compiler
{
    /// <summary>
    /// Converts a smg source file into target source code.
    /// </summary>
    public class ConverterTool : StateMachineCompiler
    {
        public bool CheckTimestamps { get; set; }

        #region Public Methods

        public static int Main(string[] args)
        {
            return new ConverterTool().Execute(args);
        }

        public int Execute(string[] args)
        {
            int rc = 1;

            if (!args.Any())
            {
                PrintLogo();
                PrintSyntax();
            }
            else
            {
                foreach (var arg in args)
                {
                    if (arg == "-t")
                    {
                        CheckTimestamps = true;
                    }
                    else
                    {
                        rc = ProcessFile(arg);
                        if (0 != rc)
                        {
                            return rc;
                        }
                    }
                }
            }

            return rc;
        }

        #endregion

        #region Private Methods

        private void Print(string format, params object[] args)
        {
            var msg = string.Format(format, args);
            Console.WriteLine(msg);
        }

        private int ProcessFile(string filename)
        {
            int result = 0;
            try
            {
                var fullpath = Path.GetFullPath(filename);
                var outputfile = fullpath + ".cs";

                if(File.Exists(outputfile) && CheckTimestamps)
                {
                    var texisting = File.GetLastWriteTime(outputfile);
                    var tsource = File.GetLastWriteTime(fullpath);

                    if(tsource <= texisting)
                    {
                        Print("smg source '{0}' skipped.", fullpath);
                        return 0;
                    }
                }

                Print("smg: processing '{0}' ...", fullpath);

                var clock = new Stopwatch();
                clock.Start();

                SM.SourceFile = fullpath;

                using (var stream = File.OpenRead(fullpath))
                {
                    CompileStream(stream);
                }

                GenerateCode();

                using (var writer = File.CreateText(outputfile))
                {
                    writer.Write(Output);
                }

                Print("smg: output '{0}' ({1:0.000}s).", outputfile, clock.Elapsed.TotalSeconds);
            }
            catch (AggregateException ex)
            {
                foreach (var subex in ex.InnerExceptions.OfType<CompilerException>())
                {
                    // allow to click on it in DEVENV
                    Print("{0}: error {1}", subex.Location, subex.Message);
                }
            }
            catch (Exception ex)
            {
                Print("smg: error: {0}", ex.Message);
            }

            return result;
        }

        private void PrintLogo()
        {
            Print("SMG State Machine Generator, v{0}\n", GetType().Assembly.GetName().Version.ToString(4));
        }

        private void PrintSyntax()
        {
            Print("translates a SMG-file into source code.\n");
            Print("syntax: smgc {{ <smg-file> }}");
        }

        #endregion
    }
}
