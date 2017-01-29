using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


//Installed objects are things like walls, doors and furniture (eg, a sofa)

public class Furniture
{
    //This represents the base tile of the object, in practice large objects may occupy 
    //multiple tiles.
    public Tile tile{get; protected set;}          

    //This objectType will be queried by the visual system to know what sprite to render for this object
    public string objectType{get; protected set;}

    //This is a multiplier so a value of 2 here means you at half speed
    //Tile types and other environmental effects may further act as a multiplier
    //For example, a rough tile with a cost of 2, and a table with a cost of 3
    //would have a total movement cost of (2+3=5)
    //So you'd move through this tile at 1/8th normal speed
    //Special: If cost is 0, then the tile is impassible (like a wall).
    public float movementCost { get; protected set; }

    //For example, a sofa might be 3x2, may only appear to cover 3x1, but extra for legroom.
    int width;
    int height;

    public bool linksToNeighbour { get; protected set; }

    Action<Furniture> cbOnChanged;
    Func<Tile, bool> funcPositionValidation; //last thing in a func is what is returned

    //TODO Implement larger objects
    //TODO Implement object rotation

    protected Furniture()
    {

    }

    //This is used by our object factory to create the prototypical object
    //Note that it doesn't ask for a tile
    //static because we don't always want to create an instance of the class to create a prototype
    static public Furniture CreatePrototype(string _objectType, float _movementCost =1f, int _width=1, int _height=1,bool _linksToNeighbour = false)
    {
        Furniture obj = new Furniture();
        obj.objectType = _objectType;
        obj.movementCost = _movementCost;
        obj.width = _width;
        obj.height = _height;
        obj.linksToNeighbour = _linksToNeighbour;

        obj.funcPositionValidation = obj._IsValidPosition;
        return obj;
    }

    //We can install the object proto on tile
    static public Furniture PlaceInstance( Furniture proto, Tile tile)
    {
        if (proto.funcPositionValidation(tile) == false)
        {
            Debug.LogError("PlaceInstance -- Position Validity function returned false");
            return null;
        }

        //we know our placement destination is valid
        Furniture obj = new Furniture();
        obj.objectType = proto.objectType;
        obj.movementCost =proto.movementCost;
        obj.width = proto.width;
        obj.height = proto.height;
        obj.tile = tile;
        obj.linksToNeighbour = proto.linksToNeighbour;

        //This assumes we are 1x1;
        if (tile.PlaceFurniture(obj) == false)
        {
            //For some reason, we weren't be to place our object in this tile, 
            //it was probably already occupied
            //do not return our newly instantiated object, instead it will be garbage collected.
            return null;
        }

        if (obj.linksToNeighbour)
        {
            //This type of object links itself to its neighbours, so we should inform neighbours that they have a new buddy
            //just trigger their onChangedCallback

            //check for neighbours North, East, South, West
            Tile t;
            int x = tile.X;
            int y = tile.Y;

            t = tile.world.GetTileAt(x,y+1);
            if (t != null && t.furniture != null && t.furniture.objectType == obj.objectType)
            {
                //we have a northern neighbour, with the same object type as us, so tell it
                //that it has changed by firing its callback
                t.furniture.cbOnChanged(t.furniture); 
            }
            t = tile.world.GetTileAt(x + 1, y);
            if (t != null && t.furniture != null && t.furniture.objectType == obj.objectType)
            {
                t.furniture.cbOnChanged(t.furniture);
            }
            t = tile.world.GetTileAt(x, y - 1);
            if (t != null && t.furniture != null && t.furniture.objectType == obj.objectType)
            {
                t.furniture.cbOnChanged(t.furniture);
            }
            t = tile.world.GetTileAt(x - 1, y);
            if (t != null && t.furniture != null && t.furniture.objectType == obj.objectType)
            {
                t.furniture.cbOnChanged(t.furniture);
            }


        }
        

        return obj;
    }

    public void RegisterOnChangedCallback(Action<Furniture> callbackFunc)
    {
        cbOnChanged += callbackFunc;
    }

    public void UnregisterOnChangedCallback(Action<Furniture> callbackFunc)
    {
        cbOnChanged -= callbackFunc;
    }

    public bool IsValidPosition(Tile t)
    {
        return funcPositionValidation(t);
    }

    public bool _IsValidPosition(Tile t)
    {
        //make sure tile is floor

        if (t.Type != TileType.Floor)
        {
            return false;
        }
        //make sure tile doesn't already have furniture
        if (t.furniture != null)
        {
            return false;
        }

        return true;
    }

    public bool _IsValidPosition_Door(Tile t)
    {
        if (_IsValidPosition(t) == false)
        {
            return false;
        }
        //make sure we have a pair of E/W walls or N/S walls

        return true;
    }

}
