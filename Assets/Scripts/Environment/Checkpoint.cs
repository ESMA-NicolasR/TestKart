using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Checkpoint : MonoBehaviour
{
    [FormerlySerializedAs("_id")] [SerializeField] private int _index;

    public void SetIndex(int id)
    {
        _index = id;
    }

    public int GetIndex()
    {
        return _index;
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerRaceManager playerRaceManager = other.GetComponent<PlayerRaceManager>();
        if (playerRaceManager != null)
        {
            playerRaceManager.PassCheckpoint(_index);
        }
    }
}
