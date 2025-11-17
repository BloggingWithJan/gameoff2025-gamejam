using GameJam.Core;
using UnityEngine;

namespace GameJam.Combat
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField]
        float speed = 1f;

        [SerializeField]
        float maxDistance = 15f;

        [SerializeField]
        bool isHoming = true;

        Health target = null;
        float damage = 0;

        private Vector3 startPosition;

        void Start()
        {
            startPosition = transform.position;
            transform.LookAt(GetAimLocation());
        }

        // Update is called once per frame
        void Update()
        {
            if (target == null)
            {
                return;
            }

            if (isHoming && !target.IsDead())
            {
                transform.LookAt(GetAimLocation());
            }

            transform.Translate(Vector3.forward * speed * Time.deltaTime);

            //destory object itself after certain distance
            if (Vector3.Distance(startPosition, transform.position) > maxDistance)
            {
                Destroy(gameObject);
            }
        }

        private Vector3 GetAimLocation()
        {
            CapsuleCollider targetCapsule = target.GetComponent<CapsuleCollider>();
            if (targetCapsule == null)
            {
                return target.transform.position;
            }

            return target.transform.position + Vector3.up * targetCapsule.height / 2;
        }

        public void SetTarget(Health target, float damage)
        {
            this.target = target;
            this.damage = damage;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<Health>() != target)
            {
                return;
            }
            target.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
