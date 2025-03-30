using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    // Gameplay
    private int _index;

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
        if (other.TryGetComponent<PlayerRaceManager>(out var player))
        {
            player.PassCheckpoint(_index);
        }
    }
}
