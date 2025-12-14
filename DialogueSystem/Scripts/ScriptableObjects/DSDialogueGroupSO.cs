using UnityEngine;

namespace DS.ScriptableObjects
{
    /// <summary>
    /// container for grouping dialogues together.
    /// a folder in the dialogue graph to organize better.
    /// </summary>
    public class DSDialogueGroupSO : ScriptableObject
    {
        [field: SerializeField] public string GroupName { get; set; }

        public void Initialize(string groupName)
        {
            GroupName = groupName;
        }
    }
}