namespace DS.Runtime
{
    using ScriptableObjects;
    using Data;
    using UnityEngine;
    using UnityEngine.Events;
    using System.Collections.Generic;

    /// <summary>
    /// Handles dialogue execution and event triggering at runtime
    /// </summary>
    public class DSDialogueController : MonoBehaviour
    {
        [SerializeField] private DSDialogueContainerSO dialogueContainer;
        [SerializeField] private DSDialogueSO currentDialogue;

        // Events that other systems can listen to
        public UnityEvent<DSDialogueSO> OnDialogueStarted;
        public UnityEvent<DSDialogueSO> OnDialogueEnded;
        public UnityEvent<DSDialogueChoiceData> OnChoiceMade;
        public UnityEvent<string> OnQuestEvent; // "QuestStarted:QuestID", "QuestCompleted:QuestID"
        public UnityEvent<string, int> OnItemEvent; // "ItemRequired:ItemID", "ItemReceived:ItemID"

        private void Start()
        {
            if (currentDialogue != null && currentDialogue.IsStartingDialogue)
            {
                StartDialogue(currentDialogue);
            }
        }

        /// <summary>
        /// Start a dialogue sequence
        /// </summary>
        public void StartDialogue(DSDialogueSO dialogue)
        {
            if (!dialogue.CanStartDialogue())
            {
                Debug.LogWarning($"Cannot start dialogue {dialogue.DialogueName} - conditions not met");
                return;
            }

            currentDialogue = dialogue;

            // Trigger dialogue start events
            dialogue.OnDialogueStarted?.Invoke();
            OnDialogueStarted?.Invoke(dialogue);

            // Display dialogue UI
            DisplayDialogue(dialogue);
        }

        /// <summary>
        /// End the current dialogue
        /// </summary>
        public void EndDialogue()
        {
            if (currentDialogue != null)
            {
                currentDialogue.OnDialogueEnded?.Invoke();
                OnDialogueEnded?.Invoke(currentDialogue);
                currentDialogue = null;
            }
        }

        /// <summary>
        /// Make a choice in the current dialogue
        /// </summary>
        public void MakeChoice(int choiceIndex)
        {
            if (currentDialogue == null || choiceIndex < 0 || choiceIndex >= currentDialogue.Choices.Count)
                return;

            var choice = currentDialogue.Choices[choiceIndex];

            if (!choice.IsChoiceAvailable())
            {
                Debug.Log("Choice requirements not met");
                return;
            }

            // Trigger choice events
            choice.OnChoiceSelected?.Invoke();
            currentDialogue.OnDialogueChoiceSelected?.Invoke();
            OnChoiceMade?.Invoke(choice);

            // Move to next dialogue
            if (choice.NextDialogue != null)
            {
                StartDialogue(choice.NextDialogue);
            }
            else
            {
                EndDialogue();
            }
        }

        /// <summary>
        /// Get available choices for current dialogue
        /// </summary>
        public List<DSDialogueChoiceData> GetAvailableChoices()
        {
            var availableChoices = new List<DSDialogueChoiceData>();

            if (currentDialogue != null)
            {
                foreach (var choice in currentDialogue.Choices)
                {
                    if (choice.IsChoiceAvailable())
                    {
                        availableChoices.Add(choice);
                    }
                }
            }

            return availableChoices;
        }

        private void DisplayDialogue(DSDialogueSO dialogue)
        {
            // This would integrate with your UI system
            Debug.Log($"Dialogue: {dialogue.Text}");

            var choices = GetAvailableChoices();
            for (int i = 0; i < choices.Count; i++)
            {
                Debug.Log($"{i}: {choices[i].GetDisplayText()}");
            }
        }

        // Public methods for external systems to trigger events
        public void TriggerQuestEvent(string questId, bool started = true)
        {
            string eventKey = started ? "QuestStarted" : "QuestCompleted";
            OnQuestEvent?.Invoke($"{eventKey}:{questId}");
        }

        public void TriggerItemEvent(string itemId, int quantity, bool required = true)
        {
            string eventKey = required ? "ItemRequired" : "ItemReceived";
            OnItemEvent?.Invoke($"{eventKey}:{itemId}", quantity);
        }
    }
}