using sjjasonliu.RTS.EventBus;
using sjjasonliu.RTS.Events;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace sjjasonliu.RTS.Units
{
    public abstract class AbstractCommandable : MonoBehaviour, ISelectable
    {
        [SerializeField] private DecalProjector _decalProjector;
        [field: SerializeField] public int Health { get; private set; }

        public void Deselect()
        {
            if (_decalProjector != null)
            {
                _decalProjector.gameObject.SetActive(false);
            }
            Bus<UnitDeselectedEvent>.Raise(new UnitDeselectedEvent(this));
        }

        public void Select()
        {
            if (_decalProjector != null)
            {
                _decalProjector.gameObject.SetActive(true);
            }
            Bus<UnitSelectedEvent>.Raise(new UnitSelectedEvent(this));
        }
    }
}