using UnityEngine;

public class PatchTurbo : MonoBehaviour
{
    [SerializeField] private float _boostMultiplier;
    [SerializeField] private float _boostDuration;
    
    private void OnTriggerEnter(Collider other)
    {
        // Check if what triggered is roughly in the same direction as the patch
        bool isAligned = Vector3.Angle(transform.forward, other.transform.forward) <= 90f;
        // And if it's a player, give it a boost
        if(isAligned && other.TryGetComponent<PlayerCarController>(out var player))
        {
            player.Boost(_boostMultiplier, _boostDuration);
        }
    }
}
