using SMG.Common;
using SMG.Common.Code;
using SMG.Common.Exceptions;
using SMG.Common.Generators;
using System;
using System.IO;
using System.Text;

namespace SMG.Compiler
{
    public class StateMachineCompiler
    {
        #region Private

        private StateMachine _sm = new StateMachine();
        private CodeParameters _parameters = new CodeParameters();
        private CodeGenerator _cg;
        private CodeWriter _writer;

        #endregion

        #region Properties

        public StateMachine SM { get { return _sm; } }

        public CodeParameters Parameters { get { return _parameters; } }

        public string Output { get { return null == _writer ? null : _writer.ToString(); } }

        #endregion

        public StateMachine CompileString(string text)
        {
            return CompileStream(MakeStream(text));
        }

        /// <summary>
        /// Compiles code into the statemachine.
        /// </summary>
        /// <param name="source">The SMG source code.</param>
        /// <returns>The current statemachine.</returns>
        public StateMachine CompileStream(Stream source)
        {
            var scanner = new Scanner(source);
            var parser = new Parser(scanner);
            parser.OnSyntaxError += HandleSyntaxError;
            parser.Parameters = Parameters;
            parser.SM = SM;

            try
            {
                parser.Parse();
            }
            catch(CompilerException ex)
            {
                SM.AddError(ex);
            }

            if(SM.IsFailed)
            {
                throw new AggregateException(SM.Errors);
            }

            return SM;
        }

        public void GenerateCode()
        {
            _writer = null;
            _cg = null;

            SelectCodeGenerator();
            SM.Calculate();
            _cg.Emit(SM);
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

        #region Private Methods

        private void HandleSyntaxError(object sender, SyntaxErrorEventArgs e)
        {
            var ce = e.Error as CompilerException;
            if(null != ce)
            {
                _sm.AddError(ce);
            }
            else
            {
            }
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

        private void EstablishCodeWriter()
        {
            if (null == _writer)
            {
                _writer = new CodeWriter();
            }
        }

        private void SelectCodeGenerator()
        {
            if (null == _cg)
            {
                EstablishCodeWriter();

                switch (Parameters.Language)
                {
                    case "C#":
                        _cg = new CSharpCodeGenerator(_writer);
                        break;

                    case "jscript":
                        _cg = new JScriptCodeGenerator(_writer);
                        break;

                    default:
                        throw new Exception("target language " + Parameters.Language + " is not supported.");
                }

                _cg.Parameters = Parameters;
            }
        }

        #endregion
    }
}
