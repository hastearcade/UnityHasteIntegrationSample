using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class LeaderboardSelection : NetworkBehaviour
{
    void Start()
    {
        if (isLocalPlayer)
        {
            Debug.Log("local");
        }
        else
        {
            Debug.Log("server");
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
