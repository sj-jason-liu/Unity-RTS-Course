using sjjasonliu.RTS.EventBus;
using sjjasonliu.RTS.Units;

namespace sjjasonliu.RTS.Events
{
    public struct UnitSelectedEvent : IEvent
    {
        public ISelectable Unit { get; private set; } // The unit that was selected

        public UnitSelectedEvent(ISelectable unit) 
        {
            Unit = unit;
        }
    }
}