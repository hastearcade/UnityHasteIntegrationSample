using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Player : NetworkBehaviour 
{
    void HandleMovement()
    {
        if(isLocalPlayer)
        {
            var moveHorizontal = Input.GetAxis("Horizontal");
            var moveVertical = Input.GetAxis("Vertical");

            var movement = new Vector3(moveHorizontal * 0.1f, moveVertical * 0.1f, 0);

            transform.position = transform.position + movement;
        }
    }

    void Update()
    {
        HandleMovement();
    }
}
