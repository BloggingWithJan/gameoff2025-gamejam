using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

namespace Controller
{
    public class GhostShipController : MonoBehaviour
    {
        [Header("Timing")]
        public float riseDuration = 2f; // coming out of water
        public float rideDuration = 2.5f; // floating toward island
        public float diveDuration = 2f; // crashing down

        [Header("Crash Settings")]
        public float crashDepth = 50f; // how far below the island the ship sinks
        public GameObject diveExplosionPrefab; // assign explosion prefab in inspector

        [Header("Ride Settings")]
        [Tooltip("Adjusts ship height while riding toward island (negative = lower in water)")]
        public float rideHeightOffset = -20f;

        [Header("Entry Settings")]
        [Tooltip("How far below the water the ship starts")]
        public float startDepth = 30;

        [Header("Audio")]
        public AudioSource crashAudioSource;

        Vector3 startPos,
            midPos,
            crashPos;
        Quaternion midRot,
            diveRot;

        public System.Action onCrash;

        public void StartSequence(
            Vector3 entryPoint,
            Vector3 destination,
            float waterLevel,
            CinemachineCamera introCamera
        )
        {
            // Start below water
            startPos = entryPoint;
            startPos.y = waterLevel - startDepth;

            // Crash position below the island
            crashPos = destination - new Vector3(0, crashDepth, 0);

            // Mid position: floating above water
            midPos = new Vector3(entryPoint.x, waterLevel + 5f, entryPoint.z);

            // Orient ship to face destination
            Quaternion lookAtDest = Quaternion.LookRotation(destination - entryPoint);
            midRot = lookAtDest;

            // Dive: lean forward + face destination
            diveRot = Quaternion.Euler(35f, lookAtDest.eulerAngles.y, 0);

            // Start at below water position
            transform.position = startPos;
            transform.rotation = midRot;

            StartCoroutine(Sequence());
        }

        IEnumerator Sequence()
        {
            float t;

            // Rise: straight up from water
            t = 0;
            Vector3 riseTarget = midPos;
            while (t < riseDuration)
            {
                t += Time.deltaTime;
                float k = t / riseDuration;
                transform.position = Vector3.Lerp(startPos, riseTarget, k);
                yield return null;
            }

            // Ride: float toward island (slightly lower in water)
            t = 0;
            Vector3 rideStart = riseTarget;
            Vector3 rideTarget = new Vector3(crashPos.x, midPos.y + rideHeightOffset, crashPos.z);
            while (t < rideDuration)
            {
                t += Time.deltaTime;
                float k = t / rideDuration;
                transform.position = Vector3.Lerp(rideStart, rideTarget, k);
                transform.rotation = midRot;
                yield return null;
            }

            // Dive: lean forward and crash below island surface
            t = 0;
            Vector3 diveStart = transform.position;
            bool explosionTriggered = false;
            onCrash?.Invoke();
            while (t < diveDuration)
            {
                t += Time.deltaTime;
                float k = t / diveDuration;
                transform.position = Vector3.Lerp(diveStart, crashPos, k);
                transform.rotation = Quaternion.Slerp(midRot, diveRot, k);

                // Trigger explosion at spawn point
                if (!explosionTriggered && k >= 0.01f)
                {
                    explosionTriggered = true;

                    if (diveExplosionPrefab != null)
                        Instantiate(
                            diveExplosionPrefab,
                            new Vector3(crashPos.x, crashPos.y + crashDepth, crashPos.z),
                            Quaternion.identity
                        );

                    if (crashAudioSource != null)
                        crashAudioSource.Play();
                }

                yield return null;
            }

            Destroy(gameObject, 0.5f);
        }
    }
}
