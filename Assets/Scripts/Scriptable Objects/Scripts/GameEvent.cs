using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameState", menuName = "ScriptableObjects/Game State")]
public class GameState : ScriptableObject
{
    public enum State
    {
        PlayerLost,
        PlayerWon,
        Paused,
        Playing
    }
    
    public delegate void GameStateListener(State state);
    private List<GameStateListener> listeners = new List<GameStateListener>();
    private State state;

    public State CurrentState
    {
        get => state;
        set
        {
            state = value;
            Raise();
        }
    }

    private void OnEnable()
    {
        state = State.Paused;
    }
    
    private void Raise()
    {
        foreach (var listener in listeners)
        {
            listener.Invoke(state);
        }
    }

    public void RegisterListener(GameStateListener listener)
    {
        listeners.Add(listener);
    }

    public void UnregisterListener(GameStateListener listener)
    {
        listeners.Remove(listener);
    }
}
