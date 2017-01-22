using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


//Installed objects are things like walls, doors and furniture (eg, a sofa)

public class InstalledObject
{
    //This represents the base tile of the object, in practice large objects may occupy 
    //multiple tiles.
    public Tile tile
    {
        get; protected set;
    }          

    //This objectType will be queried by the visual system to know what sprite to render for this object
    public string objectType
    {
        get; protected set;
    }

    //This is a multiplier so a value of 2 here means you at half speed
    //Tile types and other environmental effects may further act as a multiplier
    //For example, a rough tile with a cost of 2, and a table with a cost of 3
    //would have a total movement cost of (2+3=5)
    //So you'd move through this tile at 1/8th normal speed
    //Special: If cost is 0, then the tile is impassible (like a wall).
    float movementCost;

    //For example, a sofa might be 3x2, may only appear to cover 3x1, but extra for legroom.
    int width;
    int height;

    Action<InstalledObject> cbOnChanged;

    //TODO Implement larger objects
    //TODO Implement object rotation

    protected InstalledObject()
    {

    }

    //This is used by our object factory to create the prototypical object
    //Note that it doesn't ask for a tile
    static public InstalledObject CreatePrototype(string objectType, float movementCost =1f, int width=1, int height=1)
    {
        InstalledObject obj = new InstalledObject();
        obj.objectType = objectType;
        obj.movementCost = movementCost;
        obj.width = width;
        obj.height = height;

        return obj;

    }

    static public InstalledObject PlaceInstance( InstalledObject proto, Tile tile)
    {
        InstalledObject obj = new InstalledObject();

        obj.objectType = proto.objectType;
        obj.movementCost =proto.movementCost;
        obj.width = proto.width;
        obj.height = proto.height;

        obj.tile = tile;

        //This assumes we are 1x1;
        if (tile.PlaceObject(obj) == false)
        {
            //For some reason, we weren't be to place our object in this tile, it was probablyt already occupied
            //do not return our newly instantiated object, instead it will be garbage collected.
            return null;
        }
        //tile.PlaceObject(proto);

        return obj;
    }

    public void RegisterOnChangedCallback(Action<InstalledObject> callbackFunc)
    {
        cbOnChanged += callbackFunc;
    }

    public void UnregisterOnChangedCallback(Action<InstalledObject> callbackFunc)
    {
        cbOnChanged -= callbackFunc;
    }


}
