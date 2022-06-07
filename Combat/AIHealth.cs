using System;
using System.Collections;
using System.Collections.Generic;
using Tears_Of_Void.Core;
using Tears_Of_Void.Items;
using Tears_Of_Void.Resources;
using Tears_Of_Void.Saving;
using Tears_Of_Void.Stats;
using UnityEngine;

namespace Tears_Of_Void.Combat
{
    public class AIHealth : MonoBehaviour, ISaveable
    {
        bool isDead = false;
        LazyValue<float> maxHealth;
        float currentHealth;
        CapsuleCollider cc;
        public DamageUI damageUI;
        Experience experience;
        public BaseStats baseStats;
        HealthDisplay healthDisplay;
        EnemyHealthDisplay enemyHealth;
        public event Action onDamageTaken;
        [SerializeField] GameObject floatingDmgText = default;
        GameObject floatingDmg;

        bool trigger = false;

        public bool IsDead()
        {
            return isDead;
        }
        
        void Awake()
        {
            baseStats = GetComponent<BaseStats>();
            maxHealth = new LazyValue<float>(GetMaxHealth);
            cc = GetComponent<CapsuleCollider>();
            healthDisplay = GameObject.FindWithTag("HealthBar").GetComponent<HealthDisplay>();
            enemyHealth = GameObject.FindWithTag("EnemyHealthBar").GetComponent<EnemyHealthDisplay>();

            maxHealth.ForceInit();
            currentHealth = maxHealth.value;
        }

        private float GetMaxHealth()
        {
            return baseStats.GetStat(Stat.Health);
        }

        public bool isTargetDead()
        {
            return isDead;
        }

        public float GetCurrentHealth()
        {
            return currentHealth;
        }
        public void ChangeCurrentHealth(float h)
        {
            currentHealth = h;
        }

        public bool isAttackDodged() // Might be needed if we want AI to dodge, or evade, or something
        {
            float dodge = baseStats.GetStat(Stat.Dodge) * 100;
            bool dodged = UnityEngine.Random.Range(1, 100) < dodge;
            return dodged;
        }

        public void TakeDamage(GameObject instigator, float damage, bool wasCritical)
        {
            damage = Mathf.RoundToInt(damage);

            damageUI.NewDamage(damage, transform.position, wasCritical , Color.white);
            //ConfigureFloatingDamage(damage, wasCritical);

            currentHealth = Mathf.Max(currentHealth - damage, 0);
            trigger = true;
            enemyHealth.UpdateEnemyHealth();
            if (currentHealth == 0)
            {
                Die();
                baseStats.Death();
                AwardExperience(instigator);
                GetComponent<Drop>().DropItem();
                StartCoroutine(CleanObject());
            }
        }

        // private void ConfigureFloatingDamage(float damage, bool wasCritical)
        // {
        //     floatingDmg = Instantiate(floatingDmgText, Vector3.zero, Quaternion.identity);
        //     floatingDmg.GetComponent<TextMesh>().text = damage.ToString();

        //     if (wasCritical)
        //     {
        //         floatingDmg.GetComponent<TextMesh>().fontSize = 140;
        //         floatingDmg.GetComponent<TextMesh>().text += " crit!";
        //         floatingDmg.GetComponent<TextMesh>().color = Color.yellow;
        //     }

        //     floatingDmg.GetComponent<RectTransform>().position = RandomizePosition();

        //     floatingDmg.transform.SetParent(GameObject.FindGameObjectWithTag("Canvas").transform, false);
        // }

        private Vector3 RandomizePosition()
        {
            float x = UnityEngine.Random.Range(-400f, 400f);
            float y = UnityEngine.Random.Range(-200f, 200f);

            return new Vector3(-250, 0, 0);
        }
        private void ConfigureFloatingExperienceGain(float experience)
        {
            floatingDmg = Instantiate(floatingDmgText, Vector3.zero, Quaternion.identity);
            floatingDmg.GetComponent<TextMesh>().fontSize = 100;
            floatingDmg.GetComponent<TextMesh>().offsetZ = 100;
            floatingDmg.GetComponent<TextMesh>().text = "+" + experience.ToString() + " exp ";
            floatingDmg.GetComponent<TextMesh>().color = Color.magenta;
            floatingDmg.GetComponent<RectTransform>().position = new Vector3(250, -250, 0);
            floatingDmg.transform.SetParent(GameObject.FindGameObjectWithTag("Canvas").transform, false);
        }

        private void Die()
        {
            cc.enabled = false;
            isDead = true;
            GetComponent<Animator>().SetTrigger("die");
            GetComponent<ActionScheduler>().CancelCurrentAction();
            enemyHealth.UpdateEnemyHealth();
        }

        public void AwardExperience(GameObject instigator)
        {
            experience = instigator.GetComponent<Experience>();
            if (experience == null) return;
            var experienceGained = baseStats.GetStat(Stat.ExperienceReward);
            experience.GainExperience(experienceGained);
            ConfigureFloatingExperienceGain(experienceGained);
        }

        public float GetHealthPct()
        {
            return 100 * GetFraction();
        }

        public float GetFraction()
        {
            return currentHealth / maxHealth.value;
        }

        public void AddHealth(float hpToAdd)
        {
            currentHealth += hpToAdd;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth.value); // Prevents HP from going over 100%
        }

        public object CaptureState()
        {
            return currentHealth;
        }

        public void RestoreState(object state)
        {
            currentHealth = (float)state;

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        public bool gotDamaged()
        {
            return trigger;
        }

        IEnumerator RegenerateHealth()
        {
            while (!isDead)
            {
                AddHealth(baseStats.GetStat(Stat.HealthRegenaration));
                yield return new WaitForSeconds(2f);
            }
        }

        IEnumerator CleanObject()
        {
            yield return new WaitForSeconds(60f);
            Destroy(gameObject);
        }
    }
}
