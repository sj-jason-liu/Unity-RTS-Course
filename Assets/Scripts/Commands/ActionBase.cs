using sjjasonliu.RTS.Units;
using UnityEngine;

namespace sjjasonliu.RTS.Commands
{
    public abstract class ActionBase : ScriptableObject, ICommand
    {
        public abstract bool CanHandle(AbstractCommandable commandable, RaycastHit hit);

        public abstract void Handle(AbstractCommandable commandable, RaycastHit hit);
    }
}