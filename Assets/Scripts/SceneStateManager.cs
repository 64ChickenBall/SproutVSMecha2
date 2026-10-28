using UnityEngine;
using System;

public enum SceneState
{
    Intro,
    Normal,
    Scared,
    ScaredEnding,
    GameOver
}

public class SceneStateManager : MonoBehaviour
{
    public static SceneStateManager Instance { get; private set; }

    public SceneState CurrentState { get; private set; } = SceneState.Intro;

    // Event that other scripts can listen to
    public event Action<SceneState> OnSceneStateChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void SetState(SceneState newState)
    {
        if (CurrentState == newState) return;

        CurrentState = newState;
        Debug.Log($"Scene State changed to: {CurrentState}");

        // Notify all subscribed listeners
        OnSceneStateChanged?.Invoke(CurrentState);
    }
}