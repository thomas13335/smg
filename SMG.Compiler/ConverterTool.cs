﻿using SMG.Common;
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
    public class ConverterTool
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

                var sm = new StateMachine();
                var scanner = new Scanner(fullpath);
                var parser = new Parser(scanner);
                parser.SM = sm;

                try
                {
                    parser.Parse();

                    if (parser.errors.count > 0)
                    {
                        throw new Exception(parser.errors.ToString());
                    }
                }
                catch (Exception ex)
                {
                    var msg = fullpath + "(" + scanner.Line + "," + scanner.Column + "): error: " + ex.Message;

                    Print("{0}", msg);
                    result = 1;
                }

                if (0 == result)
                {

                    sm.CalculateDependencies();
                    var code = sm.GenerateCode();

                    using (var writer = new StreamWriter(outputfile))
                    {
                        writer.WriteLine(code);
                    }

                    Print("smg: code file '{0}' generated.", outputfile);
                }
                else
                {
                    Print("smg: parsing failed.");
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
