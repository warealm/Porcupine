using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;
using System.Linq;

//Random persons comments:I analyzed Quills path-finding algorithm with profiler and found one very slow part in it.
//This implementation use List for closedSet.And slow operation is closedSet.Contains(neighbor), because Contains on List has to enumerate list(some times whole list).
//So solution where is use HashSet instead of List for closedSet.
//This speedup things very much(around 10x in my tets).﻿


public class Path_AStar {

    Queue<Tile> path;

    public Path_AStar(World world, Tile tileStart, Tile tileEnd)
    {
        //Check to see whether we have a valid tilegraph
        if (world.tileGraph == null)
        {
            world.tileGraph = new Path_Tilegraph(world);
        }

        //A dictionary of all valid walkable nodes
        Dictionary<Tile, Path_Node<Tile>> nodes = world.tileGraph.nodes;

        //Make sure our start/end tiles are in the list of nodes
        if (nodes.ContainsKey(tileStart) == false)
        {
            Debug.LogError("Path_AStar -- the starting tile isn't in the list of nodes");

            //Right now, we're going to amnnual add the start tile into the list of valid nodes


            return;
        }

        //Mostly following wikipedia a* pseudo code
        if (nodes.ContainsKey(tileEnd) == false)
        {
            Debug.LogError("Path_AStar -- the ending tile isn't in the list of nodes");
            return;
        }


        Path_Node<Tile> start = nodes[tileStart];
        Path_Node<Tile> goal = nodes[tileEnd];


        List<Path_Node<Tile>> closedSet = new List<Path_Node<Tile>>();
        //List<Path_Node<Tile>> openSet = new List<Path_Node<Tile>>();
        //openSet.Add(start);

        SimplePriorityQueue<Path_Node<Tile>> openSet = new SimplePriorityQueue<Path_Node<Tile>>();
        openSet.Enqueue(start, 0);


        Dictionary<Path_Node<Tile>, Path_Node<Tile>> Came_From = new Dictionary<Path_Node<Tile>, Path_Node<Tile>>();


        Dictionary<Path_Node<Tile>, float> g_score = new Dictionary<Path_Node<Tile>, float>();
        foreach(Path_Node<Tile> n  in nodes.Values)
        {
            g_score[n] = Mathf.Infinity;
        }

        g_score[start] = 0;

        Dictionary<Path_Node<Tile>, float> f_score = new Dictionary<Path_Node<Tile>, float>();
        foreach (Path_Node<Tile> n in nodes.Values)
        {
            f_score[n] = Mathf.Infinity;
        }

        f_score[start] = Heuristic_Cost_Estimate(start, goal);

        while(openSet.Count > 0)
        {
            Path_Node<Tile> current = openSet.Dequeue();

            if(current == goal)
            {
                //we have reached our goal, let's conver this into an actual sequence of tiles to walk on
                //then end this constructor function
                Reconstruct_Path(Came_From, current);

                return;
            }

            closedSet.Add(current);
            foreach(Path_Edge<Tile> edge_neighbour in current.edges)
            {
                Path_Node<Tile> neighbour = edge_neighbour.node;
                if(closedSet.Contains(neighbour) == true)
                {
                    continue;       //ignore this already completed neighbour
                }

                float movement_cost_to_neighbour = neighbour.data.movementCost * dist_between(current, neighbour);

                float tentative_g_score = g_score[current] + movement_cost_to_neighbour;

                if(openSet.Contains(neighbour) && tentative_g_score >= g_score[neighbour])
                {
                    continue;
                }

                Came_From[neighbour] = current;
                g_score[neighbour] = tentative_g_score;
                f_score[neighbour] = g_score[neighbour] + Heuristic_Cost_Estimate(neighbour, goal);

                if (openSet.Contains(neighbour) == false)
                {
                    openSet.Enqueue(neighbour, f_score[neighbour]);
                } else
                {
                    openSet.UpdatePriority(neighbour, f_score[neighbour]);
                }


            }





        }

        //if we reached here, it means that we've burned through the entire openSet without ever reaching a point
        //where current = goal. This happens when there is no path from start to goal
        //so theres a wall or a missing floor or something. We don't have a failure start yet



    }

    float Heuristic_Cost_Estimate(Path_Node<Tile> a, Path_Node<Tile> b)
    {
        //Pythagoreom
        float c = Mathf.Pow(a.data.X - b.data.X, 2) + Mathf.Pow(a.data.Y - b.data.Y, 2);

        return Mathf.Sqrt(c);
    }

    float dist_between(Path_Node<Tile> a, Path_Node<Tile> b)
    {
        //we can make assumptions because we know we're working on a grid at this point
        //hori/vert neighbours have a distance of 1

        if(Mathf.Abs(a.data.X - b.data.X) + Mathf.Abs(a.data.Y - b.data.Y) == 1)
        {
            return 1f;
        }

        //diagnonal neighbours have a distance of sqroot 2
        if (Mathf.Abs(a.data.X - b.data.X) == 1 && Mathf.Abs(a.data.Y - b.data.Y) == 1)
        {
            return Mathf.Sqrt(2);
        }


        //Otherwise do the actual math
        float c = Mathf.Pow(a.data.X - b.data.X, 2) + Mathf.Pow(a.data.Y - b.data.Y, 2);

        return Mathf.Sqrt(c);
    }

    public Tile Dequeue()
    {
        return path.Dequeue();
    }

    void Reconstruct_Path(Dictionary<Path_Node<Tile>, Path_Node<Tile>> Came_From, Path_Node<Tile> current)
    {
        //So at this point, current is the goal. We want to walk backwards through the came from map until we reach the starting tile

        Queue<Tile> total_path = new Queue<Tile>();
        total_path.Enqueue(current.data);            //The final step in the path is the goal
        while (Came_From.ContainsKey(current))
        {
            //camefrom is a map where the key value relation is really saying some node => we got there from this node
            current = Came_From[current];
            total_path.Enqueue(current.data);
        }

        //At this point, total path is a queue that is running backwards from the endtile to the starttile, so lets reverse it.
        path = new Queue<Tile>(total_path.Reverse());
    }


    public int Length()
    {
        if(path == null)
        {
            return 0;
        }

        return path.Count;
    }

}
