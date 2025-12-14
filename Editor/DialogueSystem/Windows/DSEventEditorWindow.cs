namespace DS.Windows
{
    using DS.Data;
    using Elements;
    using ScriptableObjects;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Events;

    public class DSEventEditorWindow : EditorWindow
    {
        private DSNode targetNode;
        private DSDialogueSO targetDialogueSO;
        private string eventType;
        private SerializedObject serializedDialogueSO;
        private Vector2 scrollPosition;

        public static void Open(DSNode node, string eventType)
        {
            var window = GetWindow<DSEventEditorWindow>("Dialogue Event Editor");
            window.targetNode = node;
            window.eventType = eventType;
            window.titleContent = new GUIContent($"Event: {eventType}");

            // Get or find the actual DSDialogueSO for this node
            window.targetDialogueSO = FindOrCreateDialogueSOForNode(node);
            window.serializedDialogueSO = new SerializedObject(window.targetDialogueSO);

            window.minSize = new Vector2(500, 400);
        }

        public static DSDialogueSO FindOrCreateDialogueSOForNode(DSNode node)
        {
            // If node already has a DialogueSO reference, use it
            if (node.DialogueSO != null)
            {
                return node.DialogueSO;
            }

            // Otherwise, try to find it in the project
            // This would need to be implemented based on your project structure
            string expectedPath = $"Assets/DialogueSystem/Dialogues/Global/Dialogues/{node.DialogueName}.asset";
            DSDialogueSO dialogueSO = AssetDatabase.LoadAssetAtPath<DSDialogueSO>(expectedPath);

            if (dialogueSO == null)
            {
                // Create a temporary one for editing (won't be saved unless applied)
                dialogueSO = ScriptableObject.CreateInstance<DSDialogueSO>();

                // Convert node choices to dialogue choices
                List<DSDialogueChoiceData> dialogueChoices = new List<DSDialogueChoiceData>();
                foreach (var nodeChoice in node.Choices)
                {
                    dialogueChoices.Add(new DSDialogueChoiceData
                    {
                        Text = nodeChoice.Text,
                        NextDialogue = null
                    });
                }

                dialogueSO.Initialize(
                    node.DialogueName,
                    node.Text,
                    dialogueChoices,
                    node.DialogueType,
                    node.IsStartingNode()
                );
            }

            return dialogueSO;
        }

        private void OnGUI()
        {
            if (targetDialogueSO == null)
            {
                EditorGUILayout.HelpBox("No dialogue SO found for this node.", MessageType.Error);
                return;
            }

            EditorGUILayout.LabelField($"Editing: {targetNode.DialogueName}", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Event: {eventType}", EditorStyles.miniLabel);
            EditorGUILayout.Space();

            serializedDialogueSO.Update();
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // Draw the appropriate UnityEvent based on eventType
            SerializedProperty eventProperty = GetEventProperty();
            if (eventProperty != null)
            {
                EditorGUILayout.PropertyField(eventProperty, new GUIContent(eventType), true);

                // Show current event call count
                int callCount = eventProperty.FindPropertyRelative("m_PersistentCalls.m_Calls").arraySize;
                EditorGUILayout.LabelField($"Event has {callCount} listener(s)", EditorStyles.miniLabel);
            }
            else
            {
                EditorGUILayout.HelpBox($"Event type '{eventType}' not found.", MessageType.Error);
            }

            EditorGUILayout.Space();

            // Draw conditions if this is for start events
            if (eventType == "OnDialogueStarted")
            {
                EditorGUILayout.LabelField("Start Conditions", EditorStyles.boldLabel);
                SerializedProperty conditionsProperty = serializedDialogueSO.FindProperty("StartConditions");
                EditorGUILayout.PropertyField(conditionsProperty, true);
            }

            EditorGUILayout.EndScrollView();
            serializedDialogueSO.ApplyModifiedProperties();

            EditorGUILayout.Space();
            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Apply to Node"))
                {
                    ApplyEventsToNode();
                    Close();
                }
                if (GUILayout.Button("Add Example"))
                {
                    AddExampleEvent();
                }
                if (GUILayout.Button("Cancel"))
                {
                    Close();
                }
            }
            GUILayout.EndHorizontal();
        }

        private SerializedProperty GetEventProperty()
        {
            return eventType switch
            {
                "OnDialogueStarted" => serializedDialogueSO.FindProperty("OnDialogueStarted"),
                "OnDialogueEnded" => serializedDialogueSO.FindProperty("OnDialogueEnded"),
                "OnDialogueChoiceSelected" => serializedDialogueSO.FindProperty("OnDialogueChoiceSelected"),
                "OnQuestStarted" => serializedDialogueSO.FindProperty("OnQuestStarted"),
                "OnQuestCompleted" => serializedDialogueSO.FindProperty("OnQuestCompleted"),
                "OnItemRequired" => serializedDialogueSO.FindProperty("OnItemRequired"),
                "OnItemReceived" => serializedDialogueSO.FindProperty("OnItemReceived"),
                _ => null
            };
        }

        private void AddExampleEvent()
        {
            // This would add an example event call to help users understand the system
            // Note: This is complex because we're working with SerializedProperty
            // In practice, users would set up events manually in the inspector

            Debug.Log("Add example event - users should manually configure events in the UnityEvent drawer above");

            // Show help box with examples
            EditorGUILayout.HelpBox(
                "To add events:\n" +
                "1. Click '+' in the UnityEvent section above\n" +
                "2. Drag a GameObject from your scene\n" +
                "3. Select a method to call\n" +
                "4. Set any parameters\n" +
                "Example: Drag QuestManager -> StartQuest(string) -> Enter 'MyQuestID'",
                MessageType.Info
            );
        }

        private void ApplyEventsToNode()
        {
            // Save the asset if it's a persistent asset
            if (AssetDatabase.Contains(targetDialogueSO))
            {
                EditorUtility.SetDirty(targetDialogueSO);
                AssetDatabase.SaveAssets();
            }

            // Update node reference
            targetNode.SetDialogueSO(targetDialogueSO);

            Debug.Log($"Applied {eventType} events to node: {targetNode.DialogueName}");

            // Refresh the node display
            targetNode.RefreshEventDisplay();
        }
    }
}