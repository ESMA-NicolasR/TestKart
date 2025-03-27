using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemChampignon", menuName = "Data/Item/ItemChampignon")]
public class ItemChampignon : Item
{
    public float speedIncrease;
    public float decayTime;
    public override void Activation(PlayerItemManager playerItemManager)
    {
        playerItemManager.playerCarController.Boost(speedIncrease, decayTime);
    }
}
