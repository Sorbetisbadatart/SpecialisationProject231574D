//using UnityEngine;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEditor;

//// Base class for all event actions
//public abstract class EventAction : ScriptableObject, ISerializationCallbackReceiver
//{
//    [Header("Base Settings")]
//    [SerializeField, HideInInspector] private string actionGuid;
//    [SerializeField] public float delay = 0f;
//    [SerializeField] public bool waitForCompletion = true;
//    [SerializeField] public bool runParallel = false;

//    // Properties
//    public string ActionGuid => actionGuid;
//    public float Delay => delay;
//    public bool WaitForCompletion => waitForCompletion;
//    public bool RunParallel => runParallel;

//    public abstract string ActionName { get; }
//    public abstract Color DisplayColor { get; }
//    public virtual string Description => GetType().Name;

//    // Execution interface
//    public abstract IEnumerator Execute(GameObject triggerer, MapEvent parentEvent);

//    // Editor interface
//    public virtual void OnDrawInspectorGUI()
//    {
//        EditorGUILayout.LabelField(ActionName, EditorStyles.boldLabel);
//        EditorGUILayout.HelpBox(Description, MessageType.Info);

//        delay = EditorGUILayout.FloatField("Delay (seconds)", delay);
//        waitForCompletion = EditorGUILayout.Toggle("Wait for Completion", waitForCompletion);
//        runParallel = EditorGUILayout.Toggle("Run Parallel", runParallel);
//    }

//    // Validation
//    public virtual bool IsValid(out string errorMessage)
//    {
//        errorMessage = string.Empty;
//        return true;
//    }

//    // Serialization
//    public void OnBeforeSerialize()
//    {
//        if (string.IsNullOrEmpty(actionGuid))
//        {
//            actionGuid = Guid.NewGuid().ToString();
//        }
//    }

//    public void OnAfterDeserialize()
//    {
//        // Nothing needed here
//    }

//    // Factory method with asset creation
//    public static T Create<T>(MapData parentMap = null) where T : EventAction
//    {
//        T action = ScriptableObject.CreateInstance<T>();

//        // Generate unique name
//        action.name = $"{action.ActionName}_{DateTime.Now:yyyyMMdd_HHmmss}";
//        action.actionGuid = Guid.NewGuid().ToString();

//        // If parent map is provided, add as sub-asset
//        if (parentMap != null)
//        {
//            AssetDatabase.AddObjectToAsset(action, parentMap);
//            AssetDatabase.SaveAssets();
//        }

//        return action;
//    }
//}

//// Custom editor for EventAction
//[CustomEditor(typeof(EventAction), true)]
//public class EventActionEditor : Editor
//{
//    private SerializedProperty delayProp;
//    private SerializedProperty waitProp;
//    private SerializedProperty parallelProp;

//    private void OnEnable()
//    {
//        delayProp = serializedObject.FindProperty("delay");
//        waitProp = serializedObject.FindProperty("waitForCompletion");
//        parallelProp = serializedObject.FindProperty("runParallel");
//    }

//    public override void OnInspectorGUI()
//    {
//        serializedObject.Update();

//        // Draw default properties
//        EditorGUILayout.PropertyField(delayProp);
//        EditorGUILayout.PropertyField(waitProp);
//        EditorGUILayout.PropertyField(parallelProp);

//        // Draw custom GUI from the action
//        EventAction action = (EventAction)target;
//        action.OnDrawInspectorGUI();

//        // Validation
//        if (!action.IsValid(out string error))
//        {
//            EditorGUILayout.HelpBox(error, MessageType.Error);
//        }

//        serializedObject.ApplyModifiedProperties();
//    }
//}

//// Example of a concrete action with custom inspector
//[CreateAssetMenu(menuName = "RPGToolkit/Events/Actions/Set Flag")]
//public class SetFlagEventAction : EventAction
//{
//    [SerializeField] private string flagName = "new_flag";
//    [SerializeField] private bool flagValue = true;
//    [SerializeField] private FlagOperation operation = FlagOperation.Set;

//    public enum FlagOperation { Set, Toggle, Increment, Decrement }

//    public override string ActionName => "Set Game Flag";
//    public override Color DisplayColor => Color.green;
//    public override string Description => $"Sets flag '{flagName}' to {flagValue}";

//    public override void OnDrawInspectorGUI()
//    {
//        base.OnDrawInspectorGUI();

//        flagName = EditorGUILayout.TextField("Flag Name", flagName);
//        operation = (FlagOperation)EditorGUILayout.EnumPopup("Operation", operation);

//        if (operation == FlagOperation.Set)
//        {
//            flagValue = EditorGUILayout.Toggle("Value", flagValue);
//        }
//    }

//    public override IEnumerator Execute(GameObject triggerer, MapEvent parentEvent)
//    {
//        // Wait for delay
//        if (Delay > 0)
//        {
//            yield return new WaitForSeconds(Delay);
//        }

//        // Execute flag operation
//        switch (operation)
//        {
//            case FlagOperation.Set:
//                GameFlagManager.SetFlag(flagName, flagValue);
//                break;
//            case FlagOperation.Toggle:
//                GameFlagManager.ToggleFlag(flagName);
//                break;
//            case FlagOperation.Increment:
//                GameFlagManager.IncrementFlag(flagName);
//                break;
//            case FlagOperation.Decrement:
//                GameFlagManager.DecrementFlag(flagName);
//                break;
//        }

//        Debug.Log($"Set flag '{flagName}' using operation: {operation}");
//    }

//    public override IEnumerator Execute(GameObject triggerer)
//    {
//        // Wait for delay
//        if (Delay > 0)
//        {
//            yield return new WaitForSeconds(Delay);
//        }

//        // Execute flag operation
//        switch (operation)
//        {
//            case FlagOperation.Set:
//                GameFlagManager.SetFlag(flagName, flagValue);
//                break;
//            case FlagOperation.Toggle:
//                GameFlagManager.ToggleFlag(flagName);
//                break;
//            case FlagOperation.Increment:
//                GameFlagManager.IncrementFlag(flagName);
//                break;
//            case FlagOperation.Decrement:
//                GameFlagManager.DecrementFlag(flagName);
//                break;
//        }

//        Debug.Log($"Set flag '{flagName}' using operation: {operation}");
//    }

//    public override bool IsValid(out string errorMessage)
//    {
//        if (string.IsNullOrEmpty(flagName))
//        {
//            errorMessage = "Flag name cannot be empty";
//            return false;
//        }

//        errorMessage = string.Empty;
//        return true;
//    }
//}