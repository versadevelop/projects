using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DuloGames.UI;
using Tears_Of_Void.Stats;
using UnityEngine.UI;

namespace Tears_Of_Void.Combat
{
    [RequireComponent(typeof(BaseStats))]
    public class Drop : MonoBehaviour
    {
        Dictionary<int, float> itemList;
        bool fading;
        [SerializeField] Text text;
        [SerializeField] private Transform m_Container;
        // UI stuff and item lists
        UIItemDatabase itemDatabase;
        List<UIItemInfo> possibleDrops = new List<UIItemInfo>();
        List<UIItemInfo> droppedItems = new List<UIItemInfo>();


        // Other references
        [SerializeField] DropTable dropTable = default;
        BaseStats baseStats = default;

        // TODO: Add dropped items in an array so that when we right-click the dead enemy, we load them up and see/pick them up

        private void Awake()
        {
            baseStats = GetComponent<BaseStats>();
        }
        private void Start()
        {
            itemDatabase = UIItemDatabase.Instance;
            GetDropTable();
        }

        private void GetDropTable()
        {
            // Get all item ids, then use them to access the itemDatabase and populate droppedItems
            itemList = dropTable.GetDroppedItems(baseStats.characterClass);
            int[] itemIDs = itemList.Keys.ToArray();

            for (int i = 0; i < itemIDs.Length; i++)
            {
                possibleDrops.Add(itemDatabase.GetByID(itemIDs[i]));
            }
        }

        private void RollItems()
        {
            foreach (UIItemInfo itemInfo in possibleDrops)
            {
                float dropChance = 0;
                itemList.TryGetValue(itemInfo.ID, out dropChance);
                float roll = Random.Range(0f, 1f);

                if (roll >= 1 - dropChance)
                {
                    droppedItems.Add(itemInfo);
                    AssignItemToInventory(itemInfo.ID);
                }
            }
        }

        public void AssignItemToInventory(int id)
        {
            UIItemSlot[] slots = this.m_Container.gameObject.GetComponentsInChildren<UIItemSlot>();
            print("Item Dropped: "+UIItemDatabase.Instance.GetByID(id).Name);
            int nextEmptySlot = CheckFirstEmptySlot(slots);
            if(nextEmptySlot == 0){print("Inventory Full"); return;}
            UIItemSlot slot = UIItemSlot.GetSlot(nextEmptySlot, UIItemSlot_Group.Inventory);
            slot.Assign(UIItemDatabase.Instance.GetByID(id));
        }
        
        private int CheckFirstEmptySlot(UIItemSlot[] arraySlots)
        {
           for (int i = 0; i < arraySlots.Length; i++)
            {
                if (!arraySlots[i].IsAssigned())
                {
                    return arraySlots[i].ID;
                }
            }
            return 0;
        }

        public void DropItem()
        {
            RollItems();
        }

        public void RemoveItemFromDrop()
        {
            // Used when the player takes something from the mob's loot, so that the dropped items array will be updated
        }
    }

}
