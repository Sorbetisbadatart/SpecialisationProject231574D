using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace DS.Elements
{
    /// <summary>
    /// Visual group element that can contain multiple nodes for organization.
    /// Extends Unity's Group class with custom functionality and error handling.
    /// </summary>
    public class DSGroup : Group
    {
        public string ID { get; set; }
        public string OldTitle { get; set; }// For track rename

        private Color defaultBorderColor;
        private float defaultBorderWidth;

        public DSGroup(string groupTitle, Vector2 position)
        {
            //Guid is Global Unique Identifier
            ID = Guid.NewGuid().ToString();

            title = groupTitle;
            OldTitle = groupTitle;

            SetPosition(new Rect(position, Vector2.zero));

            // Store default styles for error reset
            defaultBorderColor = contentContainer.style.borderBottomColor.value;
            defaultBorderWidth = contentContainer.style.borderBottomWidth.value;
        }

        /// <summary>
        /// Applies error style to when there is naming conflicts.
        /// </summary>
        public void SetErrorStyle(Color color)
        {
            contentContainer.style.borderBottomColor = color;
            contentContainer.style.borderBottomWidth = 2f;
        }

        /// <summary>
        /// Resets to default styling when conflicts are resolved.
        /// </summary>
        public void ResetStyle()
        {
            contentContainer.style.borderBottomColor = defaultBorderColor;
            contentContainer.style.borderBottomWidth = defaultBorderWidth;
        }
    }
}