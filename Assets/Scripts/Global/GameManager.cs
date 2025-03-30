using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Singleton
    public static GameManager Instance;
    
    [Header("Data for the race")]
    public List<Transform> playerSpawns;
    [HideInInspector] public Checkpoint[] allCheckpoints; // Needs to be publicly available
    private List<PlayerRaceManager> players;
    private int _nbPlayerFinished;
    
    [Header("Gameplay balance")]
    public int maxTurns = 3;
    public int maxScore = 10;
    public int lostScorePerPlace = 2;
    [Range(0, 1)] public float pctCheckpointsNeededForTurn = 0.75f;
    
    [Header("Optimisation and tweaks")]
    public float delayBetweenPositionChecks = 0.5f;
    [SerializeField] private float timeStartRace;
    [SerializeField] private float timeEndRace;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI _globalText;

    private void Awake()
    {
        // Singleton boilerplate
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Get checkpoints in scene by hierarchy order (first of the track is index 0, last is ^1)
        allCheckpoints = FindObjectsByType<Checkpoint>(FindObjectsSortMode.None).OrderBy(checkpoint => checkpoint.transform.GetSiblingIndex()).ToArray();
        int index = 0;
        // Assign each checkpoint with an increasing index
        foreach (var checkpoint in allCheckpoints)
        {
            checkpoint.SetIndex(index++);
        }
        // Get all players, order does not matter
        players = FindObjectsByType<PlayerRaceManager>(FindObjectsSortMode.None).ToList();
    }

    private void OnEnable()
    {
        PlayerRaceManager.OnPlayerFinished += OnPlayerFinished;
    }

    private void OnDisable()
    {
        PlayerRaceManager.OnPlayerFinished -= OnPlayerFinished;

    }

    private void Start()
    {
        StartCoroutine(StartRace());
        StartCoroutine(UpdatePlayerPositions());
    }

    private IEnumerator StartRace()
    {
        // Disable player control and move them at start positions
        _nbPlayerFinished = 0;
        for (int i = 0; i < players.Count; i++)
        {
            players[i].transform.position = playerSpawns[i].position;
            players[i].transform.rotation = playerSpawns[i].rotation;
            players[i].Reset();
            players[i].DisableMovement();
        }
        StartCoroutine(DisplayText("Ready ?", timeStartRace));
        // Start turbo could be handled here
        yield return new WaitForSeconds(timeStartRace);
        StartCoroutine(DisplayText("Go !!!", timeStartRace));
        // Re-enable player control
        for (int i = 0; i < players.Count; i++)
        {
            players[i].EnableMovement();
        }
    }

    private IEnumerator UpdatePlayerPositions()
    {
        // This needs to run at all times while the game is played
        while (true)
        {
            // Order players by most advanced on the race (highest turn, then highest checkpoint passed, then highest distance from it)
            players = players.OrderByDescending(p=>p.GetCurrentTurn())
                .ThenByDescending(p => p.GetLastCheckpoint())
                .ThenByDescending(p => p.GetDistanceFromLastCheckpoint())
                .ToList();
            // Tell each player their position in the race
            for (int i = 0; i < players.Count; i++)
            {
                players[i].UpdatePositionText($"Position : {i + 1}");
            }
            // We don't need to check this every frame
            yield return new WaitForSeconds(delayBetweenPositionChecks);
        }
    }

    private void OnPlayerFinished(PlayerRaceManager player)
    {
        // Ignore players doing laps after they finished
        if (!player.isRacing) return;
        
        // Take player into account
        player.isRacing = false;
        // Players gain less score when they finished late
        player.GainScore(maxScore - _nbPlayerFinished*lostScorePerPlace);
        _nbPlayerFinished++;
        
        // If only one player has not finished, the race is over
        if (_nbPlayerFinished == players.Count - 1)
        {
            // Score the last player still racing
            // Don't find it by position in case something wild happened at last moment and positions were not updated
            PlayerRaceManager lastPlayer = players.Find(x => x.isRacing);
            lastPlayer.isRacing = false;
            lastPlayer.GainScore(maxScore - _nbPlayerFinished*lostScorePerPlace);
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