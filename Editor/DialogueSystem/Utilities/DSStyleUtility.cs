using UnityEditor;
using UnityEngine.UIElements;

namespace DS.Utilities
{
    /// <summary>
    /// Utility methods for managing UI element styles and style sheets.
    /// </summary>
    public static class DSStyleUtility
    {
        /// <summary>
        /// Adds CSS classes to a VisualElement.
        /// </summary>
        public static VisualElement AddClasses(this VisualElement element, params string[] classNames)
        {
            foreach (string className in classNames)
            {
                element.AddToClassList(className);
            }
            return element;
        }

        /// <summary>
        /// Adds style sheets to a VisualElement.
        /// </summary>
        public static VisualElement AddStyleSheets(this VisualElement element, params string[] styleSheetNames)
        {
            foreach (string styleSheetName in styleSheetNames)
            {
                StyleSheet styleSheet = (StyleSheet)EditorGUIUtility.Load(styleSheetName);
                element.styleSheets.Add(styleSheet);
            }
            return element;
        }
    }
}