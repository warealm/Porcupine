using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseController : MonoBehaviour {

    public GameObject circleCursorPrefab;

    bool buildModeIsObjects = false;

    Vector3 lastFramePosition;
    Vector3 currFramePosition;
    Vector3 dragStartPosition;
    TileType buildModeTile= TileType.Floor;
    string buildModeObjectType;

    List<GameObject> dragPreviewGameObjects;

	// Use this for initialization
	void Start ()
    {
        dragPreviewGameObjects = new List<GameObject>();

	}
	
	void Update ()
    {

        currFramePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        currFramePosition.z = 0;

        //UpdateCursor();                 //Update the circle cursor
        UpdateDragging();               //Handle left mouse clicks
        UpdateCameraMovement();         //Handle Screen dragging


        lastFramePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        lastFramePosition.z = 0;

		
	}

    //void UpdateCursor()
    //{
    //    //Update the circle cursor position
    //    Tile tileUnderMouse = WorldController.Instance.GetTileAtWorldCoord(currFramePosition);
    //    if (tileUnderMouse != null)
    //    {
    //        circleCursor.SetActive(true);
    //        Vector3 cursorPosition = new Vector3(tileUnderMouse.X, tileUnderMouse.Y, 0);
    //        circleCursor.transform.position = cursorPosition;
    //    }
    //    else
    //    {
    //        circleCursor.SetActive(false);
    //    }

    //}

    void UpdateDragging()
    {
        //If we are over a UI element, then bail out
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }


        //start drag
        if (Input.GetMouseButtonDown(0))
        {
            dragStartPosition = currFramePosition;
        }

        int start_x = Mathf.RoundToInt(dragStartPosition.x);
        int end_x = Mathf.RoundToInt(currFramePosition.x);

        if (end_x < start_x)
        {
            int tmp = end_x;
            end_x = start_x;
            start_x = tmp;
        }

        int start_y = Mathf.RoundToInt(dragStartPosition.y);
        int end_y = Mathf.RoundToInt(currFramePosition.y);

        if (end_y < start_y)
        {
            int tmp2 = end_y;
            end_y = start_y;
            start_y = tmp2;
        }

        //clean up old drag previews
        while (dragPreviewGameObjects.Count!=0)
        {
            GameObject go = dragPreviewGameObjects[0];
            dragPreviewGameObjects.RemoveAt(0);
            SimplePool.Despawn(go);
        }


        if (Input.GetMouseButton(0))
        {
            for (int x = start_x; x <= end_x; x++)
            {
                for (int y = start_y; y <= end_y; y++)
                {
                    Tile t = WorldController.Instance.World.GetTileAt(x, y);
                    if (t != null)
                    {
                        //Display the building hint on top of the tile
                        GameObject go = SimplePool.Spawn(circleCursorPrefab, new Vector3(x, y, 0), Quaternion.identity);
                        dragPreviewGameObjects.Add(go);
                        go.transform.SetParent(this.transform, true);
                    }
                }
            }

        }

        //end drag
        if (Input.GetMouseButtonUp(0))
        {

            for (int x = start_x; x <= end_x; x++)
            {
                for (int y = start_y; y <= end_y; y++)
                {
                    Tile t = WorldController.Instance.World.GetTileAt(x, y);



                    if (t != null)
                    {
                        if (buildModeIsObjects == true)
                        {
                            // create the installed object and assign it to the tile
                            //Right now we're just going to assume walls
                            WorldController.Instance.World.PlaceInstalledObject(buildModeObjectType, t);


                        }
                        else
                        {
                            //we are in tile-changing mode
                            t.Type = buildModeTile;
                        }
                        
                    }
                }
            }
        }
    }

    void UpdateCameraMovement()
    {
        if (Input.GetMouseButton(1) || Input.GetMouseButton(2)) //Right or Middle mouse button
        {
            Vector3 diff = lastFramePosition - currFramePosition;
            Camera.main.transform.Translate(diff);
        }

        //scroll wheel
        Camera.main.orthographicSize -= Camera.main.orthographicSize*Input.GetAxis("Mouse ScrollWheel");

        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, 3f, 25f);

    }

    public void SetMode_BuildFloor()
    {
        buildModeIsObjects = false;
        buildModeTile = TileType.Floor;

    }

    public void SetMode_Bulldoze()
    {
        buildModeIsObjects = false;
        buildModeTile = TileType.Empty;

    }

    public void SetMode_BuildInstalledObject(string objectType)
    {
        buildModeIsObjects = true;
        buildModeObjectType = objectType;

        //Wall is not a tile, it is an installedOjbect that exits on top of a tile.
    }
}
