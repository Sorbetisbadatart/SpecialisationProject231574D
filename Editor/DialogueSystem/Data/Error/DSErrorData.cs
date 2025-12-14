using UnityEngine;

namespace DS.Data.Error
{
    /// <summary>
    /// highlights errors in graph elements.
    /// Generates red color for error types.
    /// </summary>
    public class DSErrorData
    {
        public Color Color { get; set; }
        private Color ErrorColour = Color.red;

        public DSErrorData()
        {
            SetErrorColour();
        }

        private void GenerateRandomColor()
        {
            // Avoid dark colors for visibility
            Color = new Color32(
                (byte)Random.Range(200, 256),
                (byte)1,
                (byte)1,
                255
            );
        }

        private void SetErrorColour()
        {
            Color = ErrorColour;
        }
    }
}