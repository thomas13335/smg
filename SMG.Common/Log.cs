using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMG.Common
{
    /// <summary>
    /// Writes log file.
    /// </summary>
    class Log
    {
        public static void Trace(string format, params object[] args)
        {
            Debug.WriteLine(format, args);
        }

        public static void Warning(string format, params object[] args)
        {
            Trace(format, args);
        }

        public static void TraceGateOp2(object a, IGate r, string format, params object[] args)
        {
            var text = string.Format(format, args);
            var line = a.ToString().PadRight(46) + " " + text.PadRight(20) + " ==> " + r.ToString();
            Trace("    {0}", line);
        }

        public static void TraceGateOp3(IGate a, IGate b, IGate r, string format, params object[] args)
        {
            var text = string.Format(format, args);
            var line = a.ToString().PadRight(21) + " || " + b.ToString().PadRight(21) + " " + text.PadRight(20) + " ==> " + r.ToString();
            Trace("    {0}", line);
        }

        [Conditional("VERBOSE")]
        public static void TraceGuard(IGate gate1, IGate gate2, string p)
        {
            if (TraceFlags.ShowGuard)
            {
                TraceGateOp2(gate1, gate2, p);
            }
        }

        [Conditional("VERBOSE")]
        public static void TraceGuard(string format, params object[] args)
        {
            if (TraceFlags.ShowGuard)
            {
                Trace(format, args);
            }
        }


    }
}
