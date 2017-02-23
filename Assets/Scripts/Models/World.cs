using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

public class World : IXmlSerializable {

    //A world will have a two dimensional array of tiles
    Tile[,] tiles;

    //A list of rooms in our world
    public List<Room> rooms;
    List<Inventory> inventories;
    public InventoryManager inventoryManager;

    //A list of characters
    public List<Character> characters;
    public List<Furniture> furnitures;

    //The pathfindinggraph used to navigate our world map
    public Path_Tilegraph tileGraph;

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
    Action<Inventory> cbInventoryCreated;

    //TODO: most likely this will be replaced with a dedicated class for managing job queues
    public JobQueue jobQueue;

    //Accessor for the class, will ask for a width and height, if not provided it will default to 100x100
    public World(int _width, int _height)
    {
        //Creates an empty world
        SetupWorld(_width, _height);

        //make one character
        Character c = CreateCharacter(GetTileAt(Width / 2, Height / 2));


    }

    public Room GetOutsideRoom()
    {
        return rooms[0];
    }

    public void AddRoom(Room r)
    {
        rooms.Add(r);
    }



    public void DeleteRoom(Room r)
    {
        if (r == GetOutsideRoom())
        {
            Debug.LogError("Tried to delete the outside room");
        }

        //remove this room from our rooms list
        rooms.Remove(r);

        //All tiles that belonged to this room should be re-assigned to the outside
        r.UnAssignAllTiles();
        
    }

    void SetupWorld(int _width, int _height)
    {
        jobQueue = new JobQueue();

        width = _width;
        height = _height;
        tiles = new Tile[_width, _height];

        rooms = new List<Room>();
        rooms.Add(new Room());  //This is the outside space

        //Loop through the array tiles and add a tile to each element in the array
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                tiles[x, y] = new Tile(this, x, y); //A tile requires a world and the coordinates
                tiles[x, y].RegisterTileTypeChangedCallback(OnTileChanged);
                tiles[x, y].room = GetOutsideRoom();        //rooms 0 is always going to be outside, and that is our default room.
            }
        }

        Debug.Log("world created with " + _width * _height + " Tiles");
        CreateFurniturePrototypes();

        characters = new List<Character>();
        furnitures = new List<Furniture>();
        inventoryManager = new InventoryManager();

    }

    public void Update(float deltaTime)
    {
        foreach(Character c in characters)
        {
            c.Update(deltaTime);
        }

        foreach(Furniture f in furnitures)
        {
            f.Update(deltaTime);
        }

    }

    public Character CreateCharacter(Tile t)
    {
        Character c = new Character(t);
        characters.Add(c);
        if (cbCharacterCreated != null)
        {
            cbCharacterCreated(c);
        }

        return c;

    }

    //Initialise our dictionary, and add the prototypes to the dictionary
    void CreateFurniturePrototypes()
    {
        furniturePrototypes = new Dictionary<string, Furniture>();
        furniturePrototypes.Add("Wall", new Furniture("Wall", 0, 1, 1, true, true));
        furniturePrototypes.Add("Door", new Furniture("Door", 1, 1, 1, false, true));

        furniturePrototypes["Door"].SetParameter("openness", 0);
        furniturePrototypes["Door"].SetParameter("is_opening", 0);
        furniturePrototypes["Door"].RegisterUpdateAction(FurnitureActions.Door_UpdateAction);
        furniturePrototypes["Door"].IsEnterable = FurnitureActions.Door_IsEnterable;    //+=?

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
        if (x >= Width || x < 0 || y >= Height || y < 0)
        {
            //Debug.LogError("Tile " + x +","+ y + " is out of range");
            return null;
        }
        return tiles[x,y];
    }

    //Function that places an installed object at the tile requested
    public Furniture PlaceFurniture(string objectType, Tile t)
    {
        
        //ATM this assumes 1x1 tiles

        //check whether the object type current exists in our dictionary, if not then return
        if (furniturePrototypes.ContainsKey(objectType) == false)
        {
            Debug.LogError("installedobjectprototype doesn't contain proto for key " + objectType);
            return null;
        }

        //Create an installedObject and place the instance of that object at tile t
        Furniture furn = Furniture.PlaceInstance(furniturePrototypes[objectType], t);

        if (furn == null)
        {
            //failed to place object, most likely there was already something there
            return null;
        }

        furnitures.Add(furn);

        //Do we need to recalculate our rooms?
        if (furn.roomEnclosure)
        {
            //recalculate room floodfill method
            Room.DoRoomFloodFill(furn);

        }

        //Let everyone know that a furniture has been created, so that other functions subscribed to it can run
        if (cbFurnitureCreated != null)
        {
            cbFurnitureCreated(furn);

            if (furn.movementCost != 1)
            {
                //since tiles return movement cost as their base cost multiplied by
                //the furniture's movement cost, a furniture movement cost of exactly 1 doesn't impact our pathfinding system
                //so we can ocassionally avoid invalidating pathfinding graphs
                InvalidateTileGraph();          //reset the pathfinding system
            }
            
        }

        return furn;

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

    public void RegisterInventoryCreated(Action<Inventory> callbackfunc)
    {
        cbInventoryCreated += callbackfunc;
    }

    public void UnregisterInventoryCreated(Action<Inventory> callbackfunc)
    {
        cbInventoryCreated -= callbackfunc;
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




    #region Saving and Loading

    public World()
    {

    }

    public XmlSchema GetSchema()
    {
        return null;
    }

    public void WriteXml(XmlWriter writer)
    {
        //Save info here
        writer.WriteAttributeString("Width", Width.ToString());
        writer.WriteAttributeString("Height", Height.ToString());

        writer.WriteStartElement("Tiles");
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if(tiles[x,y].Type != TileType.Empty)
                {
                    writer.WriteStartElement("Tile");
                    tiles[x, y].WriteXml(writer);
                    writer.WriteEndElement();
                }           
            }
        }
        writer.WriteEndElement();

        writer.WriteStartElement("Furnitures");
        foreach(Furniture furn in furnitures)
        {
            writer.WriteStartElement("Furniture");
            furn.WriteXml(writer);
            writer.WriteEndElement();
        }
        
        writer.WriteEndElement();

        writer.WriteStartElement("Characters");
        foreach (Character c in characters)
        {
            writer.WriteStartElement("Character");
            c.WriteXml(writer);
            writer.WriteEndElement();
        }

        writer.WriteEndElement();
    }

    public void ReadXml(XmlReader reader)
    {
        //Load info here
        Debug.Log("World::ReadXML");

        width = int.Parse(reader.GetAttribute("Width"));
        height = int.Parse(reader.GetAttribute("Height"));

        SetupWorld(width, height);

        while (reader.Read())
        {
            switch (reader.Name)
            {
                case "Tiles":
                    ReadXml_Tiles(reader);
                    break;
                case "Furnitures":
                    ReadXml_Furnitures(reader);
                    break;
                case "Characters":
                    ReadXml_Characters(reader);
                    break;
            }
        }

        Inventory inv = new Inventory();
        inv.stackSize = 10;
        Tile t = GetTileAt(Width / 2, Height / 2);
        inventoryManager.PlaceInventory(t, inv);
        if (cbInventoryCreated != null)
        {
            cbInventoryCreated(t.inventory);
        }

        inv = new Inventory();
        inv.stackSize = 18;
        t = GetTileAt(Width / 2 + 2, Height / 2);
        inventoryManager.PlaceInventory(t, inv);
        if (cbInventoryCreated != null)
        {
            cbInventoryCreated(t.inventory);
        }

        inv = new Inventory();
        inv.stackSize = 45;
        t = GetTileAt(Width / 2 + 1, Height / 2 + 2);
        inventoryManager.PlaceInventory(t, inv);
        if (cbInventoryCreated != null)
        {
            cbInventoryCreated(t.inventory);
        }

    }

    void ReadXml_Tiles(XmlReader reader)
    {
        //we are in the tiles element, so read elements until we run out of tile elements

        if (reader.ReadToDescendant("Tile"))
        {
            do
            {
                //we have at least one tile, so do something with it
                int x = int.Parse(reader.GetAttribute("X"));
                int y = int.Parse(reader.GetAttribute("Y"));
                tiles[x, y].ReadXml(reader);
            } while (reader.ReadToNextSibling("Tile"));     
        }
    }

    void ReadXml_Furnitures(XmlReader reader)
    {
        //we are in the furnitures element, so read elements until we run out of furnitures elements
        if (reader.ReadToDescendant("Furniture"))
        {
            do
            {
                //we have at least one tile, so do something with it
                int x = int.Parse(reader.GetAttribute("X"));
                int y = int.Parse(reader.GetAttribute("Y"));
                Furniture furn = PlaceFurniture(reader.GetAttribute("objectType"), tiles[x, y]);
                furn.ReadXml(reader);
            } while (reader.ReadToNextSibling("Furniture"));
        }
    }

    void ReadXml_Characters(XmlReader reader)
    {
        //we are in the character element, so read elements until we run out of character elements
        if (reader.ReadToDescendant("Character"))
        {
            do
            {
                //we have at least one tile, so do something with it
                int x = int.Parse(reader.GetAttribute("X"));
                int y = int.Parse(reader.GetAttribute("Y"));
                Character c = CreateCharacter(tiles[x, y]);
                c.ReadXml(reader);
            } while (reader.ReadToNextSibling("Character"));
        }
    }
    #endregion

}
