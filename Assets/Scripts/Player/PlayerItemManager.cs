using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerItemManager : MonoBehaviour
{
    [Header("Item management")]
    [SerializeField] private List<Item> _itemList;
    public Transform itemDropLocation;
    private Item _currentItem;
    private int _numberOfItemUse;
    
    [Header("UI")]
    [SerializeField] private Image _itemImage;

    // Internal components
    private PlayerInputManager _playerInputManager;
    [HideInInspector] public PlayerCarController playerCarController; // Has to be accessed publicly

    private void Awake()
    {
        _playerInputManager = GetComponent<PlayerInputManager>();
        playerCarController = GetComponent<PlayerCarController>();
    }

    private void Update()
    {
        // Read inputs
        if (_playerInputManager.itemPressed)
        {
            UseItem();
        }
    }

    public void Reset()
    {
        _numberOfItemUse = 0;
        _currentItem = null;
        _itemImage.sprite = null;
    }

    public void GenerateItem()
    {
        // Only pick a random item if we have none
        if (_currentItem == null)
        {
            _currentItem = _itemList[Random.Range(0, _itemList.Count)];
            _itemImage.sprite = _currentItem.sprite;
            _numberOfItemUse = _currentItem.nbUse;
        }
    }

    private void UseItem()
    {
        // Only use if we have an item
        if (_currentItem != null)
        {
            _currentItem.Activation(this);
            // Lower uses left then check we still have some
            if (--_numberOfItemUse <= 0)
            {
                Reset();
            }
        }
    }
}
