using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        /* if (Input.GetKey(KeyCode.W))
         {
             var x = Input.GetAxis("Vertical") * Time.deltaTime * 5.0f;

             transform.Translate(0, 0, x);
         }
         if (Input.GetKey(KeyCode.A))
         {
             var x = Input.GetAxis("Horizontal") * Time.deltaTime * 150.0f;
             transform.Rotate(0,x,0);
         }
         if (Input.GetKey(KeyCode.D))
         {
             var x = Input.GetAxis("Horizontal") * Time.deltaTime * 150.0f;
             transform.Rotate(0, x, 0);
         }*/

        var x = Input.GetAxis("Vertical") * Time.deltaTime * 2.0f;

        transform.Translate(0, 0, x);

        var x1 = Input.GetAxis("Horizontal") * Time.deltaTime * 150.0f;
        transform.Rotate(0, x1, 0);
    }
}
