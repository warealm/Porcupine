using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Xml.Serialization;
using System.IO;

public class WorldController : MonoBehaviour {


    public static WorldController Instance { get; protected set; }      //create an instance so that we can access the worldcontroller
    public World world { get; protected set; }      //create a world, has getter property but cannot be set outside
    static bool loadWorld = false;

	// Use this for initialization
	void OnEnable ()
    {
        if (Instance != null)
        {
            Debug.LogError("there should not be two world controllers");
        }
        Instance = this;

        if (loadWorld)
        {
            loadWorld = false;
            CreateWorldFromSaveFile();
        }
        else
        {
            CreateEmptyWorld();
        }

    }

    private void Update()
    {
        world.Update(Time.deltaTime);
    }


    public Tile GetTileAtWorldCoord(Vector3 coord)
    {
        int x = Mathf.RoundToInt(coord.x);
        int y = Mathf.RoundToInt(coord.y);

        return world.GetTileAt(x, y);
    }



    public void NewWorld()
    {
        Debug.Log("Createing new world...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void SaveWorld()
    {
        Debug.Log("Saving world...");

        XmlSerializer serializer = new XmlSerializer(typeof(World));
        TextWriter writer = new StringWriter();
        serializer.Serialize(writer, world);
        writer.Close();

        //Debug.Log(writer.ToString());

        PlayerPrefs.SetString("SaveGame00", writer.ToString());
    }

    public void LoadWorld()
    {
        Debug.Log("Load World...");

        //Reload the scene to reset all data and purge all references
        loadWorld = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

    }

    void CreateEmptyWorld()
    {
        //Create a world with Empty tiles
        world = new World(100, 100);

        //Center the camera
        Camera.main.transform.position = new Vector3(world.Width / 2, world.Height / 2, Camera.main.transform.position.z);
    }

    void CreateWorldFromSaveFile()
    {
        Debug.Log("LOADED SAVED WORLD");

        //Create the world from our save file data
        XmlSerializer serializer = new XmlSerializer(typeof(World));
        TextReader reader = new StringReader(PlayerPrefs.GetString("SaveGame00"));
        Debug.Log(reader.ToString());
        world = (World)serializer.Deserialize(reader);
        reader.Close();

        //Center the camera
        Camera.main.transform.position = new Vector3(world.Width / 2, world.Height / 2, Camera.main.transform.position.z);

    }


}
