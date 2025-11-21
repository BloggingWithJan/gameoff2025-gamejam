using GameJam.Combat;
using GameJam.Core;
using UnityEngine;

public class EnemyAIController : MonoBehaviour
{
    private Health health;

    private EnemySoldier enemySoldier;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        health = GetComponent<Health>();
        enemySoldier = GetComponent<EnemySoldier>();
        if (enemySoldier != null)
        {
            enemySoldier.Slay();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (health.IsDead())
            return;

        // AttackBehaviour();
    }
}
