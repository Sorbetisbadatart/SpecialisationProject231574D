namespace DS.Data.Events
{
    using UnityEngine;
    using UnityEngine.Events;

    [System.Serializable]
    public class DSDialogueEvent
    {
        public string EventName;
        public UnityEvent OnDialogueStart;
        public UnityEvent OnDialogueEnd;
        public UnityEvent OnChoiceSelected;

        [Tooltip("If true, dialogue won't start unless conditions are met")]
        public bool HasStartConditions;
        public UnityEvent CheckStartConditions;
    }

    [System.Serializable]
    public class DSCondition
    {
        public enum ConditionType
        {
            QuestActive,
            QuestCompleted,
            ItemInInventory,
            StatCheck,
            CustomEvent
        }

        public ConditionType Type;
        public string ConditionKey; // Quest ID, Item ID, etc.
        public int RequiredValue;
        public bool InvertCondition; // If true, condition passes when false
    }
}