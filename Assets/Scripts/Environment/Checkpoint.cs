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
        if (other.TryGetComponent<PlayerRaceManager>(out var player))
        {
            player.PassCheckpoint(_index);
        }
    }
}
