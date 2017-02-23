using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room {

    public float atmosO2 = 0;
    public float atmosN = 0;
    public float atmosCO2 = 0;

    List<Tile> tiles;

    public Room()
    {
        tiles = new List<Tile>();
    }

    public void AssignTile(Tile t)
    {
        if (tiles.Contains(t))
        {
            //this tile already in this room
            return;
        }

        if(t.room != null)
        {
            //belongs to some other room
            t.room.tiles.Remove(t);
        }

        t.room = this;
        tiles.Add(t);

    }

    public void UnAssignAllTiles()
    {
        for (int i = 0; i < tiles.Count; i++)
        {
            tiles[i].room = tiles[i].world.GetOutsideRoom();
        }
        tiles = new List<Tile>();
    }



    public static void DoRoomFloodFill(Furniture sourceFurniture)
    {
        //sourceFurniture is the piece of furniture that may be splitting two existing rooms
        //or may be the final enclosing piece to form a new room
        //check the NESW neighbours of the furniture's tile
        //and do flood fills from them



        World world = sourceFurniture.tile.world;
        Room oldRoom = sourceFurniture.tile.room;

        //try building a new rooms for each of our NESW directions
        foreach(Tile t in sourceFurniture.tile.GetNeighbours())
        {
            ActualFloodFill(t, oldRoom);
        }

        sourceFurniture.tile.room = null;

        oldRoom.tiles.Remove(sourceFurniture.tile);


        
        //If this piece of furniture was added to an existing room
        //(which should always be true assuming we consider "outside" to be a big room)
        //delete that room and assign all tiles within it to be "outside" for now

        if (oldRoom != world.GetOutsideRoom())
        {
            //at this point, opldRoom shouldnt have anymore tiles left in it
            //so in practice this deleteroom should mostly only need to remove the 
            //room from the world's list

            if(oldRoom.tiles.Count > 0)
            {
                Debug.LogError("oldroom still has tiles assigned to it. this si clearly wrong");
            }


            world.DeleteRoom(oldRoom);    
        }
                 

        
    }

    protected static void ActualFloodFill(Tile tile, Room oldRoom)
    {
        if (tile == null)
        {
            //we are trying to flood fill off the map, so just return
            //without doing anything
            return;
        }

        if(tile.room != oldRoom)
        {
            //this tile was already assigned to another "new" room, which means that the direction picked
            //isn't isolated. So we can just return without creating a new room
            return;
        }

        if(tile.furniture != null && tile.furniture.roomEnclosure)
        {
            //this tile has a wall/door in it, so clearly we can't create a room here
            return;
        }

        if (tile.Type == TileType.Empty)
        {
            //this tile is empty space and must remain part of the outside
            return;
        }


       //if we get to this point then we know that we need to create a new room
       //start flood fill

        Room newRoom = new Room();
        Queue<Tile> tilesToCheck = new Queue<Tile>();

        tilesToCheck.Enqueue(tile);

        while(tilesToCheck.Count > 0)
        {
            Tile t = tilesToCheck.Dequeue();


            if(t.room == oldRoom)
            {
                newRoom.AssignTile(t);

                Tile[] ns = t.GetNeighbours();

                foreach(Tile t2 in ns)
                {
                    if(t2 == null || t2.Type == TileType.Empty)
                    {
                        //we have hit open space (either by being the edge of the map or being an empty tile)
                        //so this room we're building is actually part of the outside
                        //therefore we can immediately end the floodfill (which otherwise would take ages)
                        //and more importantly, wee need to delete this new room and reassign all the tiles to outside

                        newRoom.UnAssignAllTiles();
                        return;

                    }


                    //we know t2 is not null nor is it an empty tile so just make sure it hasn't been processed already and isn't a wall type tile
                    if (t2.room == oldRoom && (t2.furniture == null || t2.furniture.roomEnclosure == false))
                    {
                        tilesToCheck.Enqueue(t2);
                    }
                }
            }
        }

        //Copy data from old room into new room
        newRoom.atmosCO2 = oldRoom.atmosCO2;
        newRoom.atmosN = oldRoom.atmosN;
        newRoom.atmosO2 = oldRoom.atmosO2;



        //tell the world that a new room has been formed
        tile.world.AddRoom(newRoom);
        
    }

}
