using UnityEngine;
using Tears_Of_Void.Stats;
using Tears_Of_Void.Core;
using System;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using DuloGames.UI;
using Tears_Of_Void.Saving;
using Tears_Of_Void.Combat;
using TMPro;

namespace Tears_Of_Void.Resources
{
    public class Health : MonoBehaviour, IModifierProvider, ISaveable
    {
        [SerializeField] Text healthUIText = default, staminaUIText = default, armorUIText = default;
        bool isDead = false;
        LazyValue<float> maxHealth;
        public DamageUI HealthPopUpUI;
        float currentHealth;
        BaseStats baseStats;
        HealthDisplay healthDisplay;
        [SerializeField] TMP_Text healthSphere;
        public event Action onDamageTaken;
        [SerializeField] float regenerationPercentage = 100;
        [SerializeField] GameObject floatingDmgText = default;
        [SerializeField] Text dodged;
        GameObject floatingDmg;
        bool fading;
        private const string Message = "Dodged";
        public bool IsDead()
        {
            return isDead;
        }

        void Awake()
        {
            maxHealth = new LazyValue<float>(GetMaxHealth);
            baseStats = GetComponent<BaseStats>();
            healthDisplay = GameObject.FindWithTag("HealthBar").GetComponent<HealthDisplay>();
            currentHealth = 5050f; // Starting Health used when game is fresh started.
        }

        void Start()
        {
            maxHealth.ForceInit();
            currentHealth = Mathf.Clamp(currentHealth, currentHealth, maxHealth.value);

            //StartCoroutine(RegenerateHealth());
            healthUIText.text = maxHealth.value.ToString();
            staminaUIText.text = baseStats.GetStat(Stat.Stamina).ToString();
            armorUIText.text = baseStats.GetStat(Stat.Armor).ToString();
            healthDisplay.UpdateUI();
        }

        private void OnEnable()
        {
            GetComponent<BaseStats>().onLevelUp += RegenerateHealthOnLevelUp;
        }

        private void OnDisable()
        {
            GetComponent<BaseStats>().onLevelUp -= RegenerateHealthOnLevelUp;
        }
        public float GetCurrentHealth()
        {
            return currentHealth;
        }
        private float GetMaxHealth()
        {
            return baseStats.GetStat(Stat.Health) + (baseStats.GetStat(Stat.Stamina) * 10);
        }

        public bool isAttackDodged()
        {
            return UnityEngine.Random.Range(1, 100) < baseStats.GetStat(Stat.Dodge) * 100;
        }

        public void TakeDamage(GameObject instigator, float damage, bool wasCritical)
        {
            if (isAttackDodged())
            {
                if (!fading)
                {
                    StartCoroutine(ShowMessage(Message, 1));
                }
                return;
            }

            float dmgReduced = 100 / ((baseStats.GetStat(Stat.Armor) + 100) + ((baseStats.GetLevel()) * 7)); //percentage reduction, percentage also increases per level
            float effectiveDamage = damage * dmgReduced;
            damage = Mathf.Clamp(effectiveDamage, 0, Mathf.Infinity); // Clamp damage to 0 if enemy's damage does is not enough to pass armor
            onDamageTaken();

            damage = Mathf.RoundToInt(damage);
            StartCoroutine(ShowDamageTook(damage.ToString(), 0.4f));

            currentHealth = Mathf.Max(currentHealth - damage, 0);

            if (currentHealth == 0)
            {
                Die();
            }
        }

        //   private void ConfigureFloatingDamage(float damage, bool wasCritical)
        //   {
        //       floatingDmg = Instantiate(floatingDmgText, Vector3.zero, Quaternion.identity);
        //       floatingDmg.GetComponent<TextMesh>().text = damage.ToString();
        //       floatingDmg.GetComponent<TextMesh>().color = Color.red;
        //       floatingDmg.GetComponent<RectTransform>().position = RandomizePosition();
        //       floatingDmg.transform.SetParent(healthSphere.transform, false);
        //       floatingDmg.transform.position = healthSphere.transform.position;
        //   }

        private IEnumerator ShowMessage(string message, float delay)
        {
            fading = false;
            float fadeOutTime = 0.2f;
            dodged.text = message;
            dodged.color = Color.white;
            dodged.enabled = true;
            yield return new WaitForSeconds(delay);
            fading = true;
            Color originalColor = dodged.color;
            for (float t = 0.01f; t < fadeOutTime; t += Time.deltaTime)
            {
                dodged.color = Color.Lerp(originalColor, Color.clear, Mathf.Min(1, t / fadeOutTime));
                yield return null;
            }
            dodged.enabled = false;
            fading = false;
        }

        private IEnumerator ShowDamageTook(string message, float delay)
        {
            float fadeOutTime = 0.2f;
            healthSphere.enabled = true;
            healthSphere.text = message;
            dodged.color = Color.red;
            yield return new WaitForSeconds(delay);
            for (float t = 0.01f; t < fadeOutTime; t += Time.deltaTime)
            {
                yield return null;
            }
            healthSphere.enabled = false;
        }

        private Vector3 RandomizePosition()
        {
            float x = UnityEngine.Random.Range(-400f, 400f);
            float y = UnityEngine.Random.Range(-200f, 200f);

            return new Vector3(-400, -500, 0);
        }

        public float GetHealthPct()
        {
            return GetFraction() * 100;
        }

        public float GetFraction()
        {
            return currentHealth / maxHealth.value;
        }

        public void AddHealth(float HealingValue)
        {
            float CalculateHealth = maxHealth.value - currentHealth;
            HealingValue = Mathf.Clamp(HealingValue, HealingValue, CalculateHealth);
            if (HealingValue == 0) { return; }
            else
            {
                HealthPopUpUI.NewHealing(HealingValue, transform.position, Color.green);
                currentHealth += HealingValue;
                healthDisplay.UpdateUI();
            }
        }

        public void UpdateHealthAndUI()
        {
            maxHealth.value = GetMaxHealth();
            currentHealth = Mathf.Clamp(currentHealth, currentHealth, maxHealth.value);
            healthUIText.text = GetMaxHealth().ToString();
            staminaUIText.text = baseStats.GetStat(Stat.Stamina).ToString();
            armorUIText.text = baseStats.GetStat(Stat.Armor).ToString();
            healthDisplay.UpdateUI();
        }

        private void Die()
        {
            if (isDead) return;
            isDead = true;
            GetComponent<Animator>().SetTrigger("die");
            GetComponent<ActionScheduler>().CancelCurrentAction();
            healthDisplay.UpdateUI();
        }

        public void RegenerateHealthOnLevelUp()
        {
            float regenHealthPoints = GetMaxHealth() * (regenerationPercentage / 100);
            maxHealth.value = Mathf.Max(maxHealth.value, regenHealthPoints);
            currentHealth = maxHealth.value;
            healthUIText.text = GetMaxHealth().ToString();
            healthDisplay.UpdateUI();
        }

        IEnumerator RegenerateHealth()
        {
            while (!isDead)
            {
                AddHealth(baseStats.GetStat(Stat.HealthRegenaration));
                yield return new WaitForSeconds(2f);
            }
        }

        public IEnumerable<float> GetAdditiveModifier(Stat stat)
        {
            float sum = 0;

            if (stat == Stat.Stamina)
            {
                foreach (UIEquipSlot slot in UIEquipSlot.GetSlots())
                {
                    if (slot.GetItemInfo() != null)
                        sum += slot.GetItemInfo().Stamina;
                }
            }

            if (stat == Stat.Armor)
            {
                foreach (UIEquipSlot slot in UIEquipSlot.GetSlots())
                {
                    if (slot.GetItemInfo() != null)
                        sum += slot.GetItemInfo().Armor;
                }
            }

            //print(sum);
            yield return sum;
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
    }
}
