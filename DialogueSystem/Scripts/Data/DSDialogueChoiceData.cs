namespace DS.Data
{
    using DS.Data.Events;
    using ScriptableObjects;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;

    [System.Serializable]
    public class DSDialogueChoiceData
    {
        [field: SerializeField] public string Text { get; set; }
        [field: SerializeField] public DSDialogueSO NextDialogue { get; set; }

        //event checks
        [Header("Choice Events")]
        [SerializeField] public UnityEvent OnChoiceSelected;
        [SerializeField] public List<DSCondition> ChoiceConditions;

        [Header("Choice Requirements")]
        [SerializeField] public bool HasRequirements;
        [SerializeField] public string RequiredItemId;
        [SerializeField] public int RequiredItemQuantity = 1;
        [SerializeField] public string RequiredQuestId;
        [SerializeField] public string BlockedText = "[Requires Item]";

        /// <summary>
        /// Check if this choice is available to the player
        /// </summary>
        public bool IsChoiceAvailable()
        {
            if (!HasRequirements) return true;

            // Implement your condition checks here
            // Example:
            // if (!string.IsNullOrEmpty(RequiredItemId) && !InventoryManager.HasItem(RequiredItemId, RequiredItemQuantity))
            //     return false;
            // if (!string.IsNullOrEmpty(RequiredQuestId) && !QuestManager.IsQuestCompleted(RequiredQuestId))
            //     return false;

            return true;
        }

        /// <summary>
        /// Get the display text for this choice (may show blocked text if requirements not met)
        /// </summary>
        public string GetDisplayText()
        {
            return IsChoiceAvailable() ? Text : BlockedText;
        }
    }
}