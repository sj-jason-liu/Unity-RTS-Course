using sjjasonliu.RTS.Units;
using UnityEngine;

namespace sjjasonliu.RTS.Commands
{
    [CreateAssetMenu(fileName = "Move Action", menuName = "AI/Actions/Move", order = 100)]
    public class MoveCommand : ActionBase
    {
        public override bool CanHandle(AbstractCommandable commandable, RaycastHit hit)
        {
            return commandable is IMoveable;
        }

        public override void Handle(AbstractCommandable commandable, RaycastHit hit)
        {
            IMoveable moveable = (IMoveable)commandable;
            moveable.MoveTo(hit.point);
        }
    }
}