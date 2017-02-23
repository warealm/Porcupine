using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

//Installed objects are things like walls, doors and furniture (eg, a sofa)

public class Furniture : IXmlSerializable
{

    private Dictionary<string, float> furnParameters;


    //These actions are called every update
    private Action<Furniture, float> updateActions;

    //This func has no parameters but enters an Enterability enum
    public Func<Furniture, Enterability> IsEnterable;

    public void Update(float deltaTime)
    {
        if (updateActions != null)
        {
            updateActions(this, deltaTime);
        }
    } 






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

    //Whether or not placing this object could enclose rooms
    public bool roomEnclosure { get; protected set; }

    //For example, a sofa might be 3x2, may only appear to cover 3x1, but extra for legroom.
    int width;
    int height;

    public bool linksToNeighbour { get; protected set; }

    public Action<Furniture> cbOnChanged;
    Func<Tile, bool> funcPositionValidation; //last thing in a func is what is returned

    //TODO Implement larger objects
    //TODO Implement object rotation

    public Furniture()
    {
        //Empty constructor used for serialization
        furnParameters = new Dictionary<string, float>();
    }

    //Copy constructor
    protected Furniture(Furniture other)
    {
        objectType = other.objectType;
        movementCost = other.movementCost;
        roomEnclosure = other.roomEnclosure;
        width = other.width;
        height = other.height;
        linksToNeighbour = other.linksToNeighbour;

        furnParameters = new Dictionary<string, float>(other.furnParameters);

        if (other.updateActions != null)
        {
            updateActions = (Action<Furniture, float>)other.updateActions.Clone();
        }

        IsEnterable = other.IsEnterable;
        
    }

    //Make a copy fo the current furniture

    virtual public Furniture Clone()
    {
        return new Furniture(this);
    }

    //This is used by our object factory to create the prototypical object
    //Note that it doesn't ask for a tile
    //Create furniture from aprameters -- this will probably only be used for prototypes
    public Furniture (string _objectType, float _movementCost =1f, int _width=1, int _height=1,bool _linksToNeighbour = false, bool _roomEnclosure = false)
    {
        objectType = _objectType;
        movementCost = _movementCost;
        roomEnclosure = _roomEnclosure;
        width = _width;
        height = _height;
        linksToNeighbour = _linksToNeighbour;
        funcPositionValidation = DEFAULT_IsValidPosition;

        furnParameters = new Dictionary<string, float>();
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
        //copy constructor
        Furniture obj = proto.Clone(); ;
        obj.tile = tile;


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
            if (t != null && t.furniture != null && t.furniture.cbOnChanged != null && t.furniture.objectType == obj.objectType)
            {
                //we have a northern neighbour, with the same object type as us, so tell it
                //that it has changed by firing its callback
                t.furniture.cbOnChanged(t.furniture); 
            }
            t = tile.world.GetTileAt(x + 1, y);
            if (t != null && t.furniture != null && t.furniture.cbOnChanged != null && t.furniture.objectType == obj.objectType)
            {
                t.furniture.cbOnChanged(t.furniture);
            }
            t = tile.world.GetTileAt(x, y - 1);
            if (t != null && t.furniture != null && t.furniture.cbOnChanged != null && t.furniture.objectType == obj.objectType)
            {
                t.furniture.cbOnChanged(t.furniture);
            }
            t = tile.world.GetTileAt(x - 1, y);
            if (t != null && t.furniture != null && t.furniture.cbOnChanged != null && t.furniture.objectType == obj.objectType)
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

    //this will be replaced by validation cehcks fed to use from
    //LUA files that will be customizable for each pieve of furniture
    private bool DEFAULT_IsValidPosition(Tile t)
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


    //not used?
    public bool _IsValidPosition_Door(Tile t)
    {
        if (DEFAULT_IsValidPosition(t) == false)
        {
            return false;
        }
        //make sure we have a pair of E/W walls or N/S walls

        return true;
    }

    public float GetParameter(string key, float default_value = 0)
    {
        if (furnParameters.ContainsKey(key) == false)
        {
            return default_value;
        }

        return furnParameters[key];
    }

    public void SetParameter(string key, float value)
    {
        furnParameters[key] = value;
    }

    public void ChangeParameter(string key, float value)
    {
        if (furnParameters.ContainsKey(key) == false)
        {
            furnParameters[key] = value;
        }
        else
        {
            furnParameters[key] += value;
        }

    }

    //Register a function that will be called every update

    public void RegisterUpdateAction(Action<Furniture, float> a)
    {
        updateActions += a;
    }

    public void UnRegisterUpdateAction(Action<Furniture, float> a)
    {
        updateActions -= a;
    }

    #region Saving and Loading
    public XmlSchema GetSchema()
    {
        return null;
    }

    public void WriteXml(XmlWriter writer)
    {
        writer.WriteAttributeString("X", tile.X.ToString());
        writer.WriteAttributeString("Y", tile.Y.ToString());
        writer.WriteAttributeString("objectType", objectType );
        //writer.WriteAttributeString("movementCost", movementCost.ToString());

        foreach(string k in furnParameters.Keys)
        {
            writer.WriteStartElement("Param");
            writer.WriteAttributeString("name", k);
            writer.WriteAttributeString("value", furnParameters[k].ToString());


            writer.WriteEndElement();
        }


    }

    public void ReadXml(XmlReader reader)
    {
        //X, Y and objecttype have already been set and we should ahve already been assigned to a tile
        //just read extra data      
        //movementCost = int.Parse( reader.GetAttribute("movementCost") );

        //we are in the furniture element, so read elements until we run out of furniture elements
        if (reader.ReadToDescendant("Param"))
        {
            do
            {
                string key = reader.GetAttribute("name");
                float value = float.Parse(reader.GetAttribute("value"));
                furnParameters[key] = value;
            } while (reader.ReadToNextSibling("Param"));
        }


    }


    #endregion

}
