using UnityEngine;

[CreateAssetMenu(fileName = "ItemLaunchable", menuName = "Data/Item/ItemLaunchable")]
public class ItemLaunchable : Item
{
    public GameObject prefabToLaunch;

    public override void Activation(PlayerItemManager playerItemManager)
    {
        Instantiate(prefabToLaunch, playerItemManager.itemDropLocation.position, playerItemManager.transform.rotation);
    }
}
