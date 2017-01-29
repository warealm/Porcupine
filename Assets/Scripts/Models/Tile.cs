using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//Tiles can either be empty or have a floor
public enum TileType { Empty, Floor };

public class Tile {


    //Default to an empty tile type
    private TileType type = TileType.Empty;

    //The function we callback any time our tile's data changes
    Action<Tile> cbTileChanged;

    //tile either either have a looseobject or an installed object on top of the base tile type
    private Inventory inventory;
    public Furniture furniture { get; protected set; }

    public Job pendingFurnitureJob;


    //Tile should know which world it's in
    public World world { get; protected set; }
    //Tile should know the co-ordinates of the world
    private int x;
    private int y;

    //Getter and setter for the tiletype
    public TileType Type
    {
        get{ return type;}
        set
        {
            TileType oldType = type;
            type = value;
            //call the callback everytime the tiletype is changed and let things know we've changed
            if (cbTileChanged != null && oldType!=type)
            {
                cbTileChanged(this);

            }     
        }
    }

    //getter for the coordinates of the tile, its called X and Y for the outside classes calling this
    //but its actually called x and y in the class
    public int X{ get{return x;} }
    public int Y{ get{return y;} }

    public float movementCost
    {
        get
        {
            if(Type == TileType.Empty)
            {
                return 0;
            }

            if (furniture == null)
            {
                return 1;
            }

            return 1 * furniture.movementCost;

        }
    }



    //Constructor for the class, state the world they're in and the coordinates
    public Tile(World world, int x, int y)
    {
        this.world = world;
        this.x = x;
        this.y = y;

    }

    //register a callback, if cbTileTypeChanged is called then so is the callback action
    public void RegisterTileTypeChangedCallback(Action<Tile> callback)
    {
        cbTileChanged += callback;
    }

    //unregister a callback
    public void UnregisterTileTypeChangedCallback(Action<Tile> callback)
    {
        cbTileChanged -= callback;
    }

    //function to determine whether or not we have placed an installed object on the tile
    public bool PlaceFurniture(Furniture objInstance)
    {

        //if the thing we are trying to install is null(destroy?), then we don't want to install anything
        if (objInstance == null)
        {
            //we are uninstalling wherever was here
            furniture = null;
            //return true because we have succesfully installed a tile
            return true;
        }

        //objInstance isn't null, only install if the current tile doesn't already have an installed object
        //return false, saying we haven't installed an object in the tile
        if (furniture != null)
        {
            Debug.LogError("Trying to assign an furniture to a tile that already has one.");
            return false;
        }

        //if the objInstance isn't a null, and if we don't currently have an installed object on the tile
        //then make the installed object on the tile equal to the objInstance and return true
        furniture = objInstance;
        return true;

    }

    //Tells us if two tiles are adjacent
    public bool IsNeighbour(Tile tile, bool diagOkay = false)
    {
        if((this.X == tile.X) && (Mathf.Abs(this.Y - tile.Y) == -1))
        {
            return true;
        }

        if ((this.Y == tile.Y) && (Mathf.Abs(this.X - tile.X) == -1))
        {
            return true;
        }

        if (diagOkay)
        {
            if ((this.X == tile.X + 1) && ((this.Y == tile.Y + 1) || (this.Y == tile.Y - 1)))
            {
                return true;
            }
            if ((this.X == tile.X - 1) && ((this.Y == tile.Y - 1) || (this.Y == tile.Y + 1)))
            {
                return true;
            }
        }
        return false;
    }

    public Tile[] getNeighbours(bool diagokay = false)
    {

        Tile[] ns;


        if(diagokay == false)
        {
            ns = new Tile[4];       //Tile order is NESW
        }
        else
        {
            ns = new Tile[8];   //Tile order is NESW NE SE SW NW
        }

        Tile n;
        n = world.GetTileAt(X, Y + 1);
        ns[0] = n;
        n = world.GetTileAt(X+1, Y);
        ns[1] = n;
        n = world.GetTileAt(X, Y - 1);
        ns[2] = n;
        n = world.GetTileAt(X-1, Y);
        ns[3] = n;

        if(diagokay == true)
        {
            n = world.GetTileAt(X + 1, Y + 1);
            ns[4] = n;
            n = world.GetTileAt(X + 1, Y - 1);
            ns[5] = n;
            n = world.GetTileAt(X - 1, Y - 1);
            ns[6] = n;
            n = world.GetTileAt(X - 1, Y + 1);
            ns[7] = n;
        }

        return ns;

    }






}
