using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class World {

    //A world will have a two dimensional array of tiles
    Tile[,] tiles;

    //A list of characters
    List<Character> characters;

    //The pathfindinggraph used to navigate our world map
    Path_Tilegraph tileGraph;

    //This is a dictionary which maps a string (like wall) to an installedobject prototype
    public Dictionary<string, Furniture> furniturePrototypes;

    //World will have a width and height
    int width;
    int height;

    //Outside classes can get the width and height of the world by asking for Width and Height
    public int Height{get{return height;}}
    public int Width { get { return width; } }

    //No idea what actions do yet
    Action<Furniture> cbFurnitureCreated;
    Action<Tile> cbTileChanged;
    Action<Character> cbCharacterCreated;

    //TODO: most likely this will be replaced with a dedicated class for managing job queues
    public JobQueue jobQueue;

    //Accessor for the class, will ask for a width and height, if not provided it will default to 100x100
    public World(int _width = 100, int _height = 100)
    {
        jobQueue = new JobQueue();

        width = _width;
        height = _height;
        tiles = new Tile[_width, _height];

        //Loop through the array tiles and add a tile to each element in the array
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                tiles[x, y] = new Tile(this, x, y); //A tile requires a world and the coordinates
                tiles[x, y].RegisterTileTypeChangedCallback(OnTileChanged);
            }
        }

        Debug.Log("world created with " + _width*_height + " Tiles");
        CreateFurniturePrototypes();

        characters = new List<Character>();


    }

    public void Update(float deltaTime)
    {
        foreach(Character c in characters)
        {
            c.Update(deltaTime);
        }
    }

    public Character CreateCharacter(Tile t)
    {
        Character c = new Character(t);
        characters.Add(c);
        if (c != null)
        {
            cbCharacterCreated(c);
        }

        return c;

    }

    //Initialise our dictionary, for now add Wall and the prototype of wall to the dictionary
    void CreateFurniturePrototypes()
    {
        furniturePrototypes = new Dictionary<string, Furniture>();
        furniturePrototypes.Add("Wall", Furniture.CreatePrototype("Wall", 0, 1, 1, true));
    }

    //Go through the tiles array and change the type of each tile to something random
    public void RandomizeTiles()
    {
        Debug.Log("tile randomised");
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (UnityEngine.Random.Range(0, 2) == 0)
                {
                    tiles[x, y].Type = TileType.Empty;
                    
                }
                else
                {
                    tiles[x, y].Type = TileType.Floor;
                }
            }
        }
    }

    //function that returns a tile at the requested coordinate
    public Tile GetTileAt(int x, int y)
    {
        //first check whether the coordinate is actually valid, if not then return
        if (x > Width || x < 0 || y > Height || y < 0)
        {
            Debug.LogError("Tile " + x +","+ y + " is out of range");
            return null;
        }
        return tiles[x,y];
    }

    //Function that places an installed object at the tile requested
    public void PlaceFurniture(string objectType, Tile t)
    {
        
        //ATM this assumes 1x1 tiles

        //check whether the object type current exists in our dictionary, if not then return
        if (furniturePrototypes.ContainsKey(objectType) == false)
        {
            Debug.LogError("installedobjectprototype doesn't contain proto for key " + objectType);
            return;
        }

        //Create an installedObject and place the instance of that object at tile t
        Furniture obj = Furniture.PlaceInstance(furniturePrototypes[objectType], t);

        if (obj == null)
        {
            //failed to place object, most likely there was already something there
            return;
        }

        //Let everyone know that a furniture has been created, so that other functions subscribed to it can run
        if (cbFurnitureCreated != null)
        {
            cbFurnitureCreated(obj);
            InvalidateTileGraph();
        }

    }

    public void RegisterFurnitureCreated(Action<Furniture> callbackfunc)
    {
        cbFurnitureCreated += callbackfunc;
    }

    public void UnregisterFurnitureCreated(Action<Furniture> callbackfunc)
    {
        cbFurnitureCreated -= callbackfunc;
    }

    public void RegisterCharacterCreated(Action<Character> callbackfunc)
    {
        cbCharacterCreated += callbackfunc;
    }

    public void UnregisterCharacterCreated(Action<Character> callbackfunc)
    {
        cbCharacterCreated -= callbackfunc;
    }

    public void RegisterTileChanged(Action<Tile> callbackfunc)
    {
        cbTileChanged += callbackfunc;
    }

    public void UnregisterTileChanged(Action<Tile> callbackfunc)
    {
        cbTileChanged -= callbackfunc;
    }

    void OnTileChanged(Tile t)
    {
        if (cbTileChanged == null)
        {
            return;
        }

        //When the tile is changed, notify other classes subscribed that this event has occured
        cbTileChanged(t);
        InvalidateTileGraph();
    }

    //This should be called whenever a change to the world 
    //means that our old pathfinding info is invalid
    public void InvalidateTileGraph()
    {
        tileGraph = null;

    }

    public bool IsFurniturePlacementValid(string furnitureType, Tile t)
    {
        return furniturePrototypes[furnitureType].IsValidPosition(t);
    }

    public Furniture GetFurniturePrototype(string objectType)
    {
        if (furniturePrototypes.ContainsKey(objectType) == false)
        {
            Debug.LogError("GetFurniturePrototype -- no furniture with type: " + objectType);
            return null;
        }
        return furniturePrototypes[objectType];
    }

    public void SetupPathfindingExample()
    {
        int l = Width / 2 - 5;
        int b = Height / 2 - 5;

        for (int x = l-5; x < l+15; x++)
        {
            for (int y = b-5; y < b+15; y++)
            {
                tiles[x, y].Type = TileType.Floor;

                if(x == l || x == (l+9) || y == b || y == (b + 9))
                {
                    if (x != (l + 9) && y != (b+4))
                    {
                        PlaceFurniture("Wall", tiles[x, y]);
                    }
                }

            }

        }


    }

}
