using SMG.Common.Code;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SMG.UnitTest
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Contains("all"))
            {
                return RunAll();
            }
            else
            {
                try
                {
                    return RunAll();
                }
                catch(AggregateException ex)
                {
                    Debug.WriteLine(ex.InnerExceptions.Count + " error(s)");
                    return 1;
                }
            }
        }

        private static int RunOne()
        {
            var test = new UnitTest1();
            test.SMG_05_04_SyntaxCases();
            return 0;
        }

        private static int RunAll()
        {
            var test = new UnitTest1();
            var testtype = test.GetType();
            var methods = testtype.GetMethods()
                .Where(m => m.IsPublic && m.GetParameters().Length == 0 && !m.IsStatic && m.DeclaringType == testtype);

            var sb = new StringBuilder();
            int errors = 0;

            foreach(var method in methods)
            {
                var name = method.Name;
                var success = false;

                try
                {
                    GateCache.Instance.Purge();

                    method.Invoke(test, new object[0]);
                    success = true;
                }
                catch(Exception ex)
                {
                    while (ex is TargetInvocationException) ex = ex.InnerException;
                    Console.WriteLine("error: " + ex.Message);
                    errors++;
                }

                sb.AppendFormat("{0,-30} {1}", name, success ? "OK" : "FAIL");
                sb.AppendLine();
            }

            if(0 == errors)
            {
                sb.AppendLine("\n\t*** success ***");
            }
            else
            {
                sb.AppendFormat("\n\t{0} error(s)", errors);
                sb.AppendLine();
            }

            Console.WriteLine(sb.ToString());

            return errors;
        }
    }
}
