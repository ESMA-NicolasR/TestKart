using System.Collections;
using UnityEngine;

public class ItemBox : MonoBehaviour
{
    [Header("Gameplay")]
    [SerializeField] private float _respawnTime;
    
    [Header("UI")]
    [SerializeField] private MeshRenderer _textMeshRenderer;
    
    // Internal components
    private MeshRenderer _meshRenderer;
    private Collider _collider;

    private void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        _collider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent<PlayerItemManager>(out var player))
        {
            player.GenerateItem();
            StartCoroutine(Respawn());
        }
    }

    private IEnumerator Respawn()
    {
        Hide();
        yield return new WaitForSeconds(_respawnTime);
        Show();
    }

    private void Hide()
    {
        _meshRenderer.enabled = false;
        _collider.enabled = false;
        _textMeshRenderer.enabled = false;
    }
    
    private void Show()
    {
        _meshRenderer.enabled = true;
        _collider.enabled = true;
        _textMeshRenderer.enabled = true;
    }
}
