using System.Collections;
using System.Collections.Generic;
using DuloGames.UI;
using Tears_Of_Void.Combat;
using Tears_Of_Void.Items;
using Tears_Of_Void.Stats;
using UnityEngine;

public class ApplyBurnWeaponEffect : Weapons.WeaponAttackEffect
{
    public float PercentageChance;
    public int Damage;
    public float Time;
    
    public override void OnAttack(AIHealth target, Fighter user)
    {
        if (Random.value < (PercentageChance / 100.0f))
        {
            ElementalEffect effect = new ElementalEffect(Time, BaseStats.DamageType.Fire, Damage, 1.0f);

            user.baseStats.AddElementalEffect(effect);
        }
    }
}
