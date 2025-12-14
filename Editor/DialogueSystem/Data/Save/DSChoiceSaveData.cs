using System;
using UnityEngine;

namespace DS.Data.Save
{
    /// <summary>
    /// Serialized representation of a dialogue choice for saving/loading.
    /// Stores the choice text and target node reference.
    /// </summary>
    [Serializable]
    public class DSChoiceSaveData
    {
        [field: SerializeField] public string Text { get; set; }
        [field: SerializeField] public string NodeID { get; set; } // Reference to connected node
    }
}