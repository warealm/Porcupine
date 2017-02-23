using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager {

    //This is a list of all live inventories
    //Later on this will likely be organised by rooms insteads
    public Dictionary<string, List<Inventory>> inventories;

    public InventoryManager()
    {
        inventories = new Dictionary<string, List<Inventory>>();
    }


    public bool PlaceInventory(Tile tile, Inventory inv)
    {

        bool tileWasEmpty = tile.inventory == null;

        if(tile.PlaceInventory(inv) == false)
        {
            //the tile did not accept the inventory, so stop
            return false;

        }

        //at this point, inv might be an empty stack
        if(inv.stackSize == 0)
        {
            if (inventories.ContainsKey(tile.inventory.inventoryType))
            {
                inventories[inv.inventoryType].Remove(inv);
            }
                
        }

        //we may have also created a new stack on the tile, if the tile was previously empty
        if (tileWasEmpty)
        {
            if(inventories.ContainsKey(tile.inventory.inventoryType) == false)
            {
                inventories[tile.inventory.inventoryType] = new List<Inventory>();
            }

            inventories[tile.inventory.inventoryType].Add(tile.inventory);
        }


        return true;

    }

}
