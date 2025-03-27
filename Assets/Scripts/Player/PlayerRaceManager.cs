using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRaceManager : MonoBehaviour
{
    [SerializeField] private int _maxTurns;
    private int _currentTurn;
    [SerializeField] private TextMeshProUGUI _turnText;
    [SerializeField] private TextMeshProUGUI _positionText;
    [SerializeField] private TextMeshProUGUI _scoreText;
    private Dictionary<int, bool> _passedCheckpoints;
    private static float PCT_CHECKPOINTS_NEEDED_FOR_TURN = 0.75f;
    private int _lastCheckpoint;
    private int _score;
    private Func<KeyValuePair<int, bool>, bool> _filterPassed;

    public static event Action<PlayerRaceManager> OnPlayerFinished;
    private void Start()
    {
        UpdateTurnText();
        // Init checkpoints as not passed yet
        _passedCheckpoints = new Dictionary<int, bool>();
        foreach (var checkpoint in GameManager.Instance.allCheckpoints)
        {
            _passedCheckpoints.Add(checkpoint.GetIndex(), false);
        }
        // Set up filter to count passed checkpoints
        _filterPassed = (kv) => kv.Value;
        _lastCheckpoint = -1;
    }

    public void NextTurn()
    {
        // Count how much checkpoints we passed
        int nbCheckpointsPassed = _passedCheckpoints.Count(_filterPassed);
        float ratioPassed = (float)nbCheckpointsPassed / GameManager.Instance.allCheckpoints.Length;

        // Check we took enough checkpoints for the turn
        bool hasFinishedTurn = ratioPassed >= PCT_CHECKPOINTS_NEEDED_FOR_TURN;

        if (hasFinishedTurn)
        {
            _currentTurn++;
            UpdateTurnText();
            // Reset checkpoints passed
            foreach (var index in _passedCheckpoints.Keys.ToList())
            {
                _passedCheckpoints[index] = false;
            }

            _lastCheckpoint = 0;
            CheckHasFinished();
        }
    }

    public void PassCheckpoint(int id)
    {
        _passedCheckpoints[id] = true;
        _lastCheckpoint = id;
    }
    
    public void UpdateTurnText()
    {
        _turnText.text = $"Turn {_currentTurn}/{_maxTurns}";
    }

    public void CheckHasFinished()
    {
        if (_currentTurn >= _maxTurns)
        {
            _turnText.text = $"Finished !";
            OnPlayerFinished?.Invoke(this);
        }
    }

    public int GetCurrentTurn()
    {
        return _currentTurn;
    }
    
    public int GetLastCheckpoint()
    {
        return _lastCheckpoint;
    }

    public float GetDistanceFromLastCheckpoint()
    {
        Checkpoint referenceCheckpoint = _lastCheckpoint == -1
            ? GameManager.Instance.allCheckpoints[^1]
            : GameManager.Instance.allCheckpoints[_lastCheckpoint];
        return Vector3.Distance(transform.position, referenceCheckpoint.transform.position);
    }

    public void UpdatePositionText(string newPosition)
    {
        _positionText.text = newPosition;
    }

    public void GainScore(int scoreGained)
    {
        _score += scoreGained;
        _scoreText.text = $"Score: {_score}";
    }
}
