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

        //Go through any EXISTING furniture (i.e. from a save that was loaded on OnEnable) and call the OnCreated ebent manually?
        foreach(Furniture furn in world.furnitures)
        {
            OnFurnitureCreated(furn);
        }
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


        //This hardcoding is not ideal
        if (furn.objectType == "Door")
        {
            //By default the door graphic is meant for walls to the east and west
            //check to see if we actually have a wall north/south, and it so then rotate this GO by 90 degrees
            Tile northTile = world.GetTileAt(furn.tile.X, furn.tile.Y + 1);
            Tile southTile = world.GetTileAt(furn.tile.X, furn.tile.Y - 1);

            if (northTile != null && southTile != null && northTile.furniture != null && southTile.furniture != null && northTile.furniture.objectType == "Wall" && southTile.furniture.objectType == "Wall")
            {
                furn_go.transform.rotation = Quaternion.Euler(0, 0, 90);
            }

        }

        //add a sprite renderer, but don't bother setting a sprite because all the tiles are empty atm
        furn_go.AddComponent<SpriteRenderer>().sprite = GetSpriteForFurniture(furn);
        furn_go.GetComponent<SpriteRenderer>().sortingLayerName = "Furniture";
        //register our callback so that OnFurnitureChanged is run whenever tiletype changes
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

    public Sprite GetSpriteForFurniture(Furniture furn)
    {
        string spriteName = furn.objectType;
        if (furn.linksToNeighbour == false)
        {
            //check for openness and update the sprite
            if (furn.objectType == "Door")
            {
                if (furn.GetParameter("openness") < 0.1f)
                {
                    spriteName = "Door";
                }
                else if (furn.GetParameter("openness") < 0.5f)
                {
                    spriteName = "Door_openness_1";
                }
                else if (furn.GetParameter("openness") < 0.9f)
                {
                    spriteName = "Door_openness_2";
                }
                else
                {
                    spriteName = "Door_openness_3";
                }
            }

            return furnitureSprites[spriteName];
        }

        //otherwise the sprite name is more complicated
        spriteName = furn.objectType + "_";
        int x = furn.tile.X;
        int y = furn.tile.Y;

        //check for neighbours North, East, South, West
        Tile t;
        t = world.GetTileAt(x, y + 1);
        if (t != null && t.furniture!=null && t.furniture.objectType == furn.objectType)
        {
            spriteName += "N";
        }
        t = world.GetTileAt(x+1, y);
        if (t != null && t.furniture != null && t.furniture.objectType == furn.objectType)
        {
            spriteName += "E";
        }
        t = world.GetTileAt(x, y -1);
        if (t != null && t.furniture != null && t.furniture.objectType == furn.objectType)
        {
            spriteName += "S";
        }
        t = world.GetTileAt(x-1, y);
        if (t != null && t.furniture != null && t.furniture.objectType == furn.objectType)
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
