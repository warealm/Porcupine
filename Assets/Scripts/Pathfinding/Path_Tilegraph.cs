﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path_Tilegraph  {

    //This class constructs a simple path-finding compatible graph 
    //of our world. Each tile is a node. Each walkable neighbour 
    //from a tile is linked via an edge connection

    public Dictionary<Tile, Path_Node<Tile>> nodes;

    public Path_Tilegraph(World world)
    {

        //Debug.Log("Path_Tilegraph");
        

        //Loop through all tiles of the world
        //for each tile, create a node
        //Do we create nodes for non-floor tiles? NO!
        //Do we create nodes for tiles that are completely unwalkable (i.e. walls?) NO!

        nodes = new Dictionary<Tile, Path_Node<Tile>>();

        for (int x = 0; x < world.Width; x++)
        {
            for (int y = 0; y < world.Height; y++)
            {

                Tile t = world.GetTileAt(x, y);

                //if(t.movementCost > 0)
                //{
                    Path_Node<Tile> n = new Path_Node<Tile>();
                    n.data = t;
                    nodes.Add(t, n);
                //}
            }
        }

        //Debug.Log("Path_Tilegraph: Created " +nodes.Count + " nodes.");


        int edgeCount = 0;
        //Now loop through all nodes again
        //create edges for neighbours

        foreach (Tile t in nodes.Keys)
        {
            Path_Node<Tile> n = nodes[t];

            List<Path_Edge<Tile>> edges = new List<Path_Edge<Tile>>();
            //get a list of neighbours for the tile
            Tile[] neighbours = t.GetNeighbours(true);


            for (int i = 0; i < neighbours.Length; i++)
            {

                if (neighbours[i] != null && neighbours[i].movementCost > 0)
                {
                    //if neighbour is walkable, create an edge to the relevant node
                    //but first make sure we aren't clipping a diagonal or trying to squeeze inappropriately

                    if (IsClippingCorner(t, neighbours[i]))
                    {
                        continue;
                        //sklip[ to the next neighbour without building an edge
                    }


                    Path_Edge<Tile> e = new Path_Edge<Tile>();
                    e.cost = neighbours[i].movementCost;
                    e.node = nodes[neighbours[i]];

                    //Add the edge to our temporary (and growable!) list
                    edges.Add(e);
                    edgeCount++;
                }
            }

            n.edges = edges.ToArray();

        }

        //Debug.Log("Path_Tilegraph: Created " + edgeCount + " edges.");

    }

    bool IsClippingCorner(Tile currTile, Tile neighTile)
    {
        //if the movement from curr to neigh is diagonal (ie NE)
        //Then check to make sure we aren't clipping (e.g. N and E are both walkable
        if(Mathf.Abs(currTile.X - neighTile.X) + Mathf.Abs(currTile.Y - neighTile.Y) == 2)
        {
            //we are diagonal
            int dX = currTile.X - neighTile.X;
            int dY = currTile.Y - neighTile.Y;

            if (currTile.world.GetTileAt(currTile.X - dX, currTile.Y).movementCost == 0)
            {
                //east or west is unwalkable, therefore this would be a clipped movement
                return true;
            }

            if (currTile.world.GetTileAt(currTile.X, currTile.Y - dY).movementCost == 0)
            {
                //north or south is unwalkable, therefore this would be a clipped movement
                return true;
            }



        }

        return false;

    }



}
