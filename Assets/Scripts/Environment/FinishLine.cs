using UnityEngine;

public class FinishLine : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerRaceManager>(out var player))
        {
            player.NextTurn();
        }
    }
}
