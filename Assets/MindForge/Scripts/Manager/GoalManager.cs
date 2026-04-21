using System;
using UnityEngine;
using System.Collections.Generic;

public class GoalManager : MonoBehaviour
{
    public static GoalManager Instance { get; private set; }

    [Header("Elements")]
    [SerializeField] private Transform goalCardParent;
    [SerializeField] private GoalCard goalCardPrefab;

    [Header("Data")]
    private ItemLevelData[] goals;
    private List<GoalCard> goalCards = new List<GoalCard>();

    public ItemLevelData[] Goals => goals;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        LevelManager.levelSpawned += OnLevelSpawned;
        ItemSpotsManager.itemPickedUp += OnItemPickedUp;
        PowerupManager.itemPickedUp += OnItemPickedUp;
    }

    private void OnDestroy()
    {
        LevelManager.levelSpawned -= OnLevelSpawned;
        ItemSpotsManager.itemPickedUp -= OnItemPickedUp;
        PowerupManager.itemPickedUp -= OnItemPickedUp;
    }

    private void OnLevelSpawned(Level level)
    {
        goals = level.GetGoals();

        ClearGoalCards(); // Xóa cards cũ trước
        GenerateGoalCards();
    }

    private void ClearGoalCards()
    {
        foreach (GoalCard card in goalCards)
        {
            Destroy(card.gameObject);
        }
        goalCards.Clear();
    }

    private void GenerateGoalCards()
    {
        for (int i = 0; i < goals.Length; i++)
        {
            GenerateGoalCard(goals[i]);
        }
    }

    private void GenerateGoalCard(ItemLevelData goal)
    {
        GoalCard cardInstance = Instantiate(goalCardPrefab, goalCardParent);
        cardInstance.Configure(goal.amount, goal.itemPrefab.Icon);

        goalCards.Add(cardInstance);
    }

    private void OnItemPickedUp(Item item)
    {
        Debug.Log($"Item Picked Up: {item.ItemName}");

        for (int i = 0; i < goals.Length; i++)
        {
            if (!goals[i].itemPrefab.ItemName.Equals(item.ItemName))
            {
                Debug.Log($"Item {item.ItemName} does not match goal {goals[i].itemPrefab.ItemName}");
                continue;
            }

            goals[i].amount--;

            if (goals[i].amount <= 0)
                CompleteGoal(i);
            else
                goalCards[i].UpdateAmount(goals[i].amount);
            break;
        }
    }

    private void CompleteGoal(int goalIndex)
    {
        Debug.Log("Goal Complete: " + goals[goalIndex].itemPrefab.ItemName);

        goalCards[goalIndex].Complete();

        CheckForLevelComplete();
    }

    private void CheckForLevelComplete()
    {
        for (int i = 0; i < goals.Length; i++)
        {
            if (goals[i].amount > 0)
                return;
        }

        GameManager.Instance.SetGameState(EGameState.LEVELCOMPLETE);
        //Debug.Log("Level Complete!");
    }
}
