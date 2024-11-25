using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ladder : MonoBehaviour
{
    [SerializeField] private GameObject Top;
    [SerializeField] private GameObject Bottom;

    public GameObject GetTop() { return Top; }
    public GameObject GetBottom() { return Bottom;}

    public Vector2 TopPos()
    {
        return Top.transform.position;
    }

    public Vector2 BottomPos()
    {
        return Bottom.transform.position;
    }

}
