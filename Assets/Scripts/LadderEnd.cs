using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadderEnd : MonoBehaviour
{
    private bool isTop;
    private Ladder ladder;

    private void Start()
    {
        ladder = GetComponentInParent<Ladder>();
        if(transform.gameObject == ladder.GetTop()) isTop = true;
        else isTop = false;
            
    }

    public bool GetTop()
    {
        return isTop;
    }

}
