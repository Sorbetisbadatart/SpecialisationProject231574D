using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace DS.Utilities
{
    using Elements;

    /// <summary>
    /// Utility class for creating common UI elements used throughout the dialogue system.
    /// </summary>
    public static class DSElementUtility
    {
        /// <summary>
        /// Creates a button with optional click handler.
        /// </summary>
        public static Button CreateButton(string text, Action onClick = null)
        {
            Button button = new Button(onClick)
            {
                text = text
            };
            return button;
        }

        /// <summary>
        /// Creates a foldout (collapsible section) with specified title and initial state.
        /// </summary>
        public static Foldout CreateFoldout(string title, bool collapsed = false)
        {
            Foldout foldout = new Foldout()
            {
                text = title,
                value = !collapsed
            };
            return foldout;
        }

        /// <summary>
        /// Creates a port on a node for making connections.
        /// </summary>
        public static Port CreatePort(this DSNode node, string portName = "", Orientation orientation = Orientation.Horizontal, Direction direction = Direction.Output, Port.Capacity capacity = Port.Capacity.Single)
        {
            Port port = node.InstantiatePort(orientation, direction, capacity, typeof(bool));
            port.portName = portName;
            return port;
        }

        /// <summary>
        /// Creates a single-line text field with optional initial value and change callback.
        /// </summary>
        public static TextField CreateTextField(string value = null, string label = null, EventCallback<ChangeEvent<string>> onValueChanged = null)
        {
            TextField textField = new TextField()
            {
                value = value,
                label = label
            };

            if (onValueChanged != null)
            {
                textField.RegisterValueChangedCallback(onValueChanged);
            }
            return textField;
        }

        /// <summary>
        /// Creates a multi-line text area for longer text input.
        /// </summary>
        public static TextField CreateTextArea(string value = null, string label = null, EventCallback<ChangeEvent<string>> onValueChanged = null)
        {
            TextField textArea = CreateTextField(value, label, onValueChanged);
            textArea.multiline = true;
            return textArea;
        }
    }
}