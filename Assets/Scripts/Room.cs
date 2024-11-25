using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    private BoxCollider2D coll;

    [SerializeField] private int id;

    private List<Room> connectedRooms = new List<Room>();

    [SerializeField] private List<Connection> connections = new List<Connection>(); 

    private void Start()
    {
        coll = GetComponent<BoxCollider2D>();
    }

    public void SetID(int x)
    {
        id = x;
    }

    public int GetID()
    {
        return id;
    }

    public void setConnection(Connection c)
    {
        connections.Add(c);
        connectedRooms.Add(c.GetOtherRoom(this));
    }

    public List<Connection> GetConnections()
    {
        return connections;
    }

    public Connection FindConnection(Room other)
    {
        foreach (Connection c in connections)
        {
            if(c.GetOtherRoom(this) == other) return c;
        }
        return null;
    }

    public Vector2 GetPos()
    {
        return new Vector2(coll.bounds.center.x, coll.bounds.center.y);
    }

}
