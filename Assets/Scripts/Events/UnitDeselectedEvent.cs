using sjjasonliu.RTS.EventBus;
using sjjasonliu.RTS.Units;

namespace sjjasonliu.RTS.Events
{
    public struct UnitDeselectedEvent : IEvent
    {
        public ISelectable Unit { get; private set; } // The unit that was deselected

        public UnitDeselectedEvent(ISelectable unit)
        {
            Unit = unit;
        }
    }
}