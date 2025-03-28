using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    [HideInInspector] public Checkpoint[] allCheckpoints;
    private List<PlayerRaceManager> players;
    public List<Transform> playerSpawns;
    private int _nbPlayerFinished;
    private static float DELAY_BETWEEN_POSITION_CHECKS = 0.5f;
    private static int MAX_SCORE = 10;
    private static int LOST_SCORE_PER_PLACE = 2;
    public int maxTurns = 3;
    public float pctCheckpointsNeededForTurn = 0.75f;

    [SerializeField] private TextMeshProUGUI _globalText;
    [SerializeField] private float timeStartRace;
    [SerializeField] private float timeEndRace;

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
        StartCoroutine(StartRace());
        StartCoroutine(UpdatePlayerPositions());
    }

    private IEnumerator StartRace()
    {
        _nbPlayerFinished = 0;
        for (int i = 0; i < players.Count; i++)
        {
            players[i].transform.position = playerSpawns[i].position;
            players[i].transform.rotation = playerSpawns[i].rotation;
            players[i].Reset();
            players[i].DisableMovement();
        }
        StartCoroutine(DisplayText("Ready ?", timeStartRace));
        yield return new WaitForSeconds(timeStartRace);
        StartCoroutine(DisplayText("Go !!!", timeStartRace));
        for (int i = 0; i < players.Count; i++)
        {
            players[i].transform.position = playerSpawns[i].position;
            players[i].Reset();
            players[i].EnableMovement();
        }
    }

    private IEnumerator UpdatePlayerPositions()
    {
        while (true)
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
        // Ignore players doing laps after they finished
        if (!player.isRacing) return;
        
        // Take player into account
        player.isRacing = false;
        player.GainScore(MAX_SCORE - _nbPlayerFinished*LOST_SCORE_PER_PLACE);
        _nbPlayerFinished++;
        if (_nbPlayerFinished == players.Count - 1)
        {
            // Score the last player still racing
            // Don't find it by position in case something wild happened at last moment and positions are not updated
            PlayerRaceManager lastPlayer = players.Find(x => x.isRacing);
            lastPlayer.isRacing = false;
            lastPlayer.GainScore(MAX_SCORE - _nbPlayerFinished*LOST_SCORE_PER_PLACE);
            StartCoroutine(EndRace());
        }
    }

    private IEnumerator EndRace()
    {
        StartCoroutine(DisplayText("FINISHED !", timeEndRace));
        yield return new WaitForSeconds(timeEndRace);
        StartCoroutine(StartRace());
    }

    private IEnumerator DisplayText(string text, float time)
    {
        _globalText.text = text;
        yield return new WaitForSeconds(time);
        _globalText.text = "";
    }
}