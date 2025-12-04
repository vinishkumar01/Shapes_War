using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

//This script actually to keep track of which state we're currently in
public class EnemyStateMachine
{
    public EnemyState currentEnemyState { get; set; }

    // this function lets the EnemyState know that which state we are currently in
    public void initialize(EnemyState startingState)
    {
       // Debug.Log($"Initializing state machine with state {startingState}");

        currentEnemyState = startingState;
        currentEnemyState.EnterState();
    }

    // This function handle the transition from one state to another 
    public void ChangeState(EnemyState newState)
    {
        //Debug.Log($"Changing state to: {newState}");

        currentEnemyState.ExitState();
        currentEnemyState = newState;
        currentEnemyState.EnterState();
    }
}
