using System.Collections.Generic;
using UnityEngine;

namespace DS.ScriptableObjects
{
    using Data;
    using DS.Data.Events;
    using Enumerations;
    using UnityEngine.Events;

    /// <summary>
    /// a single dialogue object in the conversation.
    /// Contains the dialogue text, responses and other data.
    /// </summary>
    public class DSDialogueSO : ScriptableObject
    {
        [field: SerializeField] public string DialogueName { get; set; }
        [field: SerializeField] [TextArea()] public string Text { get; set; }
        [field: SerializeField] public List<DSDialogueChoiceData> Choices { get; set; }
        [field: SerializeField] public DSDialogueType DialogueType { get; set; }
        [field: SerializeField] public bool IsStartingDialogue { get; set; }
        [field: SerializeField] public List<DSCondition> StartConditions { get; set; }

        [Header("Dialogue Events")]
        [SerializeField] public UnityEvent OnDialogueStarted;
        [SerializeField] public UnityEvent OnDialogueEnded;
        [SerializeField] public UnityEvent OnDialogueChoiceSelected;

        [Header("Quest & Item Events")]
        [SerializeField] public UnityEvent OnQuestStarted;
        [SerializeField] public UnityEvent OnQuestCompleted;
        [SerializeField] public UnityEvent OnItemRequired;
        [SerializeField] public UnityEvent OnItemReceived;

        public void Initialize(string dialogueName, string text, List<DSDialogueChoiceData> choices, DSDialogueType dialogueType, bool isStartingDialogue)
        {
            DialogueName = dialogueName;
            Text = text;
            Choices = choices;
            DialogueType = dialogueType;
            IsStartingDialogue = isStartingDialogue;

            // Initialize events
            StartConditions = new List<DSCondition>();
            OnDialogueStarted = new UnityEvent();
            OnDialogueEnded = new UnityEvent();
            OnDialogueChoiceSelected = new UnityEvent();
            OnQuestStarted = new UnityEvent();
            OnQuestCompleted = new UnityEvent();
            OnItemRequired = new UnityEvent();
            OnItemReceived = new UnityEvent();
        }

        /// <summary>
        /// Check if all start conditions are met
        /// </summary>
        public bool CanStartDialogue()
        {
            if (StartConditions == null || StartConditions.Count == 0)
                return true;

            // This would be implemented in your game manager
            // For now, we'll assume all conditions are met
            return CheckConditions();
        }

        private bool CheckConditions()
        {
            // This would integrate with your quest system, inventory, etc.
            // Example implementation:
            foreach (var condition in StartConditions)
            {
                if (!EvaluateCondition(condition))
                    return false;
            }
            return true;
        }

        private bool EvaluateCondition(DSCondition condition)
        {
            // Implement based on your game systems
            switch (condition.Type)
            {
                case DSCondition.ConditionType.QuestActive:
                    // return QuestManager.IsQuestActive(condition.ConditionKey);
                    return true;
                case DSCondition.ConditionType.QuestCompleted:
                    // return QuestManager.IsQuestCompleted(condition.ConditionKey);
                    return true;
                case DSCondition.ConditionType.ItemInInventory:
                    // return InventoryManager.HasItem(condition.ConditionKey, condition.RequiredValue);
                    return true;
                default:
                    return true;
            }
        }

    }
}