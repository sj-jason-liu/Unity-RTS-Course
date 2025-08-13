using sjjasonliu.RTS.EventBus;
using sjjasonliu.RTS.Events;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace sjjasonliu.RTS.Units
{
    public abstract class AbstractCommandable : MonoBehaviour, ISelectable
    {
        [field: SerializeField] public int CurrentHealth { get; private set; }
        [field: SerializeField] public int MaxHealth { get; private set; }
        [SerializeField] private DecalProjector _decalProjector;
        [SerializeField] private UnitSO _unitSO;

        protected virtual void Start()
        {
            if (_unitSO != null) //if UnitSO is assigned
            {
                CurrentHealth = _unitSO.Health;
                MaxHealth = _unitSO.Health;
            }          
        }

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