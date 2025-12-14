using System.Collections.Generic;

namespace DS.Data.Error
{
    using Elements;

    /// <summary>
    /// Error for group naming conflicts.
    /// Tracks all groups that share the same name.
    /// </summary>
    public class DSGroupErrorData
    {
        public DSErrorData ErrorData { get; set; }
        public List<DSGroup> Groups { get; set; }

        public DSGroupErrorData()
        {
            ErrorData = new DSErrorData();
            Groups = new List<DSGroup>();
        }
    }
}