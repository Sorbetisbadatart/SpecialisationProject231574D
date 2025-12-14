//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.EventSystems;

//[System.Serializable]
//public class MapEventData
//{
//    public Vector3Int tilePosition;
//    public EventTriggerType triggerType; // OnStep, OnInteract, OnApproach
//    public List<EventAction> actions = new();
//}



//// Concrete event types
//[CreateAssetMenu(menuName = "RPGToolkit/Events/Dialogue")]
//public class DialogueEvent : EventAction
//{
//    public DialogueObject dialogue;

//    public override IEnumerator Execute(GameObject triggerer)
//    {
//        DialogueUI runner = new();
//        yield return runner.ShowDialogue(dialogue);
//    }
//}

//[CreateAssetMenu(menuName = "RPGToolkit/Events/Battle")]
//public class BattleEvent : EventAction
//{
//    public BattleSystem encounter;
//    public bool canEscape = true;

//    public override IEnumerator Execute(GameObject triggerer)
//    {
//        encounter.StartBattle();
//        yield return null;
//    }
//}