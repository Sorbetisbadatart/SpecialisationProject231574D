using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DS.Elements
{
    using Data.Save;
    using DS.Data.Events;
    using DS.ScriptableObjects;
    using Enumerations;
    using UnityEngine.Events;
    using Utilities;
    using Windows;

    /// <summary>
    /// Base class for all dialogue nodes in graph view.
    /// Handles basic functionality like initialization, drawing, and connection/port management.
    /// </summary>
    public class DSNode : Node
    {
        public string ID { get; set; }
        public string DialogueName { get; set; }
        public List<DSChoiceSaveData> Choices { get; set; }
        public string Text { get; set; }
        public DSDialogueType DialogueType { get; set; }
        public DSGroup Group { get; set; }

        public DSGraphView graphView;
        private Color defaultBackgroundColor;

        // REFERENCE TO THE ACTUAL DIALOGUE SO (when available)
        public DSDialogueSO DialogueSO { get; set; }

        // If you can't store the SO reference, store event summaries for display
        public List<string> EventSummaries { get; set; }

        private VisualElement eventListContainer; // Add this reference




        /// <summary>
        /// Initializes the node with basic properties and sets up the visual container.
        /// </summary>
        public virtual void Initialize(string nodeName, DSGraphView dsGraphView, Vector2 position)
        {
            ID = Guid.NewGuid().ToString();
            DialogueName = nodeName;
            Choices = new List<DSChoiceSaveData>();
            Text = "Dialogue text.";
            EventSummaries = new List<string>();

            SetPosition(new Rect(position, Vector2.zero));
            graphView = dsGraphView;
            defaultBackgroundColor = new Color(29f / 255f, 29f / 255f, 30f / 255f);

            mainContainer.AddToClassList("ds-node__main-container");
            extensionContainer.AddToClassList("ds-node__extension-container");
        }

        /// <summary>
        /// Extends the right-click context menu with custom options.
        /// </summary>
        public override void BuildContextualMenu(ContextualMenuPopulateEvent CMEvent)
        {
            CMEvent.menu.AppendAction("Disconnect Input Ports", actionEvent => DisconnectInputPorts());
            CMEvent.menu.AppendAction("Disconnect Output Ports", actionEvent => DisconnectOutputPorts());
            base.BuildContextualMenu(CMEvent);
        }




        /// <summary>
        /// Draws the node's visual elements including title, input port, and text area.
        /// </summary>
        public virtual void Draw()
        {

            /* TITLE CONTAINER - Editable node name */
            TextField dialogueNameTextField = DSElementUtility.CreateTextField(DialogueName, null, callback =>
            {
                TextField target = (TextField)callback.target;
                target.value = callback.newValue.RemoveWhitespaces().RemoveSpecialCharacters();

                // Handle name validation and error tracking
                if (string.IsNullOrEmpty(target.value))
                {
                    if (!string.IsNullOrEmpty(DialogueName))
                    {
                        ++graphView.NameErrorsAmount;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(DialogueName))
                    {
                        --graphView.NameErrorsAmount;
                    }
                }

                // Update node grouping based on name change
                if (Group == null)
                {
                    graphView.RemoveUngroupedNode(this);
                    DialogueName = target.value;
                    graphView.AddUngroupedNode(this);
                    return;
                }

                DSGroup currentGroup = Group;
                graphView.RemoveGroupedNode(this, Group);
                DialogueName = target.value;
                graphView.AddGroupedNode(this, currentGroup);


            });

            dialogueNameTextField.AddClasses(
                "ds-node__text-field",
                "ds-node__text-field__hidden",
                "ds-node__filename-text-field"
            );
            titleContainer.Insert(0, dialogueNameTextField);

            /* INPUT CONTAINER - Connection port for incoming dialogue */
            Port inputPort = this.CreatePort("Dialogue Connection", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi);
            inputContainer.Add(inputPort);

            /* EXTENSION CONTAINER - Collapsible dialogue text area */
            VisualElement customDataContainer = new VisualElement();
            customDataContainer.AddToClassList("ds-node__custom-data-container");

            Foldout textFoldout = DSElementUtility.CreateFoldout("Dialogue Text");
            TextField textTextField = DSElementUtility.CreateTextArea(Text, null, callback => Text = callback.newValue);
            textTextField.AddClasses("ds-node__text-field", "ds-node__quote-text-field");

            textFoldout.Add(textTextField);
            customDataContainer.Add(textFoldout);
            extensionContainer.Add(customDataContainer);
            AddEventUI();
        }


        private void AddEventUI()
        {
            /* EVENT CONTAINER */
            VisualElement eventContainer = new VisualElement();
            eventContainer.AddToClassList("ds-node__event-container");

            Foldout eventFoldout = DSElementUtility.CreateFoldout("Dialogue Events", true);

            // Dialogue Start Event
            Button addStartEventButton = DSElementUtility.CreateButton("Add Start Event", () =>
            {
                // This would open a custom event editor window
                Debug.Log("Add start event to node: " + DialogueName);
            });
            addStartEventButton.AddToClassList("ds-node__button--event");

            // Dialogue End Event  
            Button addEndEventButton = DSElementUtility.CreateButton("Add End Event", () =>
            {
                Debug.Log("Add end event to node: " + DialogueName);
            });
            addEndEventButton.AddToClassList("ds-node__button--event");

            // Conditions
            Button addConditionButton = DSElementUtility.CreateButton("Add Start Condition", () =>
            {
                Debug.Log("Add start condition to node: " + DialogueName);
            });
            addConditionButton.AddToClassList("ds-node__button--condition");

            eventFoldout.Add(addStartEventButton);
            eventFoldout.Add(addEndEventButton);
            eventFoldout.Add(addConditionButton);
            eventContainer.Add(eventFoldout);

            extensionContainer.Add(eventContainer);
        }

        /// <summary>
        /// Disconnects all ports from this node.
        /// </summary>
        public void DisconnectAllPorts()
        {
            DisconnectInputPorts();
            DisconnectOutputPorts();
        }

        private void DisconnectInputPorts()
        {
            DisconnectPorts(inputContainer);
        }

        private void DisconnectOutputPorts()
        {
            DisconnectPorts(outputContainer);
        }

        /// <summary>
        /// Disconnects all connections from ports in the specific container.
        /// </summary>
        private void DisconnectPorts(VisualElement container)
        {
            foreach (Port port in container.Children())
            {
                if (!port.connected) continue;
                graphView.DeleteElements(port.connections);
            }
        }

        /// <summary>
        /// Checks if this node is a starting node (no input connections).
        /// </summary>
        public bool IsStartingNode()
        {
            Port inputPort = (Port)inputContainer.Children().First();
            return !inputPort.connected;
        }

        /// <summary>
        /// Applies error style to show naming conflicts.
        /// </summary>
        public void SetErrorStyle(Color color)
        {
            mainContainer.style.backgroundColor = color;
        }

        /// <summary>
        /// Resets to default style when conflicts are resolved.
        /// </summary>
        public void ResetStyle()
        {
            mainContainer.style.backgroundColor = defaultBackgroundColor;
        }

      

        /// <summary>
        /// Refresh the event display based on current dialogue SO
        /// </summary>
        public void RefreshEventDisplay()
        {
            if (eventListContainer == null) return;

            // Clear existing event display
            eventListContainer.Clear();

            if (DialogueSO != null)
            {
                // Show event summaries from the actual DialogueSO
                AddEventSummaryIfHasCalls("On Start", DialogueSO.OnDialogueStarted);
                AddEventSummaryIfHasCalls("On End", DialogueSO.OnDialogueEnded);
                AddEventSummaryIfHasCalls("On Choice", DialogueSO.OnDialogueChoiceSelected);

                // Show conditions
                if (DialogueSO.StartConditions != null && DialogueSO.StartConditions.Count > 0)
                {
                    AddConditionSummary(DialogueSO.StartConditions);
                }
            }
            else
            {
                // Show placeholder if no SO assigned
                var placeholder = new Label("No events configured");
                placeholder.AddToClassList("ds-node__event-placeholder");
                eventListContainer.Add(placeholder);
            }
        }


        private void AddEventSummaryIfHasCalls(string eventName, UnityEngine.Events.UnityEvent unityEvent)
        {
#if UNITY_EDITOR
            try
            {
                // Use reflection to get persistent call count in editor
                var field = unityEvent.GetType().GetField("m_PersistentCalls",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    var persistentCalls = field.GetValue(unityEvent);
                    var callsProperty = persistentCalls.GetType().GetProperty("m_Calls");
                    if (callsProperty != null)
                    {
                        var calls = callsProperty.GetValue(persistentCalls) as System.Collections.IList;
                        if (calls != null && calls.Count > 0)
                        {
                            AddEventSummaryItem(eventName, $"{calls.Count} action(s)");
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                // Fallback: just show the event name if reflection fails
                AddEventSummaryItem(eventName, "? actions");
                Debug.LogWarning($"Failed to get event call count: {e.Message}");
            }
#endif
        }

        private void AddConditionSummary(System.Collections.Generic.List<DSCondition> conditions)
        {
            var conditionItem = new VisualElement();
            conditionItem.AddToClassList("ds-node__event-item");

            var label = new Label($"Conditions: {conditions.Count} condition(s)");
            var editButton = DSElementUtility.CreateButton("Edit", () => EditConditions());
            editButton.AddToClassList("ds-node__button--small");

            conditionItem.Add(label);
            conditionItem.Add(editButton);

            eventListContainer.Add(conditionItem);
        }

        private void AddEventSummaryItem(string eventName, string summary)
        {
            var eventItem = new VisualElement();
            eventItem.AddToClassList("ds-node__event-item");

            var label = new Label($"{eventName}: {summary}");
            var editButton = DSElementUtility.CreateButton("Edit", () => EditEvent(eventName));
            editButton.AddToClassList("ds-node__button--small");

            eventItem.Add(label);
            eventItem.Add(editButton);

            eventListContainer.Add(eventItem);
        }

        private void EditEvent(string eventName)
        {
            // Open event editor for the specific event type
            DSEventEditorWindow.Open(this, eventName);
        }

        private void EditConditions()
        {
            // Open condition editor
            DSConditionEditorWindow.Open(this);
        }

        private void AddStartEvent()
        {
            DSEventEditorWindow.Open(this, "OnDialogueStarted");
        }

        private void AddEndEvent()
        {
            DSEventEditorWindow.Open(this, "OnDialogueEnded");
        }

        private void AddCondition()
        {
            DSConditionEditorWindow.Open(this);
        }

        /// <summary>
        /// Sets the dialogue SO reference for this node
        /// </summary>
        public void SetDialogueSO(DSDialogueSO dialogueSO)
        {
            DialogueSO = dialogueSO;
            RefreshEventDisplay();
        }

    }
}