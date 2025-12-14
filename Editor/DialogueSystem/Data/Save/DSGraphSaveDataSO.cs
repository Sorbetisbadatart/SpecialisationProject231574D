using System.Collections.Generic;
using UnityEngine;

namespace DS.Data.Save
{
    /// <summary>
    /// Container for saving the complete graph.
    /// Stores all nodes, groups, and relationships for persistence.
    /// </summary>
    public class DSGraphSaveDataSO : ScriptableObject
    {
        [field: SerializeField] public string FileName { get; set; } 
        [field: SerializeField] public List<DSGroupSaveData> Groups { get; set; }
        [field: SerializeField] public List<DSNodeSaveData> Nodes { get; set; }
       

        // Track previous names to cleanup 
        [field: SerializeField] public List<string> OldGroupNames { get; set; }
        [field: SerializeField] public List<string> OldUngroupedNodeNames { get; set; }
        [field: SerializeField] public SerializableDictionary<string, List<string>> OldGroupedNodeNames { get; set; }

        public void Initialize(string fileName)
        {
            FileName = fileName;

            Groups = new List<DSGroupSaveData>();
            Nodes = new List<DSNodeSaveData>();
        }
    }


}