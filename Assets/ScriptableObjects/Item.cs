using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : ScriptableObject
{
    public Sprite sprite;
    public string itemName;
    public int nbUse;


    public virtual void Activation(PlayerItemManager playerItemManager)
    {
        return;
    }
}
