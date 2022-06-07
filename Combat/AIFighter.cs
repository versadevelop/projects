using Tears_Of_Void.Movement;
using Tears_Of_Void.Core;
using Tears_Of_Void.Resources;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Tears_Of_Void.Stats;

namespace Tears_Of_Void.Combat
{
    public class AIFighter : MonoBehaviour, IAction
    {
        Health target;
        [SerializeField] float timeBetweenAttacks = 0.5f;
        [SerializeField] Transform handTransform = null;
        [SerializeField] Weapon defaultWeapon = null;

        Mover moveAgent;
        float timeSinceLastAttack = Mathf.Infinity;

        Weapon currentWeapon = null;
        private void Start()
        {
            EquipWeapon(defaultWeapon);
            moveAgent = GetComponent<Mover>();
        }

        public void EquipWeapon(Weapon weapon)
        {
            currentWeapon = weapon;
            Animator animator = GetComponent<Animator>();
            weapon.Spawn(handTransform);
        }

        private void Update()
        {
            timeSinceLastAttack += Time.deltaTime;
            //isAttackDodged();
            if (target == null) return;
            if (target.IsDead())
            {
                return;
            }

            
            if (!GetIsInRange())
            {
                moveAgent.MoveTo(target.transform.position, 1f);

            }
            else
            {
                moveAgent.Cancel();
                AttackBehaviour();
            }
        }

        private bool GetIsInRange()
        {
            return Vector3.Distance(target.transform.position, transform.position) < currentWeapon.GetWeaponRange();
        }

        public void Attack(GameObject combatTarget)
        {
            GetComponent<ActionScheduler>().StartAction(this);
            target = combatTarget.GetComponent<Health>();
        }

        public void Cancel()
        {
            StopAttack();
            target = null;
            moveAgent.Cancel();
        }

        private void StopAttack()
        {
            GetComponent<Animator>().ResetTrigger("attack");
            GetComponent<Animator>().SetTrigger("stopAttack");
        }

        void AttackBehaviour()
        {
            transform.LookAt(target.transform);
            if (timeSinceLastAttack > timeBetweenAttacks)
            {
                TriggerAttack();
                timeSinceLastAttack = 0;
            }

        }

        private void TriggerAttack()
        {
            GetComponent<Animator>().ResetTrigger("stopAttack");
            GetComponent<Animator>().SetTrigger("attack");

        }

        // Animation Event
        void Hit()
        {
            if (target == null) return;

            // TODO: Implement critical chance of AI
            target.TakeDamage(gameObject, currentWeapon.GetWeaponDamage(), false);
        }

        public bool CanAttack(GameObject combatTarget)
        {
            if (combatTarget == null)
            {
                return false;
            }
            Health targtetToTest = combatTarget.GetComponent<Health>();
            return targtetToTest != null && !targtetToTest.IsDead();
        }
    }
}
