using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    private Room[] rooms;

    private Connection[] connections;

    private List<Room> visitedRooms;
    private List<(Queue<Room>,int, Room)> availableRooms;
    private Queue<Room> currentPath;
    private int currentCost;
    private bool foundPath;

    private void Start()
    {
        connections = GetComponentsInChildren<Connection>();
        rooms = GetComponentsInChildren<Room>();
        int n = 0;
        foreach (Room room in rooms)
        {
            room.SetID(n);
            n++;
            foreach (Connection connection in connections)
            {
                if(connection.HasRoom(room)) room.setConnection(connection);
            }
        }
    }

    public Queue<Room> GetPath(Room start, Room end)
    {
        visitedRooms = new List<Room> ();
        currentPath = new Queue<Room>();
        availableRooms = new List<(Queue<Room>, int, Room)>();
        currentPath.Enqueue(start);
        currentCost = 0;
        foundPath = false;
        GetPaths(start, end);
        if(foundPath) return currentPath;
        return null;
    }
    private bool GetPaths(Room currentRoom, Room end)
    {
        if(visitedRooms.Contains(currentRoom)) return false;
        if (currentRoom == end)
        {
            foundPath = true;
            return true;
        }
        else
        {
            visitedRooms.Add(currentRoom);
            GetAllConnectionsRoom(currentRoom);
            int min = 99999;
            (Queue<Room>, int, Room) tripleaux = (null, 0 ,null);
            foreach ((Queue<Room>, int, Room) triple in availableRooms)
            {
                if(triple.Item2 < min)
                {
                    tripleaux = triple;
                    min = tripleaux.Item2;
                }
            }

            if (tripleaux.Item1 != null)
            {
                availableRooms.Remove(tripleaux);
                currentPath = tripleaux.Item1;
                currentCost = tripleaux.Item2;
                if(GetPaths(tripleaux.Item3, end))
                {
                    return true;
                }
            }
        }
        return false;
        
    }

    private void GetAllConnectionsRoom(Room room)
    {
        List<Connection> roomConn = room.GetConnections();
        Queue<Room> auxq;
        int costaux;
        foreach (Connection connection in roomConn) 
        {
            if (!visitedRooms.Contains(connection.GetOtherRoom(room)))
            {
                auxq = new Queue<Room>(currentPath);
                auxq.Enqueue(connection.GetOtherRoom(room));
                costaux = currentCost;
                if (connection.IsLadder()) costaux += 3;
                else costaux++;
                availableRooms.Add((auxq, costaux, connection.GetOtherRoom(room)));
            }
        }
    }

    public Room GetRoomWithID(int identifier)
    {
        foreach (Room room in rooms)
        {
            if(room.GetID() == identifier) return room;
        }
        return null;
    }


}
