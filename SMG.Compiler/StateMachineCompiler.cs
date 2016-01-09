using SMG.Common;
using System;
using System.IO;
using System.Text;

namespace SMG.Compiler
{
    public class StateMachineCompiler
    {
        private StateMachine _sm = new StateMachine();

        public StateMachine SM { get { return _sm; } }

        public StateMachine CompileString(string text)
        {
            var ms = MakeStream(text);
            var scanner = new Scanner(ms);
            var parser = new Parser(scanner);
            parser.SM = _sm;

            parser.Parse();

            if (parser.errors.count > 0)
            {
                throw new Exception("state machine compiler parser error: " + parser.errors);
            }

            return _sm;
        }

        private static MemoryStream MakeStream(string text)
        {
            var ms = new MemoryStream();
            using (var writer = new StreamWriter(ms, Encoding.UTF8, 0x10000, true))
            {
                writer.Write(text);
            }

            ms.Position = 0;
            return ms;
        }

        public ICondition EvaluateCondition(string text)
        {
            var ms = MakeStream("EVAL " + text);

            var scanner = new Scanner(ms);
            var parser = new Parser(scanner);
            parser.SM = _sm;

            parser.Parse();

            if (parser.errors.count > 0)
            {
                throw new Exception("failed to parse condition: " + parser.errors);
            }

            return parser.Result;
        }
    }
}
