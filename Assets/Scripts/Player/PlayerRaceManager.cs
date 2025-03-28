using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class PlayerRaceManager : MonoBehaviour
{
    private int _currentTurn;
    [SerializeField] private TextMeshProUGUI _turnText;
    [SerializeField] private TextMeshProUGUI _positionText;
    [SerializeField] private TextMeshProUGUI _scoreText;
    private Dictionary<int, bool> _passedCheckpoints;
    private int _lastCheckpoint;
    private int _score;
    private PlayerCarController _carController;
    public bool isRacing;

    public static event Action<PlayerRaceManager> OnPlayerFinished;

    private void Awake()
    {
        _carController = GetComponent<PlayerCarController>();
    }

    private void Start()
    {
        Reset();
    }

    public void Reset()
    {
        _carController.Reset();
        // Init checkpoints as not passed yet
        _passedCheckpoints = new Dictionary<int, bool>();
        foreach (var checkpoint in GameManager.Instance.allCheckpoints)
        {
            _passedCheckpoints.Add(checkpoint.GetIndex(), false);
        }
        _lastCheckpoint = -1;
        // Set as first turn
        _currentTurn = 0;
        UpdateTurnText();
        // Set ready for race
        isRacing = true;

    }

    public void NextTurn()
    {
        // Count how many checkpoints we passed, cast as float to get a % out of it after
        float nbCheckpointsPassed = _passedCheckpoints.Count(pair => pair.Value);
        float ratioPassed = nbCheckpointsPassed / GameManager.Instance.allCheckpoints.Length;
        // Check we took enough checkpoints for the turn
        bool hasFinishedTurn = ratioPassed >= GameManager.Instance.pctCheckpointsNeededForTurn;

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

    public void EnableMovement()
    {
        _carController.canMove = true;
    }
    
    public void DisableMovement()
    {
        _carController.canMove = false;
    }
    
    private void UpdateTurnText()
    {
        _turnText.text = $"Turn {_currentTurn}/{GameManager.Instance.maxTurns}";
    }

    private void CheckHasFinished()
    {
        if (_currentTurn >= GameManager.Instance.maxTurns)
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
