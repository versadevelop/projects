using System.Collections;
using System.Collections.Generic;
using Tears_Of_Void.Stats;
using UnityEngine;

namespace Tears_Of_Void.Combat
{
    [CreateAssetMenu(fileName = "DropTable", menuName = "Drops/New Drop Table", order = 0)]
    public class DropTable : ScriptableObject
    {
        [SerializeField] DropCharacterClass[] characterClasses = null;

        Dictionary<CharacterClass, Dictionary<int, float>> lookupTable = null;

        /// <summary>
        /// Returns this mob's dropped item IDs along with their drop chance on a dictionary
        /// </summary>
        public Dictionary<int, float> GetDroppedItems(CharacterClass characterClass)
        {
            BuildLookup();

            Dictionary<int, float> items = lookupTable[characterClass];
            return items;
        }

        private void BuildLookup()
        {
            if (lookupTable != null) return;

            lookupTable = new Dictionary<CharacterClass, Dictionary<int, float>>();

            foreach (DropCharacterClass dropClass in characterClasses)
            {
                var dropLookupTable = new Dictionary<int, float>();

                foreach (DropStat dropStat in dropClass.drops)
                {
                    dropLookupTable[dropStat.itemID] = dropStat.dropChance;
                }

                lookupTable[dropClass.characterClass] = dropLookupTable;
            }
        }

        [System.Serializable]
        class DropCharacterClass
        {
            public CharacterClass characterClass = default;
            public DropStat[] drops = default;
        }

        [System.Serializable]
        class DropStat
        {
            public int itemID = default;
            [Range(0,1)] public float dropChance = default;
        }
    }
}
