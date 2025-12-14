using System.Collections.Generic;

namespace DS.Data.Error
{
    using Elements;

    /// <summary>
    /// Error for node naming conflicts.
    /// Tracks all nodes that share the same name.
    /// </summary>
    public class DSNodeErrorData
    {
        public DSErrorData ErrorData { get; set; }
        public List<DSNode> Nodes { get; set; }

        public DSNodeErrorData()
        {
            ErrorData = new DSErrorData();
            Nodes = new List<DSNode>();
        }
    }
}