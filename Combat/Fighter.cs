using Tears_Of_Void.Movement;
using Tears_Of_Void.Core;
using Tears_Of_Void.Resources;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using Tears_Of_Void.Stats;
using DuloGames.UI;
using Tears_Of_Void.Control;
using Tears_Of_Void.Items;
using Tears_Of_Void.VFX;

namespace Tears_Of_Void.Combat
{
    public class Fighter : MonoBehaviour, IAction, IModifierProvider
    {
        AIHealth attackTarget, selectedTarget;
        [SerializeField] float timeBetweenAttacks = 0.5f;
        [SerializeField] Transform handTransform = null;
        [SerializeField] Weapon defaultWeapon = null;
        [SerializeField] Text enemyTextName;
        [SerializeField] GameObject targetUI, dragon, playerUIFrameBuffTooltip;
        [SerializeField] Text playerUIFrameBuffTooltipText;
        ActionScheduler actionScheduler;
        Mover moveAgent;
        float timeSinceLastAttack = Mathf.Infinity;
        LazyValue<float> strength;
        // References
        Weapon currentWeapon = null;
        public BaseStats baseStats;
        float attackDmg;
        bool isCritical = false;
        public float finalDamage { get; set; }
        bool playerIsCasting, isSEActive;
        float critChance, critDmg;
        public event Action onDamageTaken;
        Animator animator;
        UIEquipSlot weaponSlot;
        Health health;
        [SerializeField] Weapons weapon;
        PlayerControls player;
        VFXManager.VFXInstance m_Hits;
        const string ATTACK_TRIGGER = "attack";
        const string DEFAULT_ATTACK = "Sword-Attack-R6";
        [SerializeField] public GameObject instigator;
        // UI
        [SerializeField] Text strengthUIText = default, damageUIText = default;
        AudioSource audioSource;

        private void Awake()
        {
            health = GetComponent<Health>();
            audioSource = GetComponent<AudioSource>();
            animator = GetComponent<Animator>();
            player = GetComponent<PlayerControls>();
            baseStats = GetComponent<BaseStats>();
            moveAgent = GetComponent<Mover>();
            strength = new LazyValue<float>(GetStrength);
        }
        public GameObject inst()
        {
            return instigator;
        }
        private void Start()
        {
            targetUI.SetActive(false);
            EquipWeapon(defaultWeapon);

            strengthUIText.text = strength.value.ToString();
            damageUIText.text = CalculateDamage().ToString();
            //SetAttackAnimation();
        }

        private void Update()
        {
            baseStats.Tick();
            timeSinceLastAttack += Time.deltaTime;
            EscToCancel();

            if (attackTarget == null) return;
            if (attackTarget.IsDead() || selectedTarget.IsDead())
            {
                selectedTarget = null;
                attackTarget = null;
                targetUI.SetActive(false);
                return;
            }

            if (GetIsInRange() && !playerIsCasting)
            {
                AttackBehaviour();
            }

        }
        private float GetStrength()
        {
            return baseStats.GetStat(Stat.Strength);
        }
        public bool isPlayerCasting(bool b)
        {
            return playerIsCasting = b;
        }

        public void SetStatAndUI()
        {
            strength.value = GetStrength();
            strengthUIText.text = strength.value.ToString();
            damageUIText.text = CalculateDamage().ToString();
            
        }

        public float CalculateDamage()
        {
            weaponSlot = UIEquipSlot.GetSlotWithType(UIEquipmentType.Weapon_MainHand);

            if (weaponSlot.GetItemInfo() != null) // If no weapon equipped, then damage will only depend on Strength
            {
                return Mathf.RoundToInt((strength.value * 1.5f) + UnityEngine.Random.Range(weaponSlot.GetItemInfo().MinDamage, weaponSlot.GetItemInfo().MaxDamage));
            }

            return Mathf.RoundToInt(strength.value * 1.5f);

        }

        public void ChangeHealth(float h)
        {
            health.AddHealth(h);
        }
        public bool GetIsInRange()
        {
            return Vector3.Distance(attackTarget.transform.position, transform.position) < currentWeapon.GetWeaponRange();
        }

        public bool isTargetSelected()
        {
            if (targetUI.activeSelf == true) return true;
            return false;
        }

        public void SetTarget(GameObject combatTarget)
        {
            enemyTextName.text = combatTarget.transform.name;
            selectedTarget = combatTarget.GetComponent<AIHealth>();
            selectedTarget.GetComponent<Renderer>().material.color = Color.red;
            targetUI.SetActive(true);

            if (combatTarget.tag == "BossEnemy")
            {
                dragon.SetActive(true);
            }
            else
            {
                dragon.SetActive(false);
            }
        }

        public void Attack()
        {
            GetComponent<ActionScheduler>().StartAction(this);
            attackTarget = selectedTarget;
        }

        public void Attack(GameObject combatTarget) // Used for direct right-click targeting/attacking
        {
            GetComponent<ActionScheduler>().StartAction(this);
            attackTarget = combatTarget.GetComponent<AIHealth>();
        }

        public void EquipWeapon(Weapon weapon)
        {
            currentWeapon = weapon;
            Animator animator = GetComponent<Animator>();
            weapon.Spawn(handTransform);
            animator.SetFloat("attackSpeed", weapon.GetWeaponSpeed());
        }

        public AIHealth GetTarget()
        {
            return selectedTarget;
        }
        public Vector3 GetTargetPosition()
        {
            return attackTarget.transform.position;
        }
        public void Cancel()
        {
            StopAttack();
            attackTarget = null;
        }

        public void EscToCancel()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                targetUI.SetActive(false);
                attackTarget = null;
                selectedTarget = null;
                StopAttack();
            }
        }

        public void StopAttack()
        {
            GetComponent<Animator>().ResetTrigger("attack");
            //GetComponent<Animator>().SetTrigger("stopAttack");
        }

        public IEnumerable<float> GetAdditiveModifier(Stat stat)
        {
            float sum = 0;

            if (stat == Stat.Strength)
            {
                foreach (UIEquipSlot slot in UIEquipSlot.GetSlots())
                {
                    if (slot.GetItemInfo() != null)
                    {
                        sum += slot.GetItemInfo().Strength;
                    }
                }
            }

            yield return sum;
        }

        void AttackBehaviour()
        {
            SetAttackAnimation();
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

        private void SetAttackAnimation()
        {
            //protect against animator override controller
            if (!player.GetAnimatorOverride())
            {
                Debug.Break();
                Debug.LogAssertion("Please Provide " + gameObject.name + " with Animator Override Controller ");
            }
            else
            {
                var animatorOverrideController = player.GetAnimatorOverride();
                animator.runtimeAnimatorController = animatorOverrideController;
                animatorOverrideController[DEFAULT_ATTACK] = currentWeapon.GetAnimClip();
            }
        }


        // Animation Event
        void Hit()
        {
            if (attackTarget == null || !GetIsInRange()) return;
            if (!weaponSlot.IsAssigned()) return;
            critChance = baseStats.GetStat(Stat.CriticalChance) + (weaponSlot.GetItemInfo().CriticalChance / 100);

            weapon.CalculateSpecialsEffectAndExtraElementalDamages(this, selectedTarget);
            // Add other possible methods that might affect damage here
            attackDmg = CalculateDamage();
            //CalculateSpecialEffects();
            if (UnityEngine.Random.Range(0.0f, 1.0f) <= critChance)
            {
                critDmg = baseStats.GetStat(Stat.CriticalDamage) + (weaponSlot.GetItemInfo().CriticalDamage / 100);
                attackDmg *= critDmg;
                isCritical = true;
                finalDamage = attackDmg;
            }
            m_Hits = VFXManager.GetVFX(VFXType.Hit);
            m_Hits.Effect.transform.position = attackTarget.transform.position + Vector3.up * 1.2f;
            StartCoroutine(DisableVFX());
            finalDamage = attackDmg;
            attackTarget.TakeDamage(gameObject, finalDamage, isCritical);
            audioSource.PlayOneShot(currentWeapon.GetAudioClip(), 0.7F);
            isCritical = false; // Resets flag
            onDamageTaken();

        }

        // private void CalculateSpecialEffects()
        // {
        //     if(!weaponSlot.IsAssigned()) return;
        //     critChance = baseStats.GetStat(Stat.CriticalChance) + (weaponSlot.GetItemInfo().CriticalChance / 100);

        //     if (UnityEngine.Random.Range(0.0f, 1.0f) <= weaponSlot.GetItemInfo().SpecialEffectProcChance / 100 && !isSEActive)
        //     {
        //         StartCoroutine(WeaponSpecialEffect());
        //     }
        //     else
        //     {
        //         critDmg = baseStats.GetStat(Stat.CriticalDamage) + (weaponSlot.GetItemInfo().CriticalDamage / 100);
        //     }

        //     if (isSEActive)
        //     {
        //         critDmg = baseStats.GetStat(Stat.CriticalDamage) + (weaponSlot.GetItemInfo().CriticalDamage / 100) + 1;
        //     }
        // }

        private IEnumerator WeaponSpecialEffect()
        {
            isSEActive = true;
            int specialEffectExpireTimer = 5;
            while (specialEffectExpireTimer > 0)
            {
                playerUIFrameBuffTooltip.SetActive(true);
                playerUIFrameBuffTooltipText.text = specialEffectExpireTimer.ToString();
                yield return new WaitForSeconds(1);
                specialEffectExpireTimer--;
            }
            playerUIFrameBuffTooltip.SetActive(false);
            isSEActive = false;
        }

        public bool CanAttack(GameObject combatTarget)
        {
            if (combatTarget == null)
            {
                return false;
            }
            AIHealth targetToTest = combatTarget.GetComponent<AIHealth>();
            return targetToTest != null && !targetToTest.IsDead();
        }

        IEnumerator DisableVFX()
        {
            yield return new WaitForSeconds(1f);
            m_Hits.Effect.SetActive(false);
        }
    }
}
