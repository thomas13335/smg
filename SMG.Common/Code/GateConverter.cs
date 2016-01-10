using SMG.Common.Gates;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMG.Common.Code
{
    /// <summary>
    /// Converts one set of gates into another set of gates based on code labels.
    /// </summary>
    public class GateConverter : IDisposable
    {
        #region Private

        private int _lseed = 0;
        private Dictionary<string, CodeLabel> _labels = new Dictionary<string, CodeLabel>();
        private Dictionary<string, CodeLabel> _gatemap = new Dictionary<string, CodeLabel>();
        private List<CodeLabel> _queue = new List<CodeLabel>();

        #endregion

        #region Properties

        /// <summary>
        /// Called before a label is created.
        /// </summary>
        public Func<int, IGate, IGate> OnBeforeAddLabel { get; set; }

        public int CurrentStage { get; private set; }

        #endregion

        #region Construction

        public GateConverter()
        {
            GateCache.Push();
        }

        #endregion

        #region Diagnostics

        private void Trace(string format, params object[] args)
        {
            Debug.WriteLine(format, args);
        }

        public string ToDebugString()
        {
            var sb = new StringBuilder();

            sb.AppendLine("labels:");
            foreach (var pair in _labels)
            {
                var label = pair.Value;

                sb.AppendFormat(" {0,-16}", pair.Key);
                sb.AppendFormat(" {0:X8}", label.GetHashCode());
                sb.AppendFormat(" {0} ", 0 == label.Stage ? "PRE " : "POST");
                sb.AppendFormat(label.IsEvaluated ? "*" : " ");
                sb.AppendFormat(" {0,2} ", label.ScheduleCount);
                sb.AppendFormat(" {0,4} ", label.OriginalGate.ID);
                sb.AppendFormat(" {0,-12}", label);
                sb.AppendFormat(" {0} ", label.OriginalGate);
                sb.AppendLine();
            }

            /*sb.AppendLine("gatemap:");
            foreach(var pair in _gatemap)
            {
                sb.AppendFormat(" {0,-12} {1}", pair.Value, pair.Key);
                sb.AppendLine();
            }*/

            return sb.ToString();
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            GateCache.Pop();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Converts a gate into a label gate.
        /// </summary>
        /// <param name="stage">The execution stage where the label is evaluated.</param>
        /// <param name="g">The gate to convert.</param>
        /// <returns>The converted gate.</returns>
        public IGate ConvertToGate(int stage, IGate g)
        {
            // simplify to get cached
            g = Gate.Simplify(g);

            if(g.Type.IsFixed())
            {
                //throw new Exception("cannot convert fixed gate.");
                return g;
            }

            if (!(g is LabelGate))
            {
                g = MakeCodeLabels(stage, g);
            }

            return g;
        }

        /// <summary>
        /// Schedules a gate for evaluation
        /// </summary>
        /// <param name="gate"></param>
        public void Schedule(IGate gate)
        {
            if(gate.Type.IsFixed())
            {
                return;
            }

            var g = gate.Simplify();
            var id = GetCodeLabelIdentifier(CurrentStage, g);

            CodeLabel label;
            if (_labels.TryGetValue(id, out label))
            {
                Gate.TraceLabel(g, label.Gate, "schedule ...");
                label.Schedule();
            }
            else
            {
                Gate.TraceLabel(g, g, "schedule elements ...");
                foreach (var e in gate.SelectAll().OfType<LabelGate>())
                {
                    ((LabelGate)e).Schedule();
                }
            }
        }

        public IGate GetScheduledLabel(IGate arg)
        {
            var gate = arg;
            gate = gate.Simplify();

            if (gate.IsCached())
            {
                CodeLabel label;
                if (_gatemap.TryGetValue(gate.ID, out label))
                {
                    if (label.IsEvaluated)
                    {
                        Gate.TraceLabel(arg, gate, "getlabel");
                        gate = label.Gate;
                    }
                }
            }

            return gate;
        }

        public void Queue(CodeLabel label)
        {
            _queue.Add(label);
        }

        /// <summary>
        /// Sets the alias for an expression to an existing label.
        /// </summary>
        /// <param name="stage">The stage where to evaluate the gate.</param>
        /// <param name="e">The gate.</param>
        /// <param name="label">The label to refer to.</param>
        /// <param name="pstage"></param>
        public void SetAlias(int stage, IGate e, IGate r, int pstage)
        {
            e = e.Simplify();

            var id = GetCodeLabelIdentifier(stage, e);

            Debug.Assert(_gatemap.ContainsKey(r.ID));

            CodeLabel label;
            if(!_gatemap.TryGetValue(r.ID, out label))
            {
                throw new InvalidOperationException("label for gate not found.");
            }

            if (!_labels.ContainsKey(id))
            {
                _labels[id] = label;

                Gate.TraceLabel(e, label.Gate, stage + " alias");
            }
            else
            {
                //throw new Exception("label already defined.");
                Trace("label {0} already defined {0}, {1}", e, label.Gate);
            }


            AddToGateMap(e, label);
        }

        /// <summary>
        /// Emits code to initialize labels up to the given stage.
        /// </summary>
        /// <param name="writer">To code writer where to evaluate the labels to.</param>
        /// <param name="stage">Evaluate variables scheduled up to this stage.</param>
        public void Emit(ICodeLabelEvaluator cg, int stage)
        {
            var labels = _queue.Where(l => l.Stage <= stage).ToList();
            foreach(var label in labels)
            {
                Gate.TraceLabel(label.Gate, label.OriginalGate, "emit");
                label.Evaluate(cg);
                _queue.Remove(label);
            }
        }

        /// <summary>
        /// Creates a code label for a gate.
        /// </summary>
        /// <param name="c">The gate.</param>
        /// <param name="stage">The stage where the evaluation takes place.</param>
        /// <returns></returns>
        public CodeLabel AddGate(int stage, IGate g)
        {
            var label = AddCodeLabel(stage, g, true);

            return label;
        }

        public IGate ReplaceWithLabelIf(IGate g)
        {
            if (!g.Type.IsFixed())
            {
                CodeLabel label;
                if (_gatemap.TryGetValue(g.ID, out label))
                {
                    if (label.IsEvaluated)
                    {
                        g = label.Gate;
                    }
                }

            }

            return g;
        }

        public void SetNextStage()
        {
            CurrentStage++;
        }

        #endregion

        #region Private Methods

        private IGate MakeCodeLabels(int stage, IGate g)
        {
            var r = g.Replace(e => ReplaceWithCodeLabel(stage, e));

            r = r.Simplify();

            // Gate.TraceLabel(g, r, "makelabels");

            return r;
        }

        private string GetCodeLabelIdentifier(int stage, IGate e)
        {
            var id = e.ID;
            Debug.Assert(null != id);

            var labelkey = "$" + stage + ":" + id;

            return labelkey;
        }

        private CodeLabel AddCodeLabel(int stage, IGate g, bool makelabels)
        {
            CodeLabel label;
            var e = g;

            var labelkey = GetCodeLabelIdentifier(stage, g);

            if (makelabels)
            {
                e = MakeCodeLabels(stage, e);

                e = Gate.Simplify(e);
            }

            if(!_labels.ContainsKey(labelkey))
            {
                // not found in table ...
                if (null != OnBeforeAddLabel)
                {
                    // client may do something about it
                    var r = OnBeforeAddLabel(stage, e);

                    if(r != e)
                    {
                        if(_gatemap.TryGetValue(r.ID, out label))
                        {
                            return label;
                        }
                        else
                        {
                            throw new Exception("unexpected, gate not found.");
                        }
                    }
                }
            }


            if (!_labels.TryGetValue(labelkey, out label))
            {
                if (null == label)
                {
                    // construct a new label
                    var labelindex = _lseed++;
                    //var name = "u_" + stage + "_" + labelindex;
                    var name = "_c" + labelindex;

                    var newlabel = new CodeLabel(this, stage, labelindex, name, e);

                    // Gate.TraceLabel(g, newlabel.Gate, stage + " create  {0} ...", newlabel);

                    // look for a reduced label.
                    var r = newlabel.Gate;
                    var lg = newlabel.Gate as LabelGate;
                    if (null == lg)
                    {
                        var derivedkey = GetCodeLabelIdentifier(stage, r);

                        if (!_labels.TryGetValue(derivedkey, out label))
                        {
                            _labels[derivedkey] = newlabel;

                            Gate.TraceLabel(g, newlabel.Gate, stage + " insert <" + derivedkey + ">");

                            AddToGateMap(r, label);
                        }
                        else
                        {
                            // forget
                            newlabel = null;
                        }

                    }
                    else
                    {

                    }

                    if (null != newlabel)
                    {
                        label = newlabel;
                        _labels[labelkey] = label;

                        AddToGateMap(label.Gate, label);

                        Gate.TraceLabel(g, newlabel.Gate, stage + " insert <" + labelkey + ">");
                    }

                    AddToGateMap(e, label);
                }
            }

            AddToGateMap(g, label);

            return label;
        }

        /// <summary>
        /// Replacer function for gate to code replacement.
        /// </summary>
        /// <param name="stage"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private IGate ReplaceWithCodeLabel(int stage, IGate e)
        {
            if (e is LabelGate)
            {
                return e;
            }

            var e0 = e;

            e = e.Simplify();

            int maxstage = stage;

            var children = e.SelectAll().OfType<LabelGate>();
            if (children.Any())
            {
                maxstage = children.Max(u => u.Label.Stage);
            }

            var label = AddCodeLabel(maxstage, e, false);

            if (e.Type == GateType.Input)
            {
                // leaf level of replacement, just refer to the new variable.
                e = label.Gate;
                // Gate.TraceLabel(e0, e, stage + " replace");
            }
            else
            {
                // guaranteed to contain only code labels, add to gatemap
            }

            return e;
        }

        private bool ShouldCodeLabelBeEvaluated(CodeLabel label, int stage)
        {
            if (label.Stage == stage)
            {
                var scount = label.ScheduleCount;

                // Trace("should {0} {1}", label.OriginalGate, label.OriginalGate.GetType().Name);

                if (label.OriginalGate.Type == GateType.Input && scount > 0)
                {
                    return true;
                }
                else if (scount > 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private void AddToGateMap(IGate gate, CodeLabel label)
        {
            gate = gate.Simplify();

            if (!_gatemap.ContainsKey(gate.ID))
            {
                _gatemap.Add(gate.ID, label);
            }
        }

        #endregion
    }
}
