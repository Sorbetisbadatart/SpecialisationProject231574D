namespace DS.Data.Save
{
    using DS.Data.Events;
    using Enumerations;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;

    [System.Serializable]
    public class DSNodeSaveData
    {
        [field: SerializeField] public string ID { get; set; }
        [field: SerializeField] public string Name { get; set; }
        [field: SerializeField] public string Text { get; set; }
        [field: SerializeField] public List<DSChoiceSaveData> Choices { get; set; }
        [field: SerializeField] public string GroupID { get; set; }
        [field: SerializeField] public DSDialogueType DialogueType { get; set; }
        [field: SerializeField] public Vector2 Position { get; set; }

        // ADD EVENT DATA TO SAVE SYSTEM
        [field: SerializeField] public List<DSConditionSaveData> StartConditions { get; set; }
        [field: SerializeField] public UnityEventSaveData OnDialogueStarted { get; set; }
        [field: SerializeField] public UnityEventSaveData OnDialogueEnded { get; set; }
        [field: SerializeField] public UnityEventSaveData OnDialogueChoiceSelected { get; set; }
    }

    [System.Serializable]
    public class DSConditionSaveData
    {
        public DSCondition.ConditionType Type;
        public string ConditionKey;
        public int RequiredValue;
        public bool InvertCondition;
    }

    [System.Serializable]
    public class UnityEventSaveData
    {
        // This is a simplified representation for saving UnityEvents
        public List<EventPersistentCall> PersistentCalls = new List<EventPersistentCall>();
    }

    [System.Serializable]
    public class EventPersistentCall
    {
        public string TargetObjectName;
        public string MethodName;
        public string StringArgument;
        public int IntArgument;
        public float FloatArgument;
    }
}