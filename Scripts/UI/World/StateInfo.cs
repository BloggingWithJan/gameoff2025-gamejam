using System;
using System.Collections.Generic;
using GameJam.Combat;
using GameJam.Core;
using GameJam.Movement;
using GameJam.Production;
using Production;
using UnityEngine;

namespace GameJam.UI
{
    public class StateInfo : MonoBehaviour
    {
        [SerializeField]
        GameObject unit;

        [SerializeField]
        GameObject worldSpaceCanvas;

        private ActionScheduler actionScheduler;
        private Gatherer gatherer;
        private Fighter fighter;
        private Mover mover;
        private Health health;

        void Start()
        {
            actionScheduler = unit.GetComponent<ActionScheduler>();
            gatherer = unit.GetComponent<Gatherer>();
            fighter = unit.GetComponent<Fighter>();
            mover = unit.GetComponent<Mover>();
            health = unit.GetComponent<Health>();
        }

        void LateUpdate()
        {
            if (actionScheduler == null)
                return;
            if (health.IsDead())
            {
                worldSpaceCanvas.SetActive(false);
                return;
            }

            if (actionScheduler.GetCurrentAction() is null)
            {
                worldSpaceCanvas.SetActive(true);
            }
            else if (actionScheduler.GetCurrentAction() is Mover && mover.IsDestinationReached())
            {
                worldSpaceCanvas.SetActive(true);
            }
            else if (actionScheduler.GetCurrentAction() is Fighter && fighter.IsCurrentTargetDead())
            {
                worldSpaceCanvas.SetActive(true);
            }
            else
            {
                worldSpaceCanvas.SetActive(false);
            }
        }
    }
}
