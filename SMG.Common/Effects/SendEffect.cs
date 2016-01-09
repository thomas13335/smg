using SMG.Common.Transitions;

namespace SMG.Common.Effects
{
    /// <summary>
    /// Posts a effect for postprocessing.
    /// </summary>
    public class SendEffect : Effect
    {
        public override string UniqueID
        {
            get { return "SEND " + Event.Name; }
        }

        /// <summary>
        /// The event corresponding to this send operation.
        /// </summary>
        public Event Event { get; private set; }

        public SendEffect(StateMachine sm, string name)
        {
            Event = sm.AddEvent(name);
        }
    }
}
