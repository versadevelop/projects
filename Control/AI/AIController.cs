using Tears_Of_Void.Combat;
using Tears_Of_Void.Movement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tears_Of_Void.Core;
using Tears_Of_Void.Resources;
using System;

namespace Tears_Of_Void.Control
{
    public class AIController : MonoBehaviour
    {
        [SerializeField] float chaseDistance = 5f;
        [SerializeField] float suspicionTime = 3f;
        [SerializeField] float PatrolDelay = 1f;
        [SerializeField] PatrolPath patrolPath = default;
        [SerializeField] float wayPointTolerance = 1f;
        [Range(0, 1)]
        [SerializeField] float patrolSpeedFraction = 0.2f;
        AIFighter fighter;
        GameObject player;
        Mover move;
        AIHealth aiHealth;
        Vector3 guardPosition;
        float timeSinceLastSawPlayer = Mathf.Infinity;
        float timeSinceLastArriveWaypoint = Mathf.Infinity;
        int currentWaypointIndex = 0;

        private void Awake()
        {
            fighter = GetComponent<AIFighter>();
            aiHealth = GetComponent<AIHealth>();
            move = GetComponent<Mover>();
            player = GameObject.FindWithTag("Player");
        }
        private void Start()
        {
            guardPosition = transform.position;
        }
        private void Update()
        {
            if (aiHealth.IsDead()) return;
            if ((InAttackRangeOfPlayer() && fighter.CanAttack(player)) || aiHealth.gotDamaged())
            {
                AttackBehaviour();
            }
            else if (timeSinceLastSawPlayer < suspicionTime)
            {
                SuspicionBehaviour();
            }
            else
            {
                PatrolBehaviour();
            }
            UpdateTimers();
        }

        private void UpdateTimers()
        {
            timeSinceLastSawPlayer += Time.deltaTime;
            timeSinceLastArriveWaypoint += Time.deltaTime;
        }

        private void PatrolBehaviour()
        {
            Vector3 nextPosition = guardPosition;

            if (patrolPath != null)
            {
                if (AtWaypoint())
                {
                    timeSinceLastArriveWaypoint = 0;
                    CycleWaypoint();
                }
                nextPosition = GetCurrentWaypoint();
            }
            if (timeSinceLastArriveWaypoint > PatrolDelay)
            {
                move.StartMoveAction(nextPosition, patrolSpeedFraction);
            }


        }

        private Vector3 GetCurrentWaypoint()
        {
            return patrolPath.GetWaypoint(currentWaypointIndex);
        }

        private void CycleWaypoint()
        {

            currentWaypointIndex = patrolPath.GetNextIndex(currentWaypointIndex);


        }

        private bool AtWaypoint()
        {

            float distanceToWaypoint = Vector3.Distance(transform.position, GetCurrentWaypoint());
            return distanceToWaypoint < wayPointTolerance;
        }

        private void SuspicionBehaviour()
        {
            GetComponent<ActionScheduler>().CancelCurrentAction();
        }

        private void AttackBehaviour()
        {
            timeSinceLastSawPlayer = 0;
            fighter.Attack(player);
        }

        private bool InAttackRangeOfPlayer()
        {
            return Vector3.Distance(transform.position, player.transform.position) < chaseDistance;
        }

        //Called by Unity
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, chaseDistance);
        }
    }
}