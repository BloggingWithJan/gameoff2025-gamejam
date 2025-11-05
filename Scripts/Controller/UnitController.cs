using GameJam.Core;
using GameJam.Worker;
using UnityEngine;

namespace GameJam.Controller
{
    public class UnitController : MonoBehaviour
    {
        private Health health;

        void Start()
        {
            health = GetComponent<Health>();
        }

        void Update()
        {
            if (health.IsDead())
                return;

            GathererBehaviour();
        }

        private void GathererBehaviour()
        {
            Gatherer gatherer = GetComponent<Gatherer>();
            if (gatherer == null)
                return;

            gatherer.Gather();
        }
    }
}
