using sjjasonliu.RTS.EventBus;
using sjjasonliu.RTS.Events;
using UnityEngine;
using UnityEngine.AI;

namespace sjjasonliu.RTS.Units
{
    [RequireComponent(typeof(NavMeshAgent))] //check if NavMeshAgent component exists, if not, add it
    public abstract class AbstractUnit : AbstractCommandable, IMoveable
    {
        public float AgentRadius => _agent.radius;
        private NavMeshAgent _agent;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
        }

        private void Start()
        {
            Bus<UnitSpawnEvent>.Raise(new UnitSpawnEvent(this));
        }

        public void MoveTo(Vector3 position)
        {
            _agent.SetDestination(position); //set the target position to the given position
        }
    }
}