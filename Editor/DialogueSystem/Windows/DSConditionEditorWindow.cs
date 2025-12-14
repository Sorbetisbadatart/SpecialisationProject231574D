namespace DS.Windows
{
    using Elements;
    using ScriptableObjects;
    using Data.Events;
    using UnityEditor;
    using UnityEngine;

    public class DSConditionEditorWindow : EditorWindow
    {
        private DSNode targetNode;
        private DSDialogueSO targetDialogueSO;
        private SerializedObject serializedDialogueSO;
        private Vector2 scrollPosition;

        public static void Open(DSNode node)
        {
            var window = GetWindow<DSConditionEditorWindow>("Dialogue Conditions");
            window.targetNode = node;
            window.targetDialogueSO = DSEventEditorWindow.FindOrCreateDialogueSOForNode(node); 
            window.serializedDialogueSO = new SerializedObject(window.targetDialogueSO);
        }

        private void OnGUI()
        {
            if (targetDialogueSO == null) return;

            EditorGUILayout.LabelField($"Conditions for: {targetNode.DialogueName}", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            serializedDialogueSO.Update();
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // Draw conditions list
            var conditionsProperty = serializedDialogueSO.FindProperty("StartConditions");
            EditorGUILayout.PropertyField(conditionsProperty, new GUIContent("Start Conditions"), true);

            EditorGUILayout.EndScrollView();
            serializedDialogueSO.ApplyModifiedProperties();

            EditorGUILayout.Space();
            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Apply"))
                {
                    ApplyConditions();
                    Close();
                }
                if (GUILayout.Button("Add Example"))
                {
                    AddExampleCondition();
                }
                if (GUILayout.Button("Cancel"))
                {
                    Close();
                }
            }
            GUILayout.EndHorizontal();
        }

        private void AddExampleCondition()
        {
            targetDialogueSO.StartConditions.Add(new DSCondition
            {
                Type = DSCondition.ConditionType.QuestCompleted,
                ConditionKey = "TutorialComplete",
                RequiredValue = 1
            });
            EditorUtility.SetDirty(targetDialogueSO);
        }

        private void ApplyConditions()
        {
            if (AssetDatabase.Contains(targetDialogueSO))
            {
                EditorUtility.SetDirty(targetDialogueSO);
                AssetDatabase.SaveAssets();
            }
            targetNode.SetDialogueSO(targetDialogueSO);
        }
    }
}