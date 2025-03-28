using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    public Checkpoint[] allCheckpoints;
    private List<PlayerRaceManager> players;
    private int _nbPlayerFinished;
    private static float DELAY_BETWEEN_POSITION_CHECKS = 0.5f;
    private static int MAX_SCORE = 10;
    private static int LOST_SCORE_PER_PLACE = 2;
    private bool _raceIsOver;

    private void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        PlayerRaceManager.OnPlayerFinished += OnPlayerFinished;
        // Get checkpoints in scene by hierarchy order
        allCheckpoints = FindObjectsByType<Checkpoint>(FindObjectsSortMode.None).OrderBy(checkpoint => checkpoint.transform.GetSiblingIndex()).ToArray();
        var index = 0;
        // Mark each checkpoint with an increasing index
        foreach (var checkpoint in allCheckpoints)
        {
            checkpoint.SetIndex(index++);
        }
        players = FindObjectsByType<PlayerRaceManager>(FindObjectsSortMode.None).ToList();
    }

    private void Start()
    {
        _nbPlayerFinished = 0;
        _raceIsOver = false;
        StartCoroutine(UpdatePlayerPositions());
    }

    private IEnumerator UpdatePlayerPositions()
    {
        while (!_raceIsOver)
        {
            // Order players by most advanced on the race (highest turn, then highest checkpoint passed, then highest distance from it)
            players = players.OrderByDescending(p=>p.GetCurrentTurn())
                .ThenByDescending(p => p.GetLastCheckpoint())
                .ThenByDescending(p => p.GetDistanceFromLastCheckpoint())
                .ToList();
            for (int i = 0; i < players.Count; i++)
            {
                players[i].UpdatePositionText($"Position : {i + 1}");
            }
            // We don't need to check this every frame
            yield return new WaitForSeconds(DELAY_BETWEEN_POSITION_CHECKS);
        }
    }

    private void OnPlayerFinished(PlayerRaceManager player)
    {
        player.GainScore(MAX_SCORE - _nbPlayerFinished*LOST_SCORE_PER_PLACE);
        _nbPlayerFinished++;
        _raceIsOver = _nbPlayerFinished >= players.Count - 1;
    }
}