using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class DialogueAnimator : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float openDuration = 0.5f;
    [SerializeField] private float closeDuration = 0.3f;
    [SerializeField] private AnimationCurve openCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve closeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    [Header("Scale Animation")]
    [SerializeField] private Vector3 closedScale = new Vector3(0.8f, 0.8f, 1f);
    [SerializeField] private bool useScaleAnimation = true;

    [Header("References")]
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private Button continueButton;

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Vector3 originalScale;

    // Animation coroutine references
    private Coroutine currentAnimationCoroutine;

    // Public events for other scripts to subscribe to
    public System.Action OnDialogueOpened;
    public System.Action OnDialogueClosed;
    public System.Action OnContinuePressed;

    // State tracking
    public bool IsOpen { get; private set; }
    public bool IsAnimating { get; private set; }

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        originalScale = rectTransform.localScale;

        // Set initial state
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        if (useScaleAnimation)
        {
            rectTransform.localScale = closedScale;
        }

        // Setup button listener
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueButtonPressed);
        }
    }

    #region Public Methods - Called from other scripts

    /// <summary>
    /// Opens the dialogue box with animation
    /// </summary>
    public void OpenDialogue()
    {
        if (IsOpen || IsAnimating) return;

        StopCurrentAnimation();
        currentAnimationCoroutine = StartCoroutine(OpenAnimation());
    }

    /// <summary>
    /// Closes the dialogue box with animation
    /// </summary>
    public void CloseDialogue()
    {
        if (!IsOpen || IsAnimating) return;

        StopCurrentAnimation();
        currentAnimationCoroutine = StartCoroutine(CloseAnimation());
    }

    /// <summary>
    /// Immediately shows the dialogue box without animation
    /// </summary>
    public void ShowImmediate()
    {
        StopCurrentAnimation();

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        rectTransform.localScale = originalScale;

        IsOpen = true;
        IsAnimating = false;

        OnDialogueOpened?.Invoke();
    }

    /// <summary>
    /// Immediately hides the dialogue box without animation
    /// </summary>
    public void HideImmediate()
    {
        StopCurrentAnimation();

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        if (useScaleAnimation)
        {
            rectTransform.localScale = closedScale;
        }

        IsOpen = false;
        IsAnimating = false;

        OnDialogueClosed?.Invoke();
    }

    /// <summary>
    /// Sets the dialogue text and speaker name
    /// </summary>
    /// <param name="speakerName">Name of the speaker</param>
    /// <param name="dialogue">Dialogue text to display</param>
    public void SetDialogue(string speakerName, string dialogue)
    {
        if (speakerNameText != null)
        {
            speakerNameText.text = speakerName;
        }

        if (dialogueText != null)
        {
            dialogueText.text = dialogue;
        }
    }

    /// <summary>
    /// Sets only the dialogue text
    /// </summary>
    /// <param name="dialogue">Dialogue text to display</param>
    public void SetDialogueText(string dialogue)
    {
        if (dialogueText != null)
        {
            dialogueText.text = dialogue;
        }
    }

    /// <summary>
    /// Sets only the speaker name
    /// </summary>
    /// <param name="speakerName">Name of the speaker</param>
    public void SetSpeakerName(string speakerName)
    {
        if (speakerNameText != null)
        {
            speakerNameText.text = speakerName;
        }
    }

    /// <summary>
    /// Toggles the dialogue box (opens if closed, closes if open)
    /// </summary>
    public void ToggleDialogue()
    {
        if (IsOpen)
        {
            CloseDialogue();
        }
        else
        {
            OpenDialogue();
        }
    }

    #endregion

    #region Animation Coroutines

    private IEnumerator OpenAnimation()
    {
        IsAnimating = true;

        float elapsed = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        Vector3 startScale = useScaleAnimation ? closedScale : originalScale;
        rectTransform.localScale = startScale;

        while (elapsed < openDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / openDuration;
            float curveValue = openCurve.Evaluate(normalizedTime);

            // Animate alpha
            canvasGroup.alpha = curveValue;

            // Animate scale
            if (useScaleAnimation)
            {
                rectTransform.localScale = Vector3.Lerp(startScale, originalScale, curveValue);
            }

            yield return null;
        }

        // Ensure final values
        canvasGroup.alpha = 1f;
        rectTransform.localScale = originalScale;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        IsOpen = true;
        IsAnimating = false;

        OnDialogueOpened?.Invoke();
    }

    private IEnumerator CloseAnimation()
    {
        IsAnimating = true;

        float elapsed = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        Vector3 startScale = rectTransform.localScale;
        Vector3 targetScale = useScaleAnimation ? closedScale : originalScale;

        while (elapsed < closeDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / closeDuration;
            float curveValue = closeCurve.Evaluate(normalizedTime);

            // Animate alpha
            canvasGroup.alpha = 1f - curveValue;

            // Animate scale
            if (useScaleAnimation)
            {
                rectTransform.localScale = Vector3.Lerp(startScale, targetScale, curveValue);
            }

            yield return null;
        }

        // Ensure final values
        canvasGroup.alpha = 0f;
        if (useScaleAnimation)
        {
            rectTransform.localScale = closedScale;
        }

        IsOpen = false;
        IsAnimating = false;

        OnDialogueClosed?.Invoke();
    }

    #endregion

    #region Private Methods

    private void StopCurrentAnimation()
    {
        if (currentAnimationCoroutine != null)
        {
            StopCoroutine(currentAnimationCoroutine);
            currentAnimationCoroutine = null;
        }
        IsAnimating = false;
    }

    private void OnContinueButtonPressed()
    {
        OnContinuePressed?.Invoke();
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Resets the dialogue box to its default state
    /// </summary>
    public void ResetDialogue()
    {
        StopCurrentAnimation();
        HideImmediate();

        if (speakerNameText != null)
        {
            speakerNameText.text = "";
        }

        if (dialogueText != null)
        {
            dialogueText.text = "";
        }
    }

    #endregion

    private void OnDestroy()
    {
        // Clean up button listeners
        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(OnContinueButtonPressed);
        }
    }
}