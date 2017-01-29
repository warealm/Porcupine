using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class FurnitureSpriteController : MonoBehaviour {

    Dictionary<Furniture, GameObject> furnitureGameObjectMap;
    Dictionary<String, Sprite> furnitureSprites;
    World world { get { return WorldController.Instance.world; } }

	// Use this for initialization
	void Start ()
    {
        //Create a world with Empty tiles
        LoadSprites();
        furnitureGameObjectMap = new Dictionary<Furniture, GameObject>();

        //register our callback so that our GO gets updated whenever tile's data changes
        world.RegisterFurnitureCreated(OnFurnitureCreated);

    }

    public void OnFurnitureCreated(Furniture furn)
    {
        //create a visual GO linked to this data

        //does not consider multi-tile objects nor ortated objects

        GameObject furn_go = new GameObject();
        //add our tile/gameobject pair to the dictionary.
        furnitureGameObjectMap.Add(furn, furn_go);
        //group the objects in the worldcontroller
        furn_go.transform.SetParent(this.transform, true);
        furn_go.name = furn.objectType + "_" + furn.tile.X + "_" + furn.tile.Y;
        furn_go.transform.position = new Vector3(furn.tile.X, furn.tile.Y, 0);

        //add a sprite renderer, but don't bother setting a sprite because all the tiles are empty atm
        furn_go.AddComponent<SpriteRenderer>().sprite = GetSpriteForFurniture(furn);
        furn_go.GetComponent<SpriteRenderer>().sortingLayerName = "Furniture";
        //register our callback so that our GO gets updated whenever tiletype changes
        furn.RegisterOnChangedCallback(OnFurnitureChanged);

    }

    void OnFurnitureChanged(Furniture furn)
    {

        //Make sure the furnitures graphics are correct
        if (furnitureGameObjectMap.ContainsKey(furn) == false)
        {
            Debug.LogError("OnFurnitureChanged -- Trying to change visuals for furniture not in our map");
            return;
        }
        GameObject furn_go = furnitureGameObjectMap[furn];
        furn_go.GetComponent<SpriteRenderer>().sprite = GetSpriteForFurniture(furn);
        furn_go.GetComponent<SpriteRenderer>().sortingLayerName = "Furniture";
    }

    public Sprite GetSpriteForFurniture(Furniture _obj)
    {
        if (_obj.linksToNeighbour == false)
        {
            return furnitureSprites[_obj.objectType];
        }

        //otherwise the sprite name is more complicated
        string spriteName = _obj.objectType + "_";
        int x = _obj.tile.X;
        int y = _obj.tile.Y;

        //check for neighbours North, East, South, West
        Tile t;
        t = world.GetTileAt(x, y + 1);
        if (t != null && t.furniture!=null && t.furniture.objectType == _obj.objectType)
        {
            spriteName += "N";
        }
        t = world.GetTileAt(x+1, y);
        if (t != null && t.furniture != null && t.furniture.objectType == _obj.objectType)
        {
            spriteName += "E";
        }
        t = world.GetTileAt(x, y -1);
        if (t != null && t.furniture != null && t.furniture.objectType == _obj.objectType)
        {
            spriteName += "S";
        }
        t = world.GetTileAt(x-1, y);
        if (t != null && t.furniture != null && t.furniture.objectType == _obj.objectType)
        {
            spriteName += "W";
        }

        //Eg, if object has all four corner sthen it will be objectname_NESW
        if(furnitureSprites.ContainsKey(spriteName) == false)
        {
            Debug.LogError("GetSpriteForInstalledObject -- No sprites with name:" + spriteName);
            return null;
        }
        return furnitureSprites[spriteName];
    }

    void LoadSprites()
    {
        furnitureSprites = new Dictionary<string, Sprite>();
        Sprite[] sprites = Resources.LoadAll<Sprite>("Images/Furniture/");

        foreach (Sprite s in sprites)
        {
            //Debug.Log(s);
            furnitureSprites[s.name] = s;
        }

    }

    public Sprite GetSpriteForFurnitureO(string objectType)
    {
        if (furnitureSprites.ContainsKey(objectType))
        {
            return furnitureSprites[objectType];
        }

        if (furnitureSprites.ContainsKey(objectType+"_"))
        {
            return furnitureSprites[objectType+"_"];
        }

        Debug.LogError("GetSpriteForFurnitureO -- No Sprites with name: " + objectType);
        return null;
    }
}
