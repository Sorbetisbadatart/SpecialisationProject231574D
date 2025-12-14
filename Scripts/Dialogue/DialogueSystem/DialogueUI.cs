using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// handles logic for dialogue 
/// </summary>
public class DialogueUI : MonoBehaviour
{
    [SerializeField] private GameObject DialogueBoxUI;
    [SerializeField] private TMP_Text textLabel;
    [SerializeField] private Image portraitImage; // Reference to UI Image for portrait
    [SerializeField] private AudioSource audioSource; // Reference to AudioSource for voice clips
    [SerializeField] private TMP_Text characterName_text; 
    [SerializeField] private GameObject characterNameContainer;

    [Header("Text Positioning")]
    [SerializeField] private RectTransform textLabelRect;
    [SerializeField] private Vector2 textPositionWithPortrait;
    [SerializeField] private Vector2 textPositionWithoutPortrait;
    [SerializeField] private Vector2 textSizeWithPortrait;
    [SerializeField] private Vector2 textSizeWithoutPortrait;

    private DialogueAnimator dialogueAnimator;
    private ResponseHandler responseHandler;
    private TypeWriterEffect typewriterEffect;
    public bool IsOpen { get; private set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        typewriterEffect = GetComponent<TypeWriterEffect>();
        responseHandler = GetComponent<ResponseHandler>();
        dialogueAnimator = GetComponent<DialogueAnimator>();
        audioSource = GetComponent<AudioSource>();

        // If no textLabelRect is assigned, use the textLabel's rect transform
        if (textLabelRect == null && textLabel != null)
            textLabelRect = textLabel.GetComponent<RectTransform>();     
       

        CloseDialogueBox();
    }

    //Core Functionality
    public Coroutine ShowDialogue(DialogueObject dialogueObject)
    {
        IsOpen = true;
        dialogueAnimator.OpenDialogue();
        return(StartCoroutine(StepThroughDialogue(dialogueObject)));
    }

    public void CloseDialogueBox()
    {
        IsOpen = false;
        dialogueAnimator.CloseDialogue();
        ResetDialogueUI();
    }

    public void AddResponseEvents(ResponseEvent[] responseEvents)
    {
        responseHandler.AddResponseEvents(responseEvents);
    }

    private IEnumerator StepThroughDialogue(DialogueObject dialogueObject)
    {
        for (int i = 0; i < dialogueObject.Dialogue.Length; i++)
        {
            string dialogue = dialogueObject.Dialogue[i];

            // Update portrait for this dialogue line and adjust text position
            bool hasPortrait = UpdatePortrait(dialogueObject, i);

            UpdateCharacterName(dialogueObject, i);
            AdjustTextPosition(hasPortrait);

            // Play audio clip for this dialogue line
            PlayAudioClip(dialogueObject, i);

            // Trigger event for this dialogue line if exists
            TriggerEvent(dialogueObject, i);

            yield return RunTypingEffect(dialogue);

            textLabel.text = dialogue;

            if (i == dialogueObject.Dialogue.Length - 1 && dialogueObject.HasResponses) break;

            yield return null;
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        }

        if (dialogueObject.HasResponses)
        {
            responseHandler.ShowResponses(dialogueObject.Responses);
        }
        else
        {
            CloseDialogueBox();
        }
    }

    // Portrait Methods
    private bool UpdatePortrait(DialogueObject dialogueObject, int lineIndex)
    {
        if (portraitImage == null)
        {
            AdjustTextPosition(false);
            return false;
        }

        // Check if there's a portrait for this specific line
        if (dialogueObject.Portrait != null && lineIndex < dialogueObject.Portrait.Length)
        {
            Sprite portraitSprite = dialogueObject.Portrait[lineIndex];
            if (portraitSprite != null)
            {
                portraitImage.sprite = portraitSprite;
                portraitImage.gameObject.SetActive(true);
                return true;
            }
        }

        // If no portrait for this line, hide the portrait image
        portraitImage.gameObject.SetActive(false);
        return false;
    }

    private bool UpdateCharacterName(DialogueObject dialogueObject, int lineIndex)
    {
        // Check if there's a name for this specific line
        if (dialogueObject.Charactername != null && lineIndex < dialogueObject.Charactername.Length)
        {
            string characterName = dialogueObject.Charactername[lineIndex];
            if (characterName != null)
            {
                characterName_text.text = characterName;
                characterNameContainer.SetActive(true);
                return true;
            }
        }

        // If no name for this line, hide the name image
        characterNameContainer.gameObject.SetActive(false);
        return false;
    }

    // Text Positioning Methods
    private void AdjustTextPosition(bool hasPortrait)
    {
        if (textLabelRect == null) return;

        if (hasPortrait)
        {
            // Adjust text position and size when portrait is visible
            
            textLabelRect.sizeDelta = textSizeWithPortrait;
        }
        else
        {
            // Adjust text position and size when no portrait
          
            textLabelRect.sizeDelta = textSizeWithoutPortrait;
        }

        // Force refresh the text layout
        LayoutRebuilder.ForceRebuildLayoutImmediate(textLabelRect);
        textLabel.ForceMeshUpdate();
    }

    // Audio Methods
    private void PlayAudioClip(DialogueObject dialogueObject, int lineIndex)
    {
        if (audioSource == null) return;

        // Stop any currently playing audio
        audioSource.Stop();

        // Check if there's an audio clip for this specific line
        if (dialogueObject.AudioClip != null && lineIndex < dialogueObject.AudioClip.Length)
        {
            AudioClip clip = dialogueObject.AudioClip[lineIndex];
            if (clip != null)
            {
                audioSource.clip = clip;
                audioSource.Play();
            }
        }
    }

    // Event Methods
    private void TriggerEvent(DialogueObject dialogueObject, int lineIndex)
    {
        if (dialogueObject.Events != null && lineIndex < dialogueObject.Events.Length)
        {
            dialogueObject.Events[lineIndex]?.Invoke();
        }
    }

    //Aesthetics
    private IEnumerator RunTypingEffect(string dialogue)
    {
        typewriterEffect.Run(dialogue, textLabel);
        while (typewriterEffect.IsRunning)
        {
            yield return null;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                typewriterEffect.Stop();
                // Also stop audio when skipping
                if (audioSource != null && audioSource.isPlaying)
                {
                    audioSource.Stop();
                }
            }
        }
    }


    // Helper functions
    private void ResetDialogueUI()
    {
        textLabel.text = string.Empty;

        // Reset portrait
        if (portraitImage != null)
        {
            portraitImage.gameObject.SetActive(false);
        }

        // Reset text position to default (no portrait)
        AdjustTextPosition(false);

        // Stop audio
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }

    private void ResetText()
    {
        textLabel.text = string.Empty;
    }

    // Public methods for external control
    public void SetPortraitImage(Image newPortraitImage)
    {
        portraitImage = newPortraitImage;
    }

    public void SetCharacterName(string characterName)
    {
        textLabel.text = characterName;
    }

    public void SetAudioSource(AudioSource newAudioSource)
    {
        audioSource = newAudioSource;
    }

    // Editor helper method to set up positions visually
    [ContextMenu("Set Current as With Portrait")]
    private void SetCurrentAsWithPortrait()
    {
        if (textLabelRect != null)
        {
            textPositionWithPortrait = textLabelRect.anchoredPosition;
            textSizeWithPortrait = textLabelRect.sizeDelta;
        }
    }

    [ContextMenu("Set Current as Without Portrait")]
    private void SetCurrentAsWithoutPortrait()
    {
        if (textLabelRect != null)
        {
            textPositionWithoutPortrait = textLabelRect.anchoredPosition;
            textSizeWithoutPortrait = textLabelRect.sizeDelta;
        }
    }
}