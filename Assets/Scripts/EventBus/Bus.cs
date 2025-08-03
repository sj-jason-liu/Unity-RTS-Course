namespace sjjasonliu.RTS.EventBus
{
    public static class Bus<T> where T : IEvent
    {
        public delegate void Event(T args);
        public static event Event OnEvent;

        // Method to raise an event
        public static void Raise(T evt) => OnEvent?.Invoke(evt);
    }
}