using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.Universal;

namespace sjjasonliu.RTS.Units
{
    [RequireComponent(typeof(NavMeshAgent))] //check if NavMeshAgent component exists, if not, add it
    public class Worker : MonoBehaviour, ISelectable
    {
        [SerializeField] private Transform _target;
        [SerializeField] private DecalProjector _decalProjector;
        private NavMeshAgent _agent;

        public void Deselect()
        {
            if (_decalProjector != null)
                _decalProjector.gameObject.SetActive(false); //hide the selection decal when deselected
        }

        public void Select()
        {
            if (_decalProjector != null)
                _decalProjector.gameObject.SetActive(true); //show the selection decal when selected
        }

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();    
        }

        private void Update()
        {
            if(_target != null)
            {
                _agent.SetDestination(_target.position);
            }
        }
    }
}
