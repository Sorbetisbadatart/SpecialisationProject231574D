using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class ResponseEvent 
{
    [HideInInspector] public string name;
    [SerializeField] public UnityEvent onPickedResponse;
    public UnityEvent OnPickResponse => onPickedResponse;
}
