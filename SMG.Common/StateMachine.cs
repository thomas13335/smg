using SMG.Common.Code;
using SMG.Common.Conditions;
using SMG.Common.Exceptions;
using SMG.Common.Gates;
using SMG.Common.Transitions;
using SMG.Common.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMG.Common
{
    /// <summary>
    /// The state machine.
    /// </summary>
    /// <remarks>
    /// <para>A state machine consists of a set of events, associated triggers and guards.</para>
    /// <para>Each trigger or guard can include actions.</para>
    /// </remarks>
    public class StateMachine
    {
        #region Private

        private int _nextaddress = 0;
        private Dictionary<string, StateType> _types = new Dictionary<string, StateType>();
        private Dictionary<string, Variable> _variables = new Dictionary<string, Variable>();
        private Dictionary<string, Event> _events = new Dictionary<string, Event>();
        private Dictionary<string, Guard> _guards = new Dictionary<string, Guard>();
        private HashSet<string> _methods = new HashSet<string>();
        private List<IGate> _assertions = new List<IGate>();
        private List<Exception> _errors = new List<Exception>();
        private CodeLocation _current;

        #endregion

        #region Properties

        /// <summary>
        /// The variables making up this state machine.
        /// </summary>
        public ICollection<Variable> Variables { get { return _variables.Values; } }

        public ICollection<Event> Events { get { return _events.Values; } }

        public ICollection<Guard> Guards { get { return _guards.Values; } }

        public ICollection<IGate> Assertions { get { return _assertions; } }

        public ICollection<string> Methods { get { return _methods; } }

        /*public Condition InitialCondition
        {
            get
            {
                return new IntersectCondition(_variables.Values.Select(v => v.InitialCondition));
            }
        }*/

        /// <summary>
        /// Name of the state machine given by the script.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Optional, path of the source file.
        /// </summary>
        public string SourceFile { get; set; }

        public CodeLocation CurrentLocation
        {
            get { return _current; }
            set
            {
                value.SourceFile = SourceFile;
                _current = value;
            }
        }

        public IEnumerable<Exception> Errors { get { return _errors; } }

        #endregion

        #region Construction

        /// <summary>
        /// Constructs a new, empty state machine.
        /// </summary>
        public StateMachine()
        {
            // add BOOLEAN type always (in both case)
            var booltype = new BooleanStateType();
            _types.Add(booltype.Name.ToUpper(), booltype);
            _types.Add(booltype.Name.ToLower(), booltype);
        }

        #endregion

        #region Diagnostics

        private void Trace(string format, params object[] args)
        {
            Debug.WriteLine(format, args);
        }

        private void TraceVerbose(string format, params object[] args)
        {
            if (TraceFlags.Verbose)
            {
                Debug.WriteLine(format, args);
            }
        }

        private void TraceDependencies(string format, params object[] args)
        {
            if (TraceFlags.ShowDepencencyAnalysis)
            {
                Debug.WriteLine(format, args);
            }
        }


        public void Dump()
        {
            var sb = new CodeWriter();
            sb.AppendLine("state machine dump");

            foreach (var e in _events.Values)
            {
                sb.AppendComment();
                sb.AppendComment("event " + e.Name);
                foreach (var t in e.Triggers)
                {
                    sb.AppendLine("TRIGGER " + t.Name + " WHEN");
                    sb.AppendLine("  BEFORE [" + t.PreCondition + "] AFTER [" + t.PostCondition + "]");
                }
            }

            foreach (var t in Guards)
            {
                sb.AppendLine("GUARD " + t.Name + " " + t.Type + " WHEN ");
                if (t.Type == GuardType.TRANSITION)
                {
                    sb.AppendLine("  BEFORE [" + t.PreCondition + "]");
                    sb.AppendLine("  AFTER [" + t.PostCondition + "]");
                }
                else if (t.Type == GuardType.ENTER)
                {
                    sb.AppendLine("  AFTER [" + t.PostCondition + "]");
                }
                else if (t.Type == GuardType.LEAVE)
                {
                    sb.AppendLine("  BEFORE [" + t.PreCondition + "]");
                }
            }
            // sb.AppendLine("matrix:\n" + _matrix);

            Trace("{0}", sb);
        }

        #endregion

        #region Public Methods

        #region Modifications

        public StateType GetStateType(string id)
        {
            StateType result;
            _types.TryGetValue(id, out result);
            return result;
        }

        public StateType AddSimpleType(string id)
        {
            if(null != GetStateType(id))
            {
                throw new CompilerException(21, "type '" + id + "' is already defined.");
            }

            var stype = new SimpleStateType(id);
            _types[id] = stype;

            TraceVerbose("type '{0}' added.", id);

            return stype;
        }

        /// <summary>
        /// Adds a new variable to the state machine.
        /// </summary>
        /// <param name="id">The identifier of the variable.</param>
        /// <param name="declaration">The state type.</param>
        /// <returns>The resulting variable object.</returns>
        public Variable AddVariable(string id, StateType declaration)
        {
            if(_variables.ContainsKey(id))
            {
                throw new Exception("SMG010: variable '" + id + "' already exists." );
            }

            var result = new Variable(id, declaration);
            result.Index = _variables.Count;
            result.Address = _nextaddress;
            _nextaddress += result.Cardinality;

            _variables.Add(id, result);
            return result;
        }

        /// <summary>
        /// Returns an existing variable.
        /// </summary>
        /// <param name="varname">The name of the variable.</param>
        /// <returns>The variable object.</returns>
        public Variable GetVariable(string varname)
        {
            Variable v;
            if(!_variables.TryGetValue(varname, out v))
            {
                throw new Exception("SMG019: variable '" + varname + "' undefined.");
            }

            return v;
        }

        /// <summary>
        /// Adds an assertion rule to the state machine.
        /// </summary>
        /// <param name="assertion"></param>
        public void AddAssertion(ICondition assertion)
        {
            // mark all combination of states where the condition is not true
            /*var senum = new StateEnumerator(this);
            foreach(var s in senum.GetStates(new InvertCondition(assertion)))
            {
                GetState(s).Unallowed = true;
            }*/

            _assertions.Add(assertion.Decompose(ConditionMode.Static));
        }

        /// <summary>
        /// Adds a new event to the state machine.
        /// </summary>
        /// <param name="eventname"></param>
        /// <returns></returns>
        public Event AddEvent(string eventname)
        {
            Event e;
            if (!_events.TryGetValue(eventname, out e))
            {
                _events[eventname] = e = new Event(eventname);
            }

            return e;
        }

        public Guard AddGuard(ICondition c, GuardType gtype = GuardType.ENTER, string name = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = "G" + (_guards.Count + 1);
            }
            else if (_guards.ContainsKey(name))
            {
                throw new CompilerException(33, "guard '" + name + "' already exists.");
            }

            var guard = new Guard(name, gtype, c);
            _guards.Add(name, guard);
            return guard;
        }

        private void ValidateTrigger(Trigger trigger)
        {
            var tset = trigger.Transitions;
            foreach (var v in tset.Variables)
            {
                var branches = new List<IGate>();
                foreach(var t in tset.GetTransitions(v))
                {
                    var c = t.PreCondition;

                    // preconditions must not intersect
                    foreach(var b in branches)
                    {
                        var sum = Gate.ComposeAND(c, b);
                        if(!(sum is FalseGate))
                        {
                        }
                    }

                    branches.Add(c);
                }
            }

            var tlist = new List<Trigger>();

        }

        /// <summary>
        /// Adds a trigger to the state machine.
        /// </summary>
        /// <param name="trigger">The trigger object to add.</param>
        public void AddTrigger(Trigger trigger)
        {
            var e = trigger.Event;

            try
            {

                TraceDependencies("add trigger '{0}' ...", trigger.Name);

                // ValidateTrigger(trigger);

                var tlist = new List<ProductTrigger>();

                // examinate the precondition ...
                var precondition = trigger.PreCondition;

                if (precondition.Type == GateType.Fixed)
                {
                    // constant 
                    if (precondition is FalseGate)
                    {
                        Trace("SMG033: warning: trigger '{0}' precondition is never met.", trigger);
                    }
                    else if (precondition is TrueGate)
                    {
                        Trace("SMG034: warning: trigger '{0}' precondition is always true.", trigger);
                    }
                }
                else if (precondition.Type == GateType.OR)
                {
                    // OR combination, split into multiple simple triggers
                    var inputs = precondition.GetInputs();
                    TraceDependencies("split trigger [{0}] into its branches ...", inputs.ToSeparatorList());

                    // analyze conditions
                    // TraceDependencies("{0}", trigger.Transitions.ToDebugString());

                    TraceDependencies("evaluating branches ...");

                    // split into multiple triggers
                    foreach (var g in inputs)
                    {
                        TraceDependencies("trigger path [{0}] ...", g);

                        // replace transitions
                        var tset = new TransitionSet(g);
                        var pre = ReplaceVariableCondition(g, false);
                        var post = ReplaceVariableCondition(g, true);

                        TraceDependencies("branch trigger conditions [{0} => {1}].", pre, post);

                        // add branch trigger
                        tlist.Add(new ProductTrigger(trigger, tset, pre, post));
                    }
                }
                else
                {
                    // add original trigger
                    tlist.Add(new ProductTrigger(trigger));
                }

                // process resulting triggers
                foreach (var t1 in tlist)
                {
                    TraceDependencies("product trigger {0} ...", t1);
                    TraceDependencies("  conditions [{0}, {1}] ...", t1.PreCondition, t1.PostCondition);

                    // TraceDependencies("transitions:\n{0}", trigger.Transitions.ToDebugString());
                    t1.Qualify();

                    // check if any existing trigger is conflicting
                    foreach (var existingtrigger in e.Triggers)
                    {
                        // the new trigger precondition must not intersect any existing precondition
                        var product = Gate.ComposeAND(existingtrigger.PreCondition, t1.PreCondition);
                        product = ReplaceTransitionVariables(product, existingtrigger, false);
                        if (!product.IsFalse())
                        {
                            throw new CompilerException(10, "ambigous transition conditions [" +
                                existingtrigger.PreCondition + ", " + t1.PreCondition + "].");
                        }
                    }

                    e.Triggers.Add(t1);
                }
            }
            catch(CompilerException ex)
            {
                AddError(ex);
            }
        }

        public void AddMethod(string name)
        {
            if (!_methods.Contains(name))
            {
                _methods.Add(name);
            }
        }

        #endregion

        /// <summary>
        /// Calculates dependencies between triggers and guards.
        /// </summary>
        public void CalculateDependencies()
        {
            TraceDependencies("calculate dependencies ...");
            foreach (var e in _events.Values)
            {
                foreach (var t in e.Triggers)
                {
                    var ct = t.PreCondition;
                    var gtpost = t.PostCondition;
                    var gtpre = ct;
                    TraceDependencies("trigger '{0}' conditions [{1}, {2}] ...", t.Name, gtpre, gtpost);

                    if (!t.ModifiedVariables.Any())
                    {
                        TraceDependencies("  does not modify state.");
                        continue;
                    }

                    foreach (var g in Guards)
                    {
                        var gg = g.PreCondition;
                        TraceDependencies("  guard '{0}' precondition [{1}] ...", g.Name, gg);

                        var tg = new TriggerGuard(t, g);

                        if (g.Transitions.Any())
                        {
                            TraceDependencies("    state change guard ...");
                            if (!g.ModifiedVariables.Intersect(t.ModifiedVariables).Any())
                            {
                                TraceDependencies("    no affected variable.");
                                continue;
                            }

                            var gatepre = Gate.ComposeAND(gtpre, gg);
                            if (gatepre.Type == GateType.Fixed)
                            {
                                TraceDependencies("    precondition does not match.");
                                continue;
                            }

                            var gatepost = Gate.ComposeAND(gtpost, g.PostCondition);
                            if (gatepost.Type == GateType.Fixed)
                            {
                                TraceDependencies("    postcondition does not match.");
                                continue;
                            }

                            TraceDependencies("    combination ({0}, {1}) handler.", gatepre, gatepost);

                            tg.PreCondition = gatepre;
                            tg.PostCondition = gatepost;
                        }
                        else
                        {
                            TraceDependencies("    static condition [{0}] ...", gg);

                            /*var gatepre = Gate.ComposeAND(gtpre, gg);
                            var gatepost = Gate.ComposeAND(gtpost, g.PostCondition);*/

                            Debug.Assert(g.PreCondition.ID == g.PostCondition.ID);

                            IGate genter, gleave;

                            if (g.Type == GuardType.ENTER)
                            {
                                // PRE[trigger] AND NOT POST[guard]
                                // trigger condition met, guard condition not met
                                genter = Gate.ComposeAND(t.PreCondition, Gate.Invert(g.PreCondition));

                                // PRE[trigger] AND PRE[guard]
                                // trigger condition met, guard conditon not met
                                gleave = Gate.ComposeAND(t.PostCondition, g.PostCondition);
                            }
                            else
                            {
                                genter = Gate.ComposeAND(t.PreCondition, g.PreCondition);
                                gleave = Gate.ComposeAND(t.PostCondition, Gate.Invert(g.PostCondition));
                            }

                            Gate.TraceDependencies(genter, gleave, "guard product");

                            // set all factors not appearing in the trigger to 1
                            genter = ReplaceTransitionVariables(genter, t, true);
                            gleave = ReplaceTransitionVariables(gleave, t, true);

                            // set all factors not appearing in the guard to 1
                            genter = ReplaceNonGuardVariables(genter, g);
                            gleave = ReplaceNonGuardVariables(gleave, g);

                            Gate.TraceDependencies(genter, gleave, "guard match");

                            //if (genter is FalseGate || gleave is FalseGate)
                            if(genter.Type.IsFixed() || gleave.Type.IsFixed())
                            {
                                TraceDependencies("    condition does not match.");
                                continue;
                            }

                            tg.PreCondition = g.PreCondition;
                            tg.PostCondition = g.PostCondition;

                            // see if the transition set covers the resulting transition
                            var transx = t.Transitions.Match(genter, gleave).ToArray();
                            if (transx.Length == t.Transitions.Variables.Count())
                            {
                                // TODO: this check is unnecessary
                                if (g.Type == GuardType.ENTER)
                                {
                                    TraceDependencies("    entry handler.");
                                }
                                else if (g.Type == GuardType.LEAVE)
                                {
                                    TraceDependencies("    exit handler.");
                                }
                                else
                                {
                                    throw new InvalidOperationException("unexpected guard type.");
                                }
                            }
                            else
                            {
                                TraceDependencies("    transition does not match.");
                                continue;
                            }
                        }

                        // associate the trigger with the guard and vice versa
                        t.AddGuard(tg);
                    }
                }
            }
        }

        public void CalculateEffects()
        {
            foreach (var e in _events.Values)
            {
                e.CalculateEffects();
            }
        }

        public string GenerateCode()
        {
            Trace("\ngenerating code ...");
            var sb = new CodeWriter();
            var generator = new CSharpCodeGenerator(sb);

            generator.Emit(this);




            //App.Trace("code:\n{0}", sb);
            return sb.ToString();
        }

        #endregion

        private void AddError(CompilerException ex)
        {
            if (null == ex.Location)
            {
                ex.Location = CurrentLocation;
            }

            _errors.Add(ex);

            if(null != ex.Location)
            {
                Trace("{0}: {1}", ex.Location, ex.Message);
            }
            else
            {
                Trace("{0}", ex.Message);
            }
        }

        #region Private Methods

        /// <summary>
        /// Replaces variables conditions in a gate with the pre or post conditions.
        /// </summary>
        /// <param name="e">The gate to replace.</param>
        /// <param name="post">True if the post condition shall be evaluated, false otherwise.</param>
        /// <returns>The resulting gate.</returns>
        private IGate ReplaceVariableCondition(IGate e, bool post)
        {
            return e.Replace(g => ReplaceVariableConditionHandler(g, post));
        }

        /// <summary>
        /// Replaces composite state conditions with their elementary equivalents.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="post"></param>
        /// <returns></returns>
        private IGate ReplaceVariableConditionHandler(IGate e, bool post)
        {
            IGate result = e;

            if (e is IVariableCondition)
            {
                var c = (IVariableCondition)e;
                var tlist = c.GetTransitions();

                // unique transition required
                var count = tlist.Count();
                if (1 != tlist.Count())
                {
                    throw new CompilerException(10, "multiple transitions for variable '" + c.Variable + "'.");
                }

                // unique post state required
                var t = tlist.First();
                var stateindexes = post ? t.NewStateIndexes : t.PreStateIndexes;
                if (stateindexes.Length != 1)
                {
                    throw new CompilerException(10, "ambigous variable condition.");
                }

                result = c.CreateElementaryCondition(stateindexes.First());
            }

            // Gate.TraceDecompose(e, result, "var repl {0}", post);

            return result;
        }

        private IGate ReplaceTransitionVariables(IGate gate, TransitionMonitor monitor, bool value)
        {
            return gate
                .Replace(g => ReplaceTransitionVariablesHandler(g, monitor, value))
                .Simplify();
        }

        /// <summary>
        /// Replaces all variables not found in a guard with one.
        /// </summary>
        /// <param name="gate"></param>
        /// <param name="monitor"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private IGate ReplaceNonGuardVariables(IGate gate, Guard monitor)
        {
            // get variables referenced in the condition
            var variables = monitor.PreCondition
                .SelectAll(e => e is IVariableCondition)
                .OfType<IVariableCondition>()
                .Select(u => u.Variable)
                .Distinct();

            // TraceDependencies("monitor variables: {0}", variables.ToSeparatorList());

            IGate r = gate
                .Replace(g =>
                {
                    if (g is IVariableCondition)
                    {
                        var vc = (IVariableCondition)g;
                        if(!variables.Contains(vc.Variable))
                        {
                            g = Gate.Constant(true);
                        }
                    }

                    return g;
                })
                .Simplify();

            return r;
        }

        private IGate ReplaceTransitionVariablesHandler(IGate q, TransitionMonitor monitor, bool value)
        {
            var result = q;

            if (result is IVariableCondition)
            {
                var vc = ((IVariableCondition)result);
                if (!monitor.Transitions.Contains(vc.Variable))
                {
                    result = Gate.Constant(value);
                }
                else
                {
                    // result = Gate.Constant(true);
                }
            }

            return result;
        }

        #endregion
    }
}
