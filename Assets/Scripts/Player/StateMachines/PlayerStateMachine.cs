using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateMachine 
{
   public PlayerState _currentPlayerState { get; set; }


    //This function lets the PlayerState know that which state we are currently in
    public void Initialize(PlayerState startingState)
    {
        //Debug.Log($"Initializing state machine with state {startingState}");

        _currentPlayerState = startingState;
        _currentPlayerState.EnterState();
    }

    //This function handle the transition from one state to another
    public void ChangeState(PlayerState newState)
    {
        //Debug.Log($"Changing State to {newState}");

        _currentPlayerState.ExitState();
        _currentPlayerState = newState;
        _currentPlayerState.EnterState();
    }
}
