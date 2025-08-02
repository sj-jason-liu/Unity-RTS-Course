using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.Universal;

namespace sjjasonliu.RTS.Units
{
    [RequireComponent(typeof(NavMeshAgent))] //check if NavMeshAgent component exists, if not, add it
    public class Worker : MonoBehaviour, ISelectable, IMoveable
    {
        [SerializeField] private DecalProjector _decalProjector;
        private NavMeshAgent _agent;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
        }

        public void MoveTo(Vector3 position)
        {
            _agent.SetDestination(position); //set the target position to the given position
        }

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
    }
}
