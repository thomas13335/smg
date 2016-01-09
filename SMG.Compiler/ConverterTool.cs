using SMG.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMG.Compiler
{
    public class ConverterTool
    {
        private void Trace(string format, params object[] args)
        {
            var msg = string.Format(format, args);
            Console.WriteLine(msg);
        }

        public static int Main(string[] args)
        {
            return new ConverterTool().Execute(args);
        }

        public int Execute(string[] args)
        {
            int rc = 1;

            if (!args.Any())
            {
                Trace("no arguments specified.");
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

                    Trace("{0}", msg);
                    result = 1;
                }

                if (0 == result)
                {

                    sm.CalculateTriggerGuardDependencies();
                    var code = sm.GenerateCode();

                    using (var writer = new StreamWriter(outputfile))
                    {
                        writer.WriteLine(code);
                    }

                    Trace("code file '{0}' generated.", outputfile);
                }
                else
                {
                    Trace("parsing failed.");
                }
            }
            catch (Exception ex)
            {
                Trace("error: {0}", ex.Message);
            }

            return result;
        }
    }
}
