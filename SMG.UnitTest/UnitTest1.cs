using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using SMG.Common.Gates;
using SMG.Common;
using SMG.Common.Conditions;
using SMG.Common.Transitions;
using SMG.Compiler;
using System.Diagnostics;
using SMG.Common.Types;
using System.Text;
using System.Collections.Generic;
using SMG.Common.Code;
using SMG.Common.Effects;
using SMG.Common.Exceptions;
using Microsoft.CSharp;
using System.CodeDom.Compiler;

namespace SMG.UnitTest
{
    [TestClass]
    public class UnitTest1
    {
        #region Private

        public static void Trace(string format, params object[] args)
        {
            Debug.WriteLine(format, args);
        }

        private void PrintEventEffectConditions(StateMachine sm)
        {
            foreach (var e in sm.Events)
            {
                Trace(" {0,-26} : {1}", e.Name, e.PreCondition);

                foreach (var f in e.EffectsBefore.GetEffectConditions())
                {
                    Trace("  {0,-19} leave : {1}", f.Effect, f.PreCondition);

                    foreach(var z in f.Elements)
                    {
                        Trace("    {0} => {1}", z.PreCondition, z.PostCondition);
                    }
                }

                foreach (var f in e.EffectsAfter.GetEffectConditions())
                {
                    Trace("  {0,-19} enter : {1}", f.Effect, f.PreCondition);

                    foreach (var z in f.Elements)
                    {
                        Trace("    {0} => {1}", z.PreCondition, z.PostCondition);
                    }

                }
            }
        }

        private void ValidateEventEffectConditionPre(StateMachine sm, string eventname, string methodname, string ctext)
        {
            var ev = sm.Events.Where(e => e.Name == eventname).First();
            var ec = ev.EffectsAfter.GetEffectCondition(new CallEffect(sm, methodname));

            var eff = ec.PreCondition.ToString();

            if (ctext != eff)
            {
                Trace("unexpected condition: {0}", eff);
                Assert.AreEqual(ctext, eff);
            }
        }

        private string ReadEmbeddedScript(string name)
        {
            var path = "SMG.UnitTest.Scripts." + name;
            using (var s = GetType().Assembly.GetManifestResourceStream(path))
            {
                if (null == s)
                {
                    throw new Exception("embedded resource '" + path + "' not found.");
                }
                using (var r = new StreamReader(s))
                {
                    return r.ReadToEnd();
                }
            }
        }


        private void PrintMonitorTransitions(TransitionMonitor trigger)
        {
            Trace("{0}", trigger);

            //foreach (var x in trigger.Transitions)
            {
                var c = trigger.PreCondition;
                Trace("  {0}", c);
                /*Trace("  bad result:");
                foreach (var vc in x.PreCondition.SelectAll().OfType<IVariableCondition>())
                {
                    Trace("    {0}", vc);
                    foreach (var y in vc.GetTransitions())
                    {
                        Trace("      {0}", y);
                    }
                }
                Trace("  good result:");*/
                foreach (var y in trigger.Transitions.GetTransitions(c))
                {
                    Trace("      {0}", y);

                }
            }
        }

        class SimpleCodeLabelEvaluator : ICodeLabelEvaluator
        {
            public void EmitCodeLabelAssignment(string label, IGate gate)
            {
                Trace("evaluate {0} => {1}", label, gate);
            }
        }

        #endregion

        [TestMethod]
        public void SMG_01_01_BooleanExpression()
        {
            var eval = new StateMachineCompiler();
            eval.CompileString("smg test declare BOOLEAN A, B, C");
            var c = eval.EvaluateCondition("A AND NOT B").Decompose(ConditionMode.Pre);
            Assert.AreEqual("A!B", c.ToString());
        }

        [TestMethod]
        public void SMG_01_02_BooleanRules()
        {
            var eval = new StateMachineCompiler();

            var dict = new Dictionary<string, string>();

            // fix static variable order
            var sb = new StringBuilder();
            sb.Append("SMG test DECLARE BOOLEAN ");
            for (char c = 'A'; c <= 'Z'; ++c)
            {
                if (c != 'A') sb.Append(",");
                sb.Append(c);
            }

            eval.CompileString(sb.ToString());

            dict.Add("A AND B", "AB");
            dict.Add("A OR B", "A + B");
            dict.Add("A AND NOT A", "0");
            dict.Add("A OR NOT A", "1");
            dict.Add("A AND NOT A AND B", "0");
            dict.Add("A OR (A AND B)", "A");
            dict.Add("A OR (NOT A AND B)", "A + B");

            dict.Add("(A AND B AND C) OR (B AND C)", "BC");
            dict.Add("(A AND B AND C) OR (A AND B AND NOT C)", "AB");

            // check ordering by address
            dict.Add("M OR NOT ((X AND Y) OR Z) OR (A AND B) OR F", "AB + F + M + !X!Z + !Y!Z");

            foreach (var pair in dict)
            {
                Trace("\n--- testing {0} ...", pair.Key);
                var cond = eval.EvaluateCondition(pair.Key).Decompose(ConditionMode.Pre);
                Assert.AreEqual(pair.Value, cond.ToString());
            }
        }

        [TestMethod]
        public void SMG_01_03_ConditionTransitions()
        {
            TraceFlags.ShowDepencencyAnalysis = true;
            var cc = new StateMachineCompiler();
            var sm = cc.CompileString(ReadEmbeddedScript("TransitionCondition.smg"));
            cc.GenerateCode();

            PrintEventEffectConditions(sm);

            Trace("{0}", cc.Output);

        }

        [TestMethod]
        public void SMG_01_04_ConditionTransitions()
        {
            TraceFlags.ShowDepencencyAnalysis = true;
            // TraceFlags.ShowLabel = true;

            var cc = new StateMachineCompiler();
            var sm = cc.CompileString(ReadEmbeddedScript("ConditionTransitions.smg"));
            foreach(var trigger in sm.Events.SelectMany(e => e.Triggers))
            {
                PrintMonitorTransitions(trigger);
            }

            foreach (var guard in sm.Guards)
            {
                PrintMonitorTransitions(guard);
            }

            // Trace("{0}", GateCache.Instance.ToDebugString());

            //cc.GenerateCode();
            sm.Calculate();

            PrintEventEffectConditions(sm);
        }

        [TestMethod]
        public void SMG_03_StateCondition()
        {
            TraceFlags.ShowDepencencyAnalysis = true;

            var sm = new StateMachine();
            var stype = new SimpleStateType("State");
            stype.AddStateNames(new[] { "A", "B", "C" });
            var s = sm.AddVariable("s", stype);
            var t = sm.AddVariable("t", stype);

            var c = new StateCondition(s);
            c.SetPreStates("A,B");
            c.SetPostStates("C");

            var e1 = sm.AddEvent("e1");

            var trigger = new Trigger(e1, c);
            sm.AddTrigger(trigger);

            var gc1 = new StateCondition(s);
            gc1.SetPreStates("A");
            // gc1.SetPostStates("C");
            sm.AddGuard(gc1, GuardType.LEAVE, "g1");

            var gc2 = new StateCondition(s);
            gc2.SetPreStates("C");
            sm.AddGuard(gc2, GuardType.ENTER, "g2");

            {
                var c1 = new StateCondition(t);
                c1.SetPreStates("A");

                var c2 = new StateCondition(s);
                c2.SetPreStates("A");
            }

            sm.Calculate();

            // sm.Dump();

            var e = sm.Events.First();
            Assert.AreEqual("e1", e.Name);
            Assert.AreEqual(2, e.Triggers.Count);

            var t1 = e.Triggers[0];
            var t2 = e.Triggers[1];

            Assert.AreEqual(2, t1.Guards.Count);
            Assert.AreEqual(1, t2.Guards.Count);

            Assert.IsTrue(t1.Guards.Any(g => g.Name == "g1"));
            Assert.IsTrue(t1.Guards.Any(g => g.Name == "g2"));
            Assert.IsTrue(t2.Guards.Any(g => g.Name == "g2"));
        }

        [TestMethod]
        public void SMG_04_00_Basic()
        {
            var cc = new StateMachineCompiler();
            cc.CompileString(
                "SMG BasicTest " +
                "DECLARE State (a, b) s, t " +
                "DECLARE BOOLEAN f " +
                "TRIGGER e1 WHEN s(a => b) " +
                "TRIGGER e2 WHEN t(a => b) " +
                "GUARD WHEN ENTER t(b) AND s(b) AND f CALL q1 " +
                "TRIGGER e4 WHEN f(0 => 1) " +
                ""
                );

            var sm = cc.SM;
            sm.Calculate();

            PrintEventEffectConditions(sm);

            ValidateEventEffectConditionPre(sm, "e1", "q1", "s(a)t(b)f");
            ValidateEventEffectConditionPre(sm, "e2", "q1", "s(b)t(a)f");
            ValidateEventEffectConditionPre(sm, "e4", "q1", "s(b)t(b)!f");
        }

        [TestMethod]
        public void SMG_04_01_EnterLeaveGuards()
        {
            var cc = new StateMachineCompiler();
            cc.CompileString(
                "SMG test \n" +
                "DECLARE State (a, b) s, t, x\n" +
                "TRIGGER e1 WHEN s(a => b) " +
                "TRIGGER e2 WHEN t(a => b) " +
                "TRIGGER e3 WHEN s(a => b) AND t(a => b) " +
                "GUARD WHEN ENTER t(b) AND s(b) CALL q1 " +
                "GUARD WHEN LEAVE t(a) AND s(a) CALL q2 " +
                ""
                );

            var sm = cc.SM;
            sm.Calculate();

            ValidateEventEffectConditionPre(sm, "e1", "q1", "s(a)t(b)");
            ValidateEventEffectConditionPre(sm, "e2", "q1", "s(b)t(a)");
            ValidateEventEffectConditionPre(sm, "e3", "q1", "s(a)t(a)");
        }

        [TestMethod]
        public void SMG_04_03_CodeLabels()
        {
            GateCache.Instance.Purge();

            var cc = new StateMachineCompiler();
            var sm = cc.CompileString(
                "SMG test " +
                "DECLARE State (a, b, c) s, t " +
                "TRIGGER e WHEN s(a => b) AND t(b) " +
                "GUARD WHEN s(* => b) AND t(b) CALL m1 " +
                ""
                );

            sm.Calculate();

            PrintEventEffectConditions(sm);

            var ev = sm.Events.Where(e => e.Name == "e").First();

            using (var gc = new GateConverter())
            {
                ev.EffectsAfter.Schedule(gc);

                var cle = new SimpleCodeLabelEvaluator();

                // Trace("gc: \n{0}", gc.ToDebugString());
                gc.Emit(cle, 0);
            }

            var eclist = ev.EffectsAfter.GetEffectConditions();
            var ec = eclist.Where(a => a.Effect.UniqueID == "CALL m1").First();

            Assert.AreEqual("<_c0><_c1>", ec.ConditionLabel.ToString());

            /*var sb = new StringBuilder();
            using (var w = new StringWriter(sb))
            {
                w.Write(sm.GenerateCode());
            }

            Trace("code:\n{0}", sb);*/
        }

        [TestMethod]
        public void SMG_04_04_ConflictingTriggers()
        {
            TraceFlags.ShowDepencencyAnalysis = true;
            var cc = new StateMachineCompiler();

            try
            {
                var sm = cc.CompileString(
                    "SMG test " +
                    "DECLARE State (a, b, c) s, t " +
                    "TRIGGER e1 WHEN s(a => b) OR s(a => c) CALL m1" +
                    ""
                    );

                Assert.Fail("exception expected.");
            }
            catch(Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(AggregateException));
                ex = ex.InnerException;

                Assert.IsInstanceOfType(ex, typeof(CompilerException));
                var cex = (CompilerException)ex;

                Assert.AreEqual(ErrorCode.AmbigousPostCondition, cex.Code);
            }
        }

        [TestMethod]
        public void SMG_04_05_SimplyifyBool()
        {
            var sm = new StateMachine();
            var btype = sm.GetStateType("boolean");
            var v = sm.AddVariable("v", btype);
            var u = sm.AddVariable("u", btype);
            var w = sm.AddVariable("w", btype);

            var c1 = new BooleanCondition(v);
            var c2 = new BooleanCondition(u);
            var c3 = new BooleanCondition(w);

            IGate g;

            g = Gate.ComposeAND(c1, c2);
            g = Gate.ComposeOR(g, Gate.Invert(c3));

            Trace("{0}", g);

            Assert.AreEqual("vu + !w", g.ToString());
        }

        [TestMethod]
        public void SMG_04_05_Simplyify()
        {
            var sm = new StateMachine();
            var stype = sm.AddSimpleType("S");
            stype.AddStateNames(new[] { "A", "B", "C", "D", "X" });
            var v = sm.AddVariable("v", stype);
            var u = sm.AddVariable("u", stype);

            // A + B
            var c1 = new StateCondition(v);
            c1.SetPreStates(new[] { 0, 1 });

            // D
            var c2 = new StateCondition(v);
            c2.SetPreStates(new[] { 3 });

            // A
            var c3 = new StateCondition(v);
            c2.SetPreStates(new[] { 0 });

            var c1dc = Gate.ComposeAND(Gate.Invert(c1.Decompose(ConditionMode.Pre)), Gate.Invert(c2.Decompose(ConditionMode.Pre)));

            Trace("1 === {0}", c1dc);

            return;
        }

        /// <summary>
        /// Validates some standard constructions.
        /// </summary>
        [TestMethod]
        public void SMG_04_06_StandardConditions()
        {
            // TraceFlags.ShowDepencencyAnalysis = true;

            var cc = new StateMachineCompiler();
            var smgtext = ReadEmbeddedScript("StandardConditions.smg");
            var sm = cc.CompileString(smgtext);

            sm.Calculate();


            ValidateEventEffectConditionPre(sm, "e1", "ge1", "s(a) + t(a)");
            ValidateEventEffectConditionPre(sm, "e1", "gprod", "s(a) + t(a)");
            ValidateEventEffectConditionPre(sm, "e1", "gsum", "s(a) + t(a)");

            var ce1 = "s(a)t(a)";
            ValidateEventEffectConditionPre(sm, "e2", "ge2", ce1);
            ValidateEventEffectConditionPre(sm, "e2", "gprod", ce1);
            ValidateEventEffectConditionPre(sm, "e2", "gsum", ce1);

            ValidateEventEffectConditionPre(sm, "e3", "gsum", "0");
            ValidateEventEffectConditionPre(sm, "e4", "gsum", "0");

            PrintEventEffectConditions(sm);
        }

        public void SMG_04_06_SumGuard()
        {
            var cc = new StateMachineCompiler();
            var smgtext = ReadEmbeddedScript("SumGuard.smg");
            var sm = cc.CompileString(smgtext);

            sm.Calculate();

            PrintEventEffectConditions(sm);

            cc.GenerateCode();
            Trace("{0}", cc.Output);
        }

        [TestMethod]
        public void SMG_04_07_MixedConditions()
        {
            TraceFlags.ShowDepencencyAnalysis = true;

            var cc = new StateMachineCompiler();
            var smgtext = ReadEmbeddedScript("MixedConditions.smg");
            var sm = cc.CompileString(smgtext);

            sm.Calculate();

            ValidateEventEffectConditionPre(sm, "e1", "m1", "s(a)t(a)");
        }

        [TestMethod]
        public void SMG_04_08_SyntaxErrors()
        {
            var cc = new StateMachineCompiler();
            cc.SM.SourceFile = "testfile.smg";

            try
            {
                var sm = cc.CompileString("SMG Fails TRIGGER x AND TRIGGER y ");
                Assert.Fail("exception expected.");
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(AggregateException));
                var cex = (CompilerException)ex.InnerException;
                Assert.AreEqual(ErrorCode.SyntaxError, cex.Code);
                Assert.AreEqual(cc.SM.SourceFile, cex.Location.SourceFile);
            }
        }

        /// <summary>
        /// Compiles and executes a statemachine.
        /// </summary>
        [TestMethod]
        public void SMG_05_01_CodeGeneration()
        {
            var cc = new StateMachineCompiler();
            cc.CompileString(ReadEmbeddedScript("CodeGeneration.smg"));

            cc.GenerateCode();
            Assert.AreEqual(2, cc.SM.Events.Count());

            var u = cc.SM.AddEvent("u");
            var trigger = new Trigger(u, cc.EvaluateCondition("s(b => a)"));
            trigger.AddEffects(new[] { new CallEffect(cc.SM, "m") });
            cc.SM.AddTrigger(trigger);

            Assert.IsFalse(cc.SM.IsPrepared);

            cc.Parameters.IsProcessEventPublic = true;

            cc.GenerateCode();

            Assert.IsTrue(cc.SM.IsPrepared);
            Assert.AreEqual(3, cc.SM.Events.Count());

            Trace("output:\n{0}", cc.Output);

            var csharp = new CSharpCodeProvider();
            var options = new CompilerParameters();

            var result = csharp.CompileAssemblyFromSource(options, cc.Output);

            if (result.Errors.Count > 0)
            {
                foreach (var e in result.Errors)
                {
                    Trace("{0}", e);
                }

                Assert.Fail("generated code failed to compile.");
            }

            var dll = result.CompiledAssembly;
            var type = dll.GetType("CodeGeneration");
            var eventtype = dll.GetType("EventCode");
            var events = Enum.GetValues(eventtype);

            var x = Activator.CreateInstance(type);

            var tostatestring = type.GetMethod("ToStateString");
            Trace("initial state [{0}].", tostatestring.Invoke(x, new object[0]));

            var processevent = type.GetMethod("ProcessEvent");
            var sendevent = events.GetValue(0);
            Trace("sending event '{0}' ...", sendevent);
            processevent.Invoke(x, new object[] { sendevent });

            var statestring = tostatestring.Invoke(x, new object[0]).ToString();
            Trace("state after [{0}].", statestring);

            Assert.AreEqual("s(b) t(b) f(0)", statestring);

            sendevent = events.GetValue(1);
            Trace("sending event '{0}' ...", sendevent);
            processevent.Invoke(x, new object[] { sendevent });

            statestring = tostatestring.Invoke(x, new object[0]).ToString();
            Trace("state after [{0}].", statestring);

            Assert.AreEqual("s(b) t(a) f(1)", statestring);
        }

        /// <summary>
        /// Code generation options.
        /// </summary>
        [TestMethod]
        public void SMG_05_02_CodeGeneration()
        {
            // TraceFlags.ShowLabel = true;

            var cc = new StateMachineCompiler();
            cc.CompileString(ReadEmbeddedScript("CodeGenerationOptions.smg"));

            Assert.AreEqual(true, cc.Parameters.IsPartial);
            Assert.AreEqual("Test.Code", cc.Parameters.Namespace);

            cc.GenerateCode();

            Trace("output:\n{0}", cc.Output);

            /*using(var writer = new StreamWriter(@"c:\users\tc\repositories\igra3\prototype\html\smg.js"))
            {
                writer.Write(cc.Output);
            }*/
        }

        [TestMethod]
        public void SMG_05_03_PseudoCode()
        {
            var cc = new StateMachineCompiler();
            cc.CompileString(ReadEmbeddedScript("StandardConditions.smg"));

            cc.Parameters.Language = "pseudo";

            cc.GenerateCode();

            Trace("output:\n{0}", cc.Output);
        }

        [TestMethod]
        public void SMG_05_04_SyntaxCases()
        {
            TraceFlags.ShowDepencencyAnalysis = true;

            var cc = new StateMachineCompiler();
            cc.CompileString(ReadEmbeddedScript("SyntaxCases.smg"));

            cc.Parameters.Language = "pseudo";

            cc.GenerateCode();

            Trace("output:\n{0}", cc.Output);
        }


        [TestMethod]
        public void SMG_04_Program()
        {
            TraceFlags.ShowDepencencyAnalysis = true;
            var cc = new StateMachineCompiler();

            var sm = cc.CompileString(ReadEmbeddedScript("script3.smg"));

            cc.GenerateCode();

            PrintEventEffectConditions(cc.SM);

            Trace("{0}", cc.Output);

            // Trace("gatecache: \n{0}", GateCache.Instance.ToDebugString());
        }

    }
}
