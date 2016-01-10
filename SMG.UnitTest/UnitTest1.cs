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
                Trace(" {0,-20} {1}", e.Name, e.PreCondition);

                foreach (var f in e.EffectsAfter.GetEffectConditions())
                {
                    Trace("  {0,-19} {1}", f.Effect, f.PreCondition);
                }
            }
        }

        private void ValidateEventEffectCondition(StateMachine sm, string eventname, string methodname, string ctext)
        {
            var ev = sm.Events.Where(e => e.Name == eventname).First();
            var ec = ev.EffectsAfter.GetEffectCondition(new CallEffect(sm, methodname));

            Assert.AreEqual(ctext, ec.PreCondition.ToString());
        }


        private string ReadEmbeddedScript(string name)
        {
            var path = "SMG.UnitTest.Scripts." + name;
            using (var s = GetType().Assembly.GetManifestResourceStream(path))
            using (var r = new StreamReader(s))
            {
                return r.ReadToEnd();
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
        public void SMG_03_StateCondition()
        {
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

            sm.CalculateDependencies();

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
            var sm = cc.CompileString(
                "SMG BasicTest " +
                "DECLARE State (a, b) s, t " +
                "DECLARE BOOLEAN f " +
                "TRIGGER e1 WHEN s(a => b) " +
                "TRIGGER e2 WHEN t(a => b) " +
                "GUARD WHEN ENTER t(b) AND s(b) AND f CALL q1 " +
                "TRIGGER e4 WHEN f(0 => 1) " +
                ""
                );

            sm.CalculateDependencies();
            sm.CalculateEffects();

            ValidateEventEffectCondition(sm, "e1", "q1", "s(a)t(b)f");
            ValidateEventEffectCondition(sm, "e2", "q1", "s(b)t(a)f");
            ValidateEventEffectCondition(sm, "e4", "q1", "s(b)t(b)!f");
        }

        [TestMethod]
        public void SMG_04_01_EnterLeaveGuards()
        {
            var cc = new StateMachineCompiler();
            var sm = cc.CompileString(
                "SMG test \n" +
                "DECLARE State (a, b) s, t, x\n" +
                "TRIGGER e1 WHEN s(a => b) " +
                "TRIGGER e2 WHEN t(a => b) " +
                "TRIGGER e3 WHEN s(a => b) AND t(a => b) " +
                "GUARD WHEN ENTER t(b) AND s(b) CALL q1 " +
                "GUARD WHEN LEAVE t(a) AND s(a) CALL q2 " +
                ""
                );

            sm.CalculateDependencies();
            sm.CalculateEffects();

            ValidateEventEffectCondition(sm, "e1", "q1", "s(a)t(b)");
            ValidateEventEffectCondition(sm, "e2", "q1", "s(b)t(a)");
            ValidateEventEffectCondition(sm, "e3", "q1", "s(a)t(a)");
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

            sm.CalculateDependencies();
            sm.CalculateEffects();

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
            var sm = cc.CompileString(
                "SMG test " +
                "DECLARE State (a, b, c) s, t " +
                "TRIGGER e1 WHEN s(a => b) OR s(a => c) CALL m1" +
                ""
                );

            sm.CalculateDependencies();
            sm.CalculateEffects();

            PrintEventEffectConditions(sm);

            /*using (var w = new StreamWriter(@"c:\work\tmp\smg.cs"))
            {
                w.Write(sm.GenerateCode());
            }*/
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

            IGate g;

            //var c1dc = Gate.Invert(c1);
            //Trace("0 === {0}", c1dc);

            var c1dc = Gate.ComposeAND(Gate.Invert(c1.Decompose(ConditionMode.Pre)), Gate.Invert(c2.Decompose(ConditionMode.Pre)));
            // var c1dc = Gate.Invert(c1);
            //var c1dc = c1.Decompose();

            Trace("1 === {0}", c1dc);

            /*var ic1 = Gate.Invert(c1dc);
            Trace("2 === {0}", ic1);*/

            return;

            //g = Gate.ComposeAND(Gate.Invert(c1), c2);
            //g = Gate.ComposeAND(g, Gate.Invert(c3));



            Trace("{0}", g);
        }

        /// <summary>
        /// Validates some standard constructions.
        /// </summary>
        [TestMethod]
        public void SMG_04_06_StandardConditions()
        {
            //TraceFlags.ShowDepencencyAnalysis = true;

            var cc = new StateMachineCompiler();
            var smgtext = ReadEmbeddedScript("StandardConditions.smg");
            var sm = cc.CompileString(smgtext);

            sm.CalculateDependencies();
            sm.CalculateEffects();


            ValidateEventEffectCondition(sm, "e1", "ge1", "s(a) + t(a)");
            ValidateEventEffectCondition(sm, "e1", "gprod", "s(a)t(b) + s(b)t(a)");
            ValidateEventEffectCondition(sm, "e1", "gsum", "s(a) + t(a)");

            var ce1 = "s(a)t(a)";
            ValidateEventEffectCondition(sm, "e2", "ge2", ce1);
            ValidateEventEffectCondition(sm, "e2", "gprod", ce1);
            ValidateEventEffectCondition(sm, "e2", "gsum", ce1);

            ValidateEventEffectCondition(sm, "e3", "gsum", "0");
            ValidateEventEffectCondition(sm, "e4", "gsum", "0");
        }

        [TestMethod]
        public void SMG_04_07_MixedConditions()
        {
            TraceFlags.ShowDepencencyAnalysis = true;

            var cc = new StateMachineCompiler();
            var smgtext = ReadEmbeddedScript("MixedConditions.smg");
            var sm = cc.CompileString(smgtext);

            sm.CalculateDependencies();
            sm.CalculateEffects();

            ValidateEventEffectCondition(sm, "e1", "m1", "s(a)t(a)");
        }

        [TestMethod]
        public void SMG_04_Program()
        {
            TraceFlags.ShowDepencencyAnalysis = true;
            var cc = new StateMachineCompiler();

            var sm = cc.CompileString(ReadEmbeddedScript("script1.smg"));

            // Trace("gatecache: \n{0}", GateCache.Instance.ToDebugString());
            // sm.Dump();

            if (true)
            {
                sm.CalculateDependencies();

                using (var w = new StreamWriter(@"c:\work\tmp\smg.cs"))
                {
                    w.Write(sm.GenerateCode());
                }
            }

            // Trace("gatecache: \n{0}", GateCache.Instance.ToDebugString());
        }

    }
}
