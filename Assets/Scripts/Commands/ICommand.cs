using sjjasonliu.RTS.Units;
using UnityEngine;

namespace sjjasonliu.RTS.Commands
{
    public interface ICommand
    {
        bool CanHandle(AbstractCommandable commandable, RaycastHit hit);
        void Handle(AbstractCommandable commandable, RaycastHit hit);
    }
}