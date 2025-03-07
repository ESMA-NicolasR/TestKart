using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ItemBox : MonoBehaviour
{
    [SerializeField] private float _respawnTime;
    private MeshRenderer _meshRenderer;
    private Collider _collider;
    [SerializeField] private MeshRenderer _textMeshRenderer;

    private void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        _collider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerItemManager player = other.GetComponent<PlayerItemManager>();
        if (player!=null)
        {
            player.GenerateItem();
            StartCoroutine(RespawnCoroutine());
        }
    }

    private IEnumerator RespawnCoroutine()
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
