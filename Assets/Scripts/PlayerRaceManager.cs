using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class PlayerRaceManager : MonoBehaviour
{
    [SerializeField] private int _maxTurns;
    private int _currentTurn;
    [SerializeField] private TextMeshProUGUI _turnText;
    private Dictionary<int, bool> _passedCheckpoints;

    private void Start()
    {
        UpdateDisplay();
        _passedCheckpoints = new Dictionary<int, bool>();
        var index = 0;
        foreach (var checkpoint in FindObjectsOfType<Checkpoint>())
        {
            checkpoint.SetIndex(index);
            _passedCheckpoints.Add(index, false);
            index++;
        }
    }

    public void NextTurn()
    {
        bool hasFinishedTurn = true;
        foreach (var keyValuePair in _passedCheckpoints)
        {
            Debug.Log($"Checkpoint {keyValuePair.Key} : {keyValuePair.Value}");
            hasFinishedTurn = hasFinishedTurn && keyValuePair.Value;
        }

        if (hasFinishedTurn)
        {
            _currentTurn++;
            UpdateDisplay();
            foreach (var index in _passedCheckpoints.Keys.ToList())
            {
                _passedCheckpoints[index] = false;
            }
        }
    }

    public void PassCheckpoint(int id)
    {
        _passedCheckpoints[id] = true;
    }
    
    public void UpdateDisplay()
    {
        if(_currentTurn < _maxTurns)
            _turnText.text = $"Turn {_currentTurn}/{_maxTurns}";
        else
        {
            _turnText.text = $"Finished !";
        }
    }
}
