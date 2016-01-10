using SMG.Common;
using SMG.Common.Exceptions;
using System;
using System.Collections.Generic;
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
                    rc = ProcessFile(arg);
                    if (0 != rc)
                    {
                        return rc;
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

                Print("generated file '{0}'.", outputfile);
            }
            catch (AggregateException ex)
            {
                foreach (var subex in ex.InnerExceptions.OfType<CompilerException>())
                {
                    Print("{0}: error {1}", subex.Location, subex.Message);
                }
            }
            catch (Exception ex)
            {
                Print("error: {0}", ex.Message);
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
