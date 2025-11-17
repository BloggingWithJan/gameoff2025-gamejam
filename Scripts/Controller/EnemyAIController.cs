//-----------------------------------------------------------------------
//Anforderungen
//Enemys werden irgendwo in der Welt gespawnt
//Enemys müssen beim Start sofort ein ZIel suchen (also ein Target)
//Alle Enemeys sollten das gleiche Ziel haben (z.B. die Basis der Player)
//sobald sich military units in Reichweite befinden, sollten Enemys diese angreifen
//
//-----------------------------------------------------------------------
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

        // AttackBehaviour();
    }

    private void AttackBehaviour()
    {
        if (fighter.CanAttack(target))
        {
            fighter.Attack(target);
        }
    }

    //methode die einfach ein target aussucht - zuerst einfach das objekt was am nächsten ist
    //woher weiß enemy was angreifbar ist - alles was ne health componente hat? und kein enemy ist? dann greift er auch tiere an
}
