using SMG.Common.Code;
using SMG.Common.Conditions;
using SMG.Common.Effects;
using SMG.Common.Transitions;
using SMG.Common.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMG.Common.Code
{
    /// <summary>
    /// Generates target language code from a state machine.
    /// </summary>
    /// <remarks>
    /// <para>Derive from this class to create specific target language generators.</para></remarks>
    public abstract class CodeGenerator : ICodeLabelEvaluator, ICodeGateEvaluator
    {
        #region Private

        private GateConverter _gc;
        private GuardCollection _guards;
        private CodeWriter _writer;
        private TransitionSet _tset;
        private List<int> _modified;
        private EffectsCollection _effectsbefore;
        private EffectsCollection _effectsafter;
        private StateMachine _sm;

        #endregion

        #region Properties

        protected StateMachine SM { get { return _sm; } }

        protected CodeWriter Writer { get { return _writer; } }

        public CodeParameters Parameters { get; set; }

        #endregion

        #region Construction

        protected CodeGenerator(CodeWriter writer)
        {
            this._writer = writer;
            _gc = null;
            Parameters = new CodeParameters();
        }

        #endregion

        #region Diagnostics

        private void Trace(string format, params object[] args)
        {
            Debug.WriteLine(format, args);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Emits code for a state machine.
        /// </summary>
        /// <param name="sm">The state machine to generate.</param>
        public void Emit(StateMachine sm)
        {
            if(string.IsNullOrEmpty(sm.Name))
            {
                throw new ArgumentException("state machine name required.");
            }

            try
            {
                _sm = sm;

                EmitCore();
            }
            finally
            {
                _sm = null;
            }
        }

        #endregion

        #region Overrideable

        protected virtual void EmitCore()
        {
            EmitPreamble();
            EmitEventTypeDeclaration();

            EmitClassHeader();
            Writer.EnterBlock(); // class
            EmitTypeDeclarations();
            EmitVariableInitialization();

            // EmitStateString(sb);

            Writer.AppendLine();
            EmitMethodDeclarations();
            EmitProcessEventMethod();

            foreach (var e in SM.Events)
            {
                EmitEventHandler(e);
            }

            EmitClassFooter();
            Writer.LeaveBlock(); // class

            EmitFooter();
        }

        protected virtual void EmitPreamble()
        {
        }

        protected virtual void EmitFooter()
        {
        }

        protected virtual void EmitEventTypeDeclaration()
        {}

        protected virtual void EmitTypeDeclaration(string typename, IEnumerable<string> values)
        { }

        protected abstract void EmitVariableDeclaration(Variable v);

        protected abstract void EmitVariableAccessor(Variable v);

        protected abstract void EmitVariableAssignment(Variable v, int stateindex);

        protected virtual void EmitMethodDeclaration(string method)
        {
        }

        protected virtual void EmitMethodDeclarations()
        {
            foreach (var m in SM.Methods)
            {
                EmitMethodDeclaration(m);
            }
        }

        protected abstract void EmitProcessEventMethodHeader();

        protected virtual void EmitEnterBlock()
        {
            Writer.EnterBlock();
        }

        protected virtual void EmitLeaveBlock()
        {
            Writer.LeaveBlock();
        }

        protected virtual void EmitSwitchBlockHeader()
        {
            Writer.AppendLine("switch(e)");
            EmitEnterBlock();
        }

        protected virtual void EmitSwitchBlockFooter()
        {
            EmitLeaveBlock();
        }

        protected abstract void EmitSwitchCaseLabel(Event e);

        protected abstract void EmitHandlerInvocation(Event e);

        protected virtual void EmitProcessEventMethod()
        {
            EmitProcessEventMethodHeader();
            EmitEnterBlock();

            EmitSwitchBlockHeader();
            foreach (var e in SM.Events)
            {
                EmitSwitchCaseLabel(e);
                Writer.Indent();
                EmitHandlerInvocation(e);
                Writer.Unindent();
            }
            EmitSwitchBlockFooter();
            EmitLeaveBlock();
        }

        protected abstract void EmitClassHeader();

        protected virtual void EmitClassFooter()
        {
        }

        protected virtual void EmitHandlerHeader(string name)
        {
        }

        protected virtual void EmitGate(IGate gate)
        {
            gate = _gc.ReplaceWithLabelIf(gate);
            gate.Emit(this);
        }

        protected virtual void EmitIfHeader(IGate gate)
        {
            Writer.Append("if(");
            EmitGate(gate);
            Writer.AppendLine(")");
        }

        protected virtual void EmitEffect(Effect effect)
        {
        }

        public virtual void EmitCodeLabelAssignment(string label, IGate gate)
        {
            Writer.Append("var " + label + " = ");
            gate.Emit(this);
            Writer.AppendLine(";");
        }

        #endregion

        #region ICodeGateEvaluator

        void ICodeGateEvaluator.Append(string text)
        {
            Writer.Append(text);
        }

        public abstract void EmitVariable(Variable v);

        public abstract void EmitBinaryOperator(GateType type);

        public abstract void EmitVariableStateCondition(Variable v, int stateindex);

        #endregion

        #region Private Methods

        private void EmitTypeDeclarations()
        {
            var types = SM.Variables.Select(e => e.Type).Distinct();
            foreach (var type in types.OfType<SimpleStateType>())
            {
                EmitTypeDeclaration(type.Name, type.GetAllStateNames());
            }
        }

        private void EmitVariableInitialization()
        {
            foreach (var v in SM.Variables)
            {
                EmitVariableDeclaration(v);
            }

            Writer.AppendComment();
            foreach (var v in SM.Variables)
            {
                EmitVariableAccessor(v);
            }
        }

        /// <summary>
        /// Emits code for a single event handler.
        /// </summary>
        /// <param name="e"></param>
        private void EmitEventHandler(Event e)
        {
            BeginHandler(e);

            PrepareTransitionSet(e);

            // use gate converter to schedule condition evaluation
            ConvertConditions(e);

            // stage 0: preconditions and LEAVE handlers
            _guards.AddLeaveEffects(_effectsbefore);
            _guards.AddEnterEffects(_effectsafter);
            AddTriggerEffects(e.Triggers);

            foreach(var t in e.Triggers)
            {
                var c = _gc.ConvertToGate(0, t.PreCondition);
                _gc.Schedule(c);
            }

            _effectsbefore.Schedule(_gc);
            _effectsafter.Schedule(_gc);

            // code output: precondition labels
            EmitCodeLabels(0);

            // pre-effects
            if (!_effectsbefore.IsEmpty)
            {
                Writer.AppendComment();
                Writer.AppendComment("state exit handler effects");

                EmitEffects(_effectsbefore.GetEffectConditions());
            }

            // emit state transition code
            EmitTriggerStateChange(_writer, e.Triggers);

            // stage 1: postconditions and other handlers.
            _gc.SetNextStage();

            // code output: postcondition labels
            EmitCodeLabels(1);

            // post-effects
            if (!_effectsafter.IsEmpty)
            {
                Writer.AppendComment();
                Writer.AppendComment("state entry handler effects");

                EmitEffects(_effectsafter.GetEffectConditions());
            }

            EndHandler();
        }

        private void EmitEffects(IEnumerable<EffectCondition> list)
        {
            foreach(var ec in list)
            {
                var pre = _gc.ConvertToGate(0, ec.PreCondition);
                var post = _gc.ConvertToGate(1, ec.PostCondition);

                var c = Gate.ComposeAND(pre, post);

                EmitIfHeader(c);
                EmitEnterBlock();

                foreach(var source in ec.Sources)
                {
                    Writer.AppendComment(source.ToString());
                }

                EmitEffect(ec.Effect);
                EmitLeaveBlock();
            }
        }

        private void PrepareTransitionSet(Event e)
        {
            // next state inference ...
            _tset = new TransitionSet();
            _tset.AddRange(e.Triggers
                .SelectMany(t => t.Transitions)
                );

            // set of modified variable indexes
            _modified = e.Triggers
                .SelectMany(t => t.ModifiedVariables)
                .Distinct()
                .Select(z => z.Index).ToList();
        }

        private void AddTriggerEffects(IEnumerable<ProductTrigger> triggers)
        {
            foreach (var t in triggers)
            {
                foreach (var effect in t.Effects)
                {
                    _effectsafter.AddEffect(t, effect);
                }
            }
        }

        private void EmitTriggerStateChange(CodeWriter writer, IEnumerable<Trigger> triggers)
        {
            writer.AppendComment();
            writer.AppendComment("state transition");

            foreach (var t in triggers)
            {
                var c = _gc.ConvertToGate(0, t.PreCondition);

                EmitIfHeader(c);
                writer.EnterBlock();
                writer.AppendComment("transition " + t);
                foreach (var x in t.Transitions)
                {
                    EmitVariableAssignment(x.Variable, x.SinglePostStateIndex);
                }

                writer.LeaveBlock();
            }
        }

        private void EmitCodeLabels(int stage)
        {
            /*_writer.AppendComment();
            _writer.AppendComment("stage " + stage + " conditions");*/ 
            _gc.Emit(this, stage);
        }

        private void BeginHandler(Event e)
        {
            // code generation start here
            EmitHandlerHeader(e.Name);
            Writer.EnterBlock();

            _gc = new GateConverter();

            // process labels before they are added ...
            _gc.OnBeforeAddLabel = EvaluateCodeLabelBefore;

            _guards = new GuardCollection(_gc);
            _effectsbefore = new EffectsCollection();
            _effectsafter = new EffectsCollection();
        }

        private void EndHandler()
        {
            if(TraceFlags.ShowLabel)
            {
                Trace("labels:\n{0}", _gc.ToDebugString());
            }

            _guards = null;
            _effectsbefore = null;
            _effectsafter = null;

            _gc.Dispose();
            _gc = null;

            Writer.LeaveBlock();
        }

        private void ConvertConditions(Event e)
        {
            var c = new TriggerTermCollection<bool>(true);

            foreach (var t in e.Triggers)
            {
                c.Add(t);
            }

            foreach (var t in e.Triggers)
            {
                foreach (var g in t.Guards)
                {
                    _guards.AddGuard(c, g);
                }
            }
        }

        /// <summary>
        /// Expresses a variable post-condition with preconditions.
        /// </summary>
        /// <param name="stage"></param>
        /// <param name="gate"></param>
        /// <returns></returns>
        private IGate EvaluateCodeLabelBefore(int stage, IGate gate)
        {
            if (stage == 1)
            {
                if (gate is IVariableCondition)
                {
                    var c = (IVariableCondition)gate;
                    if (!_modified.Contains(c.Variable.Index))
                    {
                        // state variable is not modified by the trigger
                        var label = _gc.AddGate(0, gate);

                        // create an alias for stage 1 value    
                        _gc.SetAlias(1, gate, label.Gate, 0);
                    }
                    else
                    {
                        // state variable is part of the transition, infer postcondition ...
                        //IGate inferred;
                        /*if (_tset.InferPostState(c.Variable, c.StateIndex, out inferred))
                        {
                            var alt = _gc.ConvertToGate(0, inferred);
                            _gc.SetAlias(1, gate, alt, 0);
                            gate = alt;
                        }*/
                    }
                }
            }

            return gate;
        }

        #endregion
    }
}
