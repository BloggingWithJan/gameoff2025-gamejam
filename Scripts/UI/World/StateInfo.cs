using System;
using System.Collections.Generic;
using GameJam.Combat;
using GameJam.Core;
using GameJam.Movement;
using GameJam.Production;
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

        void Start()
        {
            actionScheduler = unit.GetComponent<ActionScheduler>();
            gatherer = unit.GetComponent<Gatherer>();
            fighter = unit.GetComponent<Fighter>();
            mover = unit.GetComponent<Mover>();
        }

        void LateUpdate()
        {
            if (actionScheduler == null)
                return;

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
