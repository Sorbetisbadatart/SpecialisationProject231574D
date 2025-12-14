using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private DialogueUI dialogueUI;

    private float MoveSpeed = 10f;

    private Vector2 playerPosition;
    public DialogueUI DialogueUI => dialogueUI;
    public Iinteractable Interactable { get; set; }

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (dialogueUI.IsOpen) return;

        Vector3 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
   
        rb.MovePosition(rb.position + input.normalized * (MoveSpeed * Time.deltaTime));

        playerPosition = rb.position;

        if (Input.GetKeyDown(KeyCode.E))
        {
            Interactable?.Interact(this);
        }
    }

    public Vector2 GetPosition()
    {
        return playerPosition;
    }

    public void SetPosition(Vector2 position)
    {
        rb.position = position;
    } 


    
}
