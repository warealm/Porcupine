using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventorySpriteController : MonoBehaviour {

    public GameObject inventoryUIPrefab;

    Dictionary<Inventory, GameObject> inventoryGameObjectMap;
    Dictionary<string, Sprite> inventorySprites;
    World world { get { return WorldController.Instance.world; } }


    void Start () {

        //Create a world with Empty tiles
        LoadSprites();
        inventoryGameObjectMap = new Dictionary<Inventory, GameObject>();

        //register our callback
        world.RegisterInventoryCreated(OnInventoryCreated);

        //Check for preexisting characters
        foreach(string objectType in world.inventoryManager.inventories.Keys)
        {
            foreach(Inventory inv in world.inventoryManager.inventories[objectType])
            {
                OnInventoryCreated(inv);
            }
            
        }

        //c.SetDestination(world.GetTileAt(world.Width / 2 + 5, world.Height / 2));

    }

    // Update is called once per frame
    void Update () {
		
	}

    public void OnInventoryCreated(Inventory inv)
    {
        //create a visual GO linked to this data
        Debug.Log("CREATED");
        //does not consider multi-tile objects nor ortated objects

        GameObject inv_go = new GameObject();
        //add our tile/gameobject pair to the dictionary.
        inventoryGameObjectMap.Add(inv, inv_go);
        //group the objects in the worldcontroller
        inv_go.transform.SetParent(this.transform, true);
        inv_go.name = inv.inventoryType;
        inv_go.transform.position = new Vector3(inv.tile.X, inv.tile.Y, 0);

        //add a sprite renderer, but don't bother setting a sprite because all the tiles are empty atm
        inv_go.AddComponent<SpriteRenderer>().sprite = inventorySprites[inv.inventoryType];
        inv_go.GetComponent<SpriteRenderer>().sortingLayerName = "Inventory";
        //register our callback so that our GO gets updated whenever tiletype changes
        //inv.RegisterOnChangedCallBack(OnCharacterChanged);

        if(inv.maxStackSize > 1)
        {
            //this is a stackable object, s lets add a Inventory UI component
            GameObject ui_go = Instantiate(inventoryUIPrefab);
            ui_go.transform.SetParent(inv_go.transform);
            ui_go.transform.localPosition = Vector3.zero;
            ui_go.GetComponentInChildren<Text>().text = inv.stackSize.ToString();
        }




    }

    void OnInventoryChanged(Inventory inv)
    {

        //Make sure the furnitures graphics are correct
        if (inventoryGameObjectMap.ContainsKey(inv) == false)
        {
            Debug.LogError("OnFurnitureChanged -- Trying to change visuals for char not in our map");
            return;
        }
        GameObject char_go = inventoryGameObjectMap[inv];
        char_go.transform.position = new Vector3(inv.tile.X, inv.tile.Y, 0);
        
    }

    void LoadSprites()
    {
        inventorySprites = new Dictionary<string, Sprite>();
        Sprite[] sprites = Resources.LoadAll<Sprite>("Images/Characters/");

        foreach (Sprite s in sprites)
        {
            //Debug.Log(s);
            inventorySprites[s.name] = s;
        }

    }
}
