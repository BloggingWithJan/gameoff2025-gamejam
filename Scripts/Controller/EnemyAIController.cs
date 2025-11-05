using GameJam.Combat;
using GameJam.Core;
using UnityEngine;

public class EnemyAIController : MonoBehaviour
{
    //only temporary
    [SerializeField]
    GameObject target;
    private Health health;
    private Fighter fighter;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        health = GetComponent<Health>();
        fighter = GetComponent<Fighter>();
    }

    // Update is called once per frame
    void Update()
    {
        if (health.IsDead())
            return;

        AttackBehavior();
    }

    private void AttackBehavior()
    {
        if (fighter.CanAttack(target))
        {
            fighter.Attack(target);
        }
    }

    //methode die einfach ein target aussucht - zuerst einfach das objekt was am nächsten ist
    //woher weiß enemy was angreifbar ist - alles was ne health componente hat? und kein enemy ist? dann greift er auch tiere an
}
