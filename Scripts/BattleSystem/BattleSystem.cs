using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum BattleState
{
    START,
    PLAYERTURN,
    ENEMYTURN,
    WON,
    LOST,
    
}
public class BattleSystem : MonoBehaviour
{
    //Object references
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject BattleUI;

    [SerializeField] private Transform playerBattleStation;
    [SerializeField] private Transform enemyBattleStation;

    //UI Elements
    [SerializeField] private TMP_Text dialogueText; // BS (battle system) dialogue text 
    [SerializeField] private BattleSystemUI playerHUD;// BS player UI
    [SerializeField] private BattleSystemUI enemyHUD; //BS enemy UI

    [SerializeField] private float TurnTransitionTime = 3f;
    [SerializeField] private float AttackBufferTime = 1f;
    //private members
    private Unit playerUnit;
    private Unit enemyUnit;
    public BattleState currentstate; //current state of the battle

    private void Start()
    {
        
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B)){
            StartBattle();
        }
    }

    public void StartBattle()
    {
        ShiftBattleState(BattleState.START);
        BattleUI.SetActive(true);
    }
    private IEnumerator SetupBattle()
    {
        GameObject playerGO = Instantiate(playerPrefab, playerBattleStation);

        playerUnit = playerGO.GetComponent<Unit>();

        GameObject enemyGO = Instantiate(enemyPrefab, enemyBattleStation);
        enemyUnit = enemyGO.GetComponent<Unit>();

        dialogueText.text = "A wild " + enemyUnit.name + " Appears!";

        playerHUD.InitialiseHUD(playerUnit);
        enemyHUD.InitialiseHUD(enemyUnit);

        yield return new WaitForSeconds(TurnTransitionTime);

        ShiftBattleState(BattleState.PLAYERTURN);
    }
    private void EndBattle()
    {
        if (currentstate == BattleState.WON)
        {
            dialogueText.text = "You won the battle!";
        }
        else if (currentstate == BattleState.LOST)
        {
            dialogueText.text = "You suck why u lose";
        }
        else
        {
            Debug.Log("current unused state is " + currentstate.ToString());
            Debug.LogError("You are not supposed to see this, check last state before this function call");
        }

        BattleUI.SetActive(false);
    }
    private IEnumerator EnemyTurn()
    {
        dialogueText.text = enemyUnit.unitName + " attacks";
        yield return new WaitForSeconds(AttackBufferTime);

        bool isDead = playerUnit.TakeDamage(enemyUnit.damage);
        UpdateHealthHUD(playerHUD, playerUnit);

        yield return new WaitForSeconds(AttackBufferTime);

        if (isDead)
        {
            //end battle
            ShiftBattleState(BattleState.LOST);
        }
        else
        {
            //shift turn to player       
            ShiftBattleState(BattleState.PLAYERTURN);
        }
    }

    private void StartPlayerTurn()
    {
        dialogueText.text = "Choose an action:";
    }

    //Action function Template (Button + Function)
    //Attack
    public void OnAttackButton()
    {
        //allow attack only during player's turn
        if (currentstate != BattleState.PLAYERTURN)
            return;

        TriggerAttack();
      
    }
    private IEnumerator PlayerAttack()
    {
        //do action - damage enemy
        bool isDead = enemyUnit.TakeDamage(playerUnit.damage);
       
        //update UI
        UpdateHealthHUD(enemyHUD, enemyUnit);
        dialogueText.text = "Attack has hit!";

        yield return new WaitForSeconds(AttackBufferTime);

        //check if enemy died    
        if (isDead)
        {
            //end battle
            ShiftBattleState(BattleState.WON);
        }
        else
        {
            //shift turn to enemy
            ShiftBattleState(BattleState.ENEMYTURN);
        }
    }

    private void ShiftBattleState(BattleState newState)
    {
        currentstate = newState;
        switch (currentstate)
        {
            case BattleState.START:
                StartCoroutine(nameof(SetupBattle));
                break;
            case BattleState.PLAYERTURN:
                TriggerPlayerTurn();
                break;
            case BattleState.ENEMYTURN:
                TriggerEnemyTurn();
                break;
            case BattleState.LOST:
                EndBattle();
                break;
            case BattleState.WON:
                EndBattle();
                break;
            default:
                Debug.Log("You missed a testcase");
                break;
        }
    }

    //Helper functions
    //Updates health UI
    private void UpdateHealthHUD(BattleSystemUI unitHUD, Unit unit)
    {
        unitHUD.UpdateHealthUI(unit.currentHealth);
    }
    private void TriggerAttack()
    {
         StartCoroutine(nameof(PlayerAttack));
    }

    private void TriggerPlayerTurn()
    {
        //shift turn            
        StartCoroutine(nameof(StartPlayerTurn));
    }

    private void TriggerEnemyTurn()
    {
        //shift turn            
        StartCoroutine(nameof(EnemyTurn));
    }
}
