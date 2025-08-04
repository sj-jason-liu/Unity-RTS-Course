using sjjasonliu.RTS.EventBus;
using sjjasonliu.RTS.Units;

namespace sjjasonliu.RTS.Events
{
    public struct UnitSpawnEvent : IEvent
    {
        public AbstractUnit Unit { get; private set; } // The unit that was selected

        public UnitSpawnEvent(AbstractUnit unit) 
        {
            Unit = unit;
        }
    }
}