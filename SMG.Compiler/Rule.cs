using SMG.Common;
using SMG.Common.Conditions;
using SMG.Common.Effects;
using SMG.Common.Exceptions;
using SMG.Common.Transitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMG.Compiler
{
    class Rule
    {
        public EffectList Effects = new EffectList();

        public ICondition Condition { get; set; }

        public Event Event { get; set; }

        public Rule()
        {
        }

        public Rule(Rule top, bool trigger)
        {
            if (trigger)
            {
                Event = top.Event;
            }

            Condition = top.Condition;
        }

        public void AddEffect(Effect effect)
        {
            Effects.Add(effect);
        }

        public void CreateTrigger(StateMachine sm)
        {
            if (null != Condition)
            {
                var trigger = new Trigger(Event, Condition);
                trigger.AddEffects(Effects);
                sm.AddTrigger(trigger);
            }
            else if(Effects.Any())
            {
                var trigger = new Trigger(Event, new AnyCondition());
                trigger.AddEffects(Effects);
                sm.AddTrigger(trigger);
            }
        }

        public void CreateGuard(StateMachine sm, string name, GuardType type)
        {
            var guard = sm.AddGuard(Condition, type, name);
            guard.AddEffects(Effects);
        }

        public void NestCondition(ICondition cond)
        {
            if (null != Condition)
            {
                Condition = Condition.Intersection(cond);
            }
            else
            {
                Condition = cond;
            }
        }

        public void CreateNested(StateMachine sm)
        {
            if(null != Event)
            {
                CreateTrigger(sm);
            }
            else
            {
                throw new CompilerException(ErrorCode.Unsupported, "nested guard is unsupported.");
            }
        }
    }
}
