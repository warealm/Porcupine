﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Looseobjects are things that are lying on the floor/stockpile, like a bunch of metal bars
//or potentially a non-installed copy of furniture (e.g. a cabinet still in the box from IKEA)

public class Inventory  {

    public string inventoryType = "Steel Plate";
    public int maxStackSize = 50;
    public int stackSize = 1;

    public Tile tile;
    public Character character;

    public Inventory()
    {

    }

    protected Inventory(Inventory other)
    {
        inventoryType = other.inventoryType;
        maxStackSize = other.maxStackSize;
        stackSize = other.stackSize;

    }

    public virtual Inventory Clone()
    {
        return new Inventory(this);
    }



}
