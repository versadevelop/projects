using System.Collections;
using System.Collections.Generic;
using DuloGames.UI;
using Tears_Of_Void.Combat;
using Tears_Of_Void.Stats;
using UnityEngine;

public class VampiricWeaponEffect : Weapons.WeaponAttackEffect
{
    public int PercentageHealthStolen;

    public override string GetDescription()
    {
        return $"Convert {PercentageHealthStolen}% of physical damage into Health";
    }

    public override void OnPostAttack(AIHealth target, Fighter user)
    {
        int amount = Mathf.FloorToInt(user.finalDamage * (PercentageHealthStolen / 100.0f));
        user.ChangeHealth(amount);
    }
}
