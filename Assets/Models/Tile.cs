using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum TileType { Empty, Floor };

public class Tile {



    private TileType type = TileType.Empty;

    Action<Tile> cbTileTypeChanged;

    private LooseObject looseObject;
    private InstalledObject installedObject;

    //know which world it's in
    World world;
    //co-ordinates of the world
    private int x;
    private int y;

    public TileType Type
    {
        get{ return type;}
        set
        {
            TileType oldType = type;
            type = value;
            //call the callback and let things know we've changed
            if (cbTileTypeChanged != null && oldType!=type)
            {
                cbTileTypeChanged(this);
            }     
        }
    }

    public int X{ get{return x;} }
    public int Y{ get{return y;} }


    //Constructor for the class
    public Tile(World world, int x, int y)
    {
        this.world = world;
        this.x = x;
        this.y = y;

    }

    //register a callback
    public void RegisterTileTypeChangedCallback(Action<Tile> callback)
    {
        cbTileTypeChanged += callback;
    }

    //unregister a callback
    public void UnregisterTileTypeChangedCallback(Action<Tile> callback)
    {
        cbTileTypeChanged -= callback;
    }

    public bool PlaceObject(InstalledObject objInstance)
    {
        if (objInstance == null)
        {
            //we are unisntalling wherever was here
            installedObject = null;
            return true;
        }

        //objInstance isn't null
        if (installedObject != null)
        {
            Debug.LogError("Tring to assign an isntalled object to a tile that already has one.");
            return false;
        }


        installedObject = objInstance;
        return true;

    }


}
