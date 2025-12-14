using UnityEngine;

[CreateAssetMenu(fileName = "New Quest", menuName = "RPG System/Quest")]
public class QuestScriptableObject : ScriptableObject
{
    [Header("Quest Information")]
    public string questID;
    public string questName;
    public string description;

    [Header("Quest Type")]
    public QuestType questType;

    [Header("Quest Requirements")]
    public int requiredAmount;
    public string targetName; // For kill quests or specific targets
   // public ItemScriptableObject requiredItem; // For item check quests

    [Header("Rewards")]
    public int experienceReward;
    public int goldReward;
   // public ItemScriptableObject[] itemRewards;

    [Header("Quest Chain")]
    public QuestScriptableObject nextQuest;

    [TextArea(3, 10)]
    public string completionText;
}

public enum QuestType
{
    FetchQuest,
    KillQuest,
    ItemCheck
}