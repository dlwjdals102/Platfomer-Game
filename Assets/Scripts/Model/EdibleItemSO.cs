using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Model
{
    [CreateAssetMenu]
    public class EdibleItemSO : ItemSO, IDestroybleItem, IItemAction
    {
        [SerializeField] 
        private List<ModifierData> modifiersData = new List<ModifierData>();

        public string ActionName => "Consume";

        public AudioClip actionSFX {get; private set;}

        public bool PerforAction(GameObject character)
        {
            foreach (ModifierData data in modifiersData)
            {
                data.statModifier.AffectCharacter(character, data.value);
            }
            return true;
        }
    }

    public interface IDestroybleItem
    {

    }

    public interface IItemAction
    {
        public string ActionName { get; }
        public AudioClip actionSFX { get; }
        bool PerforAction(GameObject character);
    }

    [Serializable]
    public class ModifierData
    {
        public CharacterStatModifierSO statModifier;
        public float value;
    }
}