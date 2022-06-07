using UnityEngine;

namespace Tears_Of_Void.Combat
{
    [CreateAssetMenu(fileName = "Weapon", menuName = "Weapons/Make New Weapon", order = 0)]
    public class Weapon : ScriptableObject
    {
        [SerializeField] GameObject EquippedPrefab = null;
        //[SerializeField] AnimatorOverrideController animatorOverride = null;
        [SerializeField] float weaponRange = 2f;
        [SerializeField] float weaponDamage = 5f;
        [SerializeField] float attackSpeed = 1f;
        [SerializeField] AnimationClip attackAnimation;
        public AudioClip impact;

        public void Spawn(Transform handTransform)
        {
            if (EquippedPrefab != null)
            {
                Instantiate(EquippedPrefab, handTransform);
            }
            /* if (animatorOverride != null)
             {
                 animator.runtimeAnimatorController = animatorOverride;
             }*/
        }

        public float GetWeaponDamage()
        {
            return weaponDamage;
        }

        public float GetWeaponRange()
        {
            return weaponRange;
        }

        public float GetWeaponSpeed()
        {
            return attackSpeed;
        }
        public AnimationClip GetAnimClip()
        {
            return attackAnimation;
        }

        public AudioClip GetAudioClip()
        {
            return impact;
        }
    }

}