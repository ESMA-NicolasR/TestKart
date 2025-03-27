using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishLine : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        PlayerRaceManager playerRaceManager = other.GetComponent<PlayerRaceManager>();
        if (playerRaceManager != null)
        {
            playerRaceManager.NextTurn();
        }
    }
}
