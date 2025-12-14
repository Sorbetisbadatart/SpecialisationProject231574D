using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "DialogueObject", menuName = "Dialogue/DialogueObject")]
public class DialogueObject : ScriptableObject
{
    [SerializeField][TextArea] private string[] dialogue;
    [SerializeField] private Response[] responses;
    [SerializeField] private Sprite[] portrait;
    [SerializeField] private AudioClip[] audioClip;
    [SerializeField] private UnityEvent[] events;
    [SerializeField] private string[] charactername;
    public string[] Dialogue  => dialogue;
    public bool HasResponses => Responses != null && responses.Length > 0;
    public Response[] Responses => responses;

    public Sprite[] Portrait => portrait;
    public AudioClip[] AudioClip => audioClip;  
    public UnityEvent[] Events => events;
    public string[] Charactername => charactername;
    
}
