using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMG.Common.Code
{
    /// <summary>
    /// Represents a variable in generated code storing the result of a 
    /// condition evaluation at a given stage.
    /// </summary>
    public class CodeLabel
    {
        #region Private

        private IGate _originalgate;
        private IGate _gate;
        private GateConverter _gc;
        private int _schedulecount;

        #endregion

        #region Properties

        public int Index { get; private set; }

        public int Stage { get; private set; }

        public string Name { get; set; }

        public virtual IGate Gate { get { return _gate; } }

        public virtual IGate OriginalGate { get { return _originalgate; } }

        public int ScheduleCount { get { return _schedulecount; } }

        public bool IsScheduled { get { return _schedulecount > 0; } }

        public bool IsEvaluated { get; set; }

        #endregion

        #region Construction

        private CodeLabel(int index, string name)
        {
            Index = index;
            Name = name;
        }

        internal CodeLabel(GateConverter gc, int stage, int index, string name, IGate g)
            : this(index, name)
        {
            _gc = gc;
            _originalgate = _gate = g;

            Stage = stage;

            Debug.Assert(!(g is CodeLabel));

            // if (g is IVariableCondition)
            {
                _gate = new LabelGate(this);
            }
        }

        #endregion

        #region Diagnostics

        public override string ToString()
        {
            return "[" + Name + "]";
        }

        #endregion

        #region Public Methods

        public void Schedule()
        {
            var first = 0 == _schedulecount;

            _schedulecount++;

            if (first)
            {
                foreach (var e in _originalgate.SelectAll())
                {
                    if (e is LabelGate)
                    {
                        ((LabelGate)e).Schedule();
                    }
                    else if (e.Type.IsLogical())
                    {
                        _gc.Schedule(e);
                    }
                }

                _gc.Queue(this);
            }
        }

        /// <summary>
        /// Evaluates the label into a code writer, once.
        /// </summary>
        /// <param name="writer">The code writer.</param>
        public void Evaluate(CodeGenerator writer)
        {
            if (!IsEvaluated)
            {
                EvaluateOnce(writer);
                IsEvaluated = true;
            }
        }

        #endregion

        #region Private Methods

        private IGate EvaluateRecurse(CodeGenerator cg, IGate g)
        {
            if (g.Type == GateType.Input)
            {
                if (g is LabelGate)
                {
                    var lg = (LabelGate)g;
                    lg.Label.Evaluate(cg);
                }
            }
            else
            {
                var list = new List<IGate>();
                foreach (var i in g.GetInputs())
                {
                    var u = i;
                    var l = _gc.GetScheduledLabel(u);
                    if (null != l)
                    {
                        u = l;
                    }
                    else
                    {
                        u = EvaluateRecurse(cg, u);
                    }

                    list.Add(u);
                }

                g = SMG.Common.Gate.Compose(g.Type, list);
            }

            return g;
        }

        private void EvaluateOnce(CodeGenerator cg)
        {
            var g = EvaluateRecurse(cg, _originalgate);
            cg.EmitCodeLabelAssignment(Name, g);
        }

        #endregion
    }
}
