using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSpriteController : MonoBehaviour {

    Dictionary<Character, GameObject> characterGameObjectMap;
    Dictionary<string, Sprite> characterSprites;
    World world { get { return WorldController.Instance.world; } }


    void Start () {

        //Create a world with Empty tiles
        LoadSprites();
        characterGameObjectMap = new Dictionary<Character, GameObject>();

        //register our callback
        world.RegisterCharacterCreated(OnCharacterCreated);

        //Check for preexisting characters
        foreach(Character c in world.characters)
        {
            OnCharacterCreated(c);
        }

        //c.SetDestination(world.GetTileAt(world.Width / 2 + 5, world.Height / 2));

    }

    // Update is called once per frame
    void Update () {
		
	}

    public void OnCharacterCreated(Character character)
    {
        //create a visual GO linked to this data
        Debug.Log("CREATED");
        //does not consider multi-tile objects nor ortated objects

        GameObject char_go = new GameObject();
        //add our tile/gameobject pair to the dictionary.
        characterGameObjectMap.Add(character, char_go);
        //group the objects in the worldcontroller
        char_go.transform.SetParent(this.transform, true);
        char_go.name = "Character";
        char_go.transform.position = new Vector3(character.X, character.Y, 0);

        //add a sprite renderer, but don't bother setting a sprite because all the tiles are empty atm
        char_go.AddComponent<SpriteRenderer>().sprite = characterSprites["p1_front"];
        char_go.GetComponent<SpriteRenderer>().sortingLayerName = "Character";
        //register our callback so that our GO gets updated whenever tiletype changes
        character.RegisterOnChangedCallBack(OnCharacterChanged);

    }

    void OnCharacterChanged(Character character)
    {

        //Make sure the furnitures graphics are correct
        if (characterGameObjectMap.ContainsKey(character) == false)
        {
            Debug.LogError("OnFurnitureChanged -- Trying to change visuals for char not in our map");
            return;
        }
        GameObject char_go = characterGameObjectMap[character];
        char_go.transform.position = new Vector3(character.X, character.Y, 0);
        
    }

    void LoadSprites()
    {
        characterSprites = new Dictionary<string, Sprite>();
        Sprite[] sprites = Resources.LoadAll<Sprite>("Images/Characters/");

        foreach (Sprite s in sprites)
        {
            //Debug.Log(s);
            characterSprites[s.name] = s;
        }

    }
}
