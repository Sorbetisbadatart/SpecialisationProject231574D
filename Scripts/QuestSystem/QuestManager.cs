using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    // Events
    public static event Action<QuestScriptableObject> OnQuestReceived;
    public static event Action<QuestScriptableObject> OnQuestCompleted;

    // Quest tracking
    private Dictionary<string, QuestData> activeQuests = new Dictionary<string, QuestData>();
    private Dictionary<string, QuestScriptableObject> completedQuests = new Dictionary<string, QuestScriptableObject>();

    [SerializeField] private List<QuestScriptableObject> availableQuests = new List<QuestScriptableObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Initialize any starting quests
        InitializeStartingQuests();
    }

    /// <summary>
    /// Adds a quest to the player's active quests
    /// </summary>
    public void AcceptQuest(QuestScriptableObject quest)
    {
        if (quest == null)
        {
            Debug.LogWarning("Attempted to accept null quest!");
            return;
        }

        if (activeQuests.ContainsKey(quest.questID) || completedQuests.ContainsKey(quest.questID))
        {
            Debug.Log($"Quest {quest.questName} is already active or completed!");
            return;
        }

        var questData = new QuestData(quest);
        activeQuests.Add(quest.questID, questData);

        // Invoke the event
        OnQuestReceived?.Invoke(quest);

        Debug.Log($"Quest accepted: {quest.questName}");
    }

    /// <summary>
    /// Updates progress for active quests
    /// </summary>
    public void UpdateQuestProgress(string questID, int amount = 1)
    {
        if (activeQuests.TryGetValue(questID, out QuestData questData))
        {
            questData.currentAmount += amount;

            // Check if quest is complete
            if (questData.currentAmount >= questData.quest.requiredAmount)
            {
                CompleteQuest(questID);
            }
        }
    }

    /// <summary>
    /// Updates kill quest progress
    /// </summary>
    public void UpdateKillQuest(string enemyType, int amount = 1)
    {
        foreach (var questData in activeQuests.Values)
        {
            if (questData.quest.questType == QuestType.KillQuest &&
                questData.quest.targetName == enemyType)
            {
                UpdateQuestProgress(questData.quest.questID, amount);
            }
        }
    }

    ///// <summary>
    ///// Checks if player has required item for item check quests
    ///// </summary>
    //public void CheckItemQuest(ItemScriptableObject item)
    //{
    //    foreach (var questData in activeQuests.Values)
    //    {
    //        if (questData.quest.questType == QuestType.ItemCheck &&
    //            questData.quest.requiredItem == item)
    //        {
    //            CompleteQuest(questData.quest.questID);
    //        }
    //    }
    //}

    /// <summary>
    /// Completes a quest and gives rewards
    /// </summary>
    private void CompleteQuest(string questID)
    {
        if (activeQuests.TryGetValue(questID, out QuestData questData))
        {
            // Remove from active quests
            activeQuests.Remove(questID);

            // Add to completed quests
            completedQuests.Add(questID, questData.quest);

            // Give rewards
            GiveQuestRewards(questData.quest);

            // Invoke the event
            OnQuestCompleted?.Invoke(questData.quest);

            Debug.Log($"Quest completed: {questData.quest.questName}");

            // Check for next quest in chain
            if (questData.quest.nextQuest != null)
            {
                AcceptQuest(questData.quest.nextQuest);
            }
        }
    }

    /// <summary>
    /// Gives rewards for completed quest
    /// </summary>
    private void GiveQuestRewards(QuestScriptableObject quest)
    {
        // Give experience
        if (quest.experienceReward > 0)
        {
            // Implement your experience system here
            Debug.Log($"Gained {quest.experienceReward} experience!");
        }

        // Give gold
        if (quest.goldReward > 0)
        {
            // Implement your currency system here
            Debug.Log($"Gained {quest.goldReward} gold!");
        }

        //// Give items
        //if (quest.itemRewards != null && quest.itemRewards.Length > 0)
        //{
        //    foreach (var item in quest.itemRewards)
        //    {
        //        if (item != null)
        //        {
        //            // Implement your inventory system here
        //            Debug.Log($"Received item: {item.name}");
        //        }
        //    }
        //}
    }

    /// <summary>
    /// Initializes any starting quests
    /// </summary>
    private void InitializeStartingQuests()
    {
        foreach (var quest in availableQuests)
        {
            if (quest != null && !activeQuests.ContainsKey(quest.questID) && !completedQuests.ContainsKey(quest.questID))
            {
                AcceptQuest(quest);
            }
        }
    }

    /// <summary>
    /// Checks if a quest is active
    /// </summary>
    public bool IsQuestActive(string questID)
    {
        return activeQuests.ContainsKey(questID);
    }

    /// <summary>
    /// Checks if a quest is completed
    /// </summary>
    public bool IsQuestCompleted(string questID)
    {
        return completedQuests.ContainsKey(questID);
    }

    /// <summary>
    /// Gets progress for an active quest
    /// </summary>
    public (int current, int required) GetQuestProgress(string questID)
    {
        if (activeQuests.TryGetValue(questID, out QuestData questData))
        {
            return (questData.currentAmount, questData.quest.requiredAmount);
        }
        return (0, 0);
    }

    /// <summary>
    /// Gets all active quests
    /// </summary>
    public List<QuestData> GetActiveQuests()
    {
        return new List<QuestData>(activeQuests.Values);
    }
}

[System.Serializable]
public class QuestData
{
    public QuestScriptableObject quest;
    public int currentAmount;

    public QuestData(QuestScriptableObject quest)
    {
        this.quest = quest;
        this.currentAmount = 0;
    }
}