using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ladder : MonoBehaviour
{
    [SerializeField] private GameObject top;
    [SerializeField] private GameObject bottom;

    public GameObject GetTop() { return top; }
    public GameObject GetBottom() { return bottom;}

    public Vector2 TopPos()
    {
        return top.transform.position;
    }

    public Vector2 BottomPos()
    {
        return bottom.transform.position;
    }

}
