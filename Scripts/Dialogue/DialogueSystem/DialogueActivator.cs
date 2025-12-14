using UnityEngine;

/// <summary>
/// Allows dialogue interaction between player and NPC 
/// </summary>
public class DialogueActivator : MonoBehaviour, Iinteractable
{
    [SerializeField] private DialogueObject dialogueObject;
    [SerializeField] private GameObject InteractablePrompt;

    //Checks for players in range
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && other.TryGetComponent(out Player player))
        {
            player.Interactable = this;
            InteractablePrompt.SetActive(true);
        }
    }
    //out of range for interacting
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && other.TryGetComponent(out Player player))
        {
            if (player.Interactable is DialogueActivator dialogueActivator && dialogueActivator == this)
            {
                player.Interactable = null;
                InteractablePrompt.SetActive(false);
            }
        }
    }

    public void Interact(Player player)
    {
        foreach (DialogueResponseEvents responseEvents in GetComponents<DialogueResponseEvents>()) 
        {
            player.DialogueUI.AddResponseEvents(responseEvents.Events);
            break;
        }

        player.DialogueUI.ShowDialogue(dialogueObject);       
    }

    public void UpdateDialogueObject(DialogueObject dialogueObject)
    {
        this.dialogueObject = dialogueObject;
    }
}
