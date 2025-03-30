using System;
using UnityEngine;

public class Item : ScriptableObject
{
    public Sprite sprite;
    public int nbUse;
    
    public virtual void Activation(PlayerItemManager playerItemManager)
    {
        throw new NotImplementedException();
    }
}
