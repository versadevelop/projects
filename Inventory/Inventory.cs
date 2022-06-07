using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DuloGames.UI;
using Tears_Of_Void.Saving;

namespace Tears_Of_Void.Inventory
{
    public class Inventory : MonoBehaviour
    {
        List<UIItemSlot> itemSlots;

        private void Awake()
        {
            InitializeInventory();
        }

        private void Start()
        {
            itemSlots = UIItemSlot.GetSlotsInGroup(UIItemSlot_Group.Inventory);
        }

        private void InitializeInventory()
        {
            foreach (UIItemSlot slot in UIItemSlot.GetSlotsInGroup(UIItemSlot_Group.Inventory))
            {
                slot.tooltipDelay = 0.25f;
            }

            foreach (UIEquipSlot slot in UIEquipSlot.GetSlots())
            {
                slot.tooltipDelay = 0.25f;
            }
        }

        public void AddToInventory(UIItemInfo item)
        {
            foreach (UIItemSlot slot in itemSlots)
            {
                if (!slot.IsAssigned())
                {
                    slot.Assign(item);
                    return;
                }
            }
        }
    }
}

