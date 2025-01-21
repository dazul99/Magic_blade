using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Connection : MonoBehaviour
{
    [SerializeField] private Room firstRoom;
    [SerializeField] private Room secondRoom;
    private Collider2D coll;
    [SerializeField] private bool isLadder;

    private void Awake()
    {
        coll = GetComponent<Collider2D>();
    }

    public void SetRoomsConnected(Room first, Room second)
    {
        firstRoom = first;
        secondRoom = second;
    }

    public (Room, Room) GetRooms()
    {
        return (firstRoom,secondRoom);
    }

    public bool IsLadder()
    {
        return isLadder;
    }

    public bool HasRoom(Room room)
    {
        return firstRoom == room || secondRoom == room;
    }

    public Room GetOtherRoom(Room room)
    {
        if (room == firstRoom) return secondRoom;
        if (room == secondRoom) return firstRoom;
        return null;
    }

    public Vector2 GetPos()
    {
        return transform.position;
    }

}
