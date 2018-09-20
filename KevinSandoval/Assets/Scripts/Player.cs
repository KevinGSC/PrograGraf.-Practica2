using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Controller2D))]
public class Player : MonoBehaviour
{
    public float jumpHeight = 4;
    public float timeToJumpApex = 0.4f;//------------------tiempo para llegar a la altura maxima

    float acelerationTimeAirbone = 0.2f;
    float acelerationTimeGrounded = 0.1F;


    float moveSpeed = 6 ;
    float gravity ;

    float velocitySmoot;
    float jumpVelocity;
    Vector3 velocity;

    Controller2D controller;
    void Start()
    {
        controller = GetComponent<Controller2D>();

        gravity = -(2 * jumpHeight) / Mathf.Pow(timeToJumpApex, 2);

        jumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;

        Debug.Log("Gravity: " + gravity + " Jump Velocity: " + jumpVelocity);
    }

    void Update()
    {
        if(controller.collisions.above || controller.collisions.below)//-----todo el tiempo genero el raycast
        {
            velocity.y = 0;
        }





        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if(Input.GetKeyDown(KeyCode.Space) && controller.collisions.below)//si la collision esta abajo
        {
            velocity.y = jumpVelocity;
        }


        float targetVelocityX = input.x * moveSpeed;


        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocitySmoot, ( controller.collisions.below)?acelerationTimeGrounded:acelerationTimeAirbone);//velocidad actaualy la velocidad objetivo
        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);




    }
}
