using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : Singleton<EventManager>
{
    #region UI_ACTIONS
    public event Action<int> OnHotbarSlotChange;
    public event Action<GameObject> OnHotbarItemSelect;
    public event Action<ChestBuilding> OnOpenChestUI;
    public event Action<FactoryBuilding> OnOpenBuildingUI;
    public event Action OnOpenInventoryUI;
    public event Action OnCloseInventoryUI;
    public event Action<ItemEntity> OnPickUpWorldObject;
    public event Action<ChestSlot> OnPickUpChestItem;
    public event Action<ItemStack> OnInventoryUpdate;
    public event Action<bool> OnBuildModeActive;
    public void EventHotbarSlotChange(int idx)
    {
        OnHotbarSlotChange?.Invoke(idx);
    }
    
    public void EventHotbarItemSelect(GameObject idx)
    {
        OnHotbarItemSelect?.Invoke(idx);
    }
    
    public void EventOpenChestUI(ChestBuilding chest)
    {
        OnOpenChestUI?.Invoke(chest);
    }
    
    public void EventOpenBuildingUI(FactoryBuilding building)
    {
        OnOpenBuildingUI?.Invoke(building);
    }

    public void EventOpenInventoryUI()
    {
        OnOpenInventoryUI?.Invoke();
    }

    public void EventCloseInventoryUI()
    {
        OnCloseInventoryUI?.Invoke();
    }
    public void EventPickUpItemEntity(ItemEntity itemEntity)
    {
        OnPickUpWorldObject?.Invoke(itemEntity);
    }
    public void EventPickUpChestItem(ChestSlot chestItem)
    {
        OnPickUpChestItem?.Invoke(chestItem);
    }
    public void EventInventoryUpdate(ItemStack stack)
    {
        OnInventoryUpdate?.Invoke(stack);
    }
    
    public void EventBuildModeActive(bool active)
    {
        OnBuildModeActive?.Invoke(active);
    }
    #endregion
}
