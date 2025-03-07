using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class PlayerItemManager : MonoBehaviour
{
    [SerializeField]
    private List<Item> _itemList;
    private Item _currentItem;
    [SerializeField]
    private Image _itemImage;
    public CarControllerSimple carController;

    private int _numberOfItemUse;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            UseItem();
        }
    }

    public void GenerateItem()
    {
        if (_currentItem == null)
        {
            _currentItem = _itemList[Random.Range(0, _itemList.Count)];
            _itemImage.sprite = _currentItem.sprite;
            _numberOfItemUse = _currentItem.nbUse;
        }
    }

    public void UseItem()
    {
        if (_currentItem != null)
        {
            _currentItem.Activation(this);
            if (--_numberOfItemUse <= 0)
            {
                _currentItem = null;
                _itemImage.sprite = null;
            }
        }
    }
}
