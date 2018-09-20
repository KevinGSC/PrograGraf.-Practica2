using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(BoxCollider2D))]
public class Controller2D : MonoBehaviour
{
    public LayerMask collisionMask;

    const float skinWidth = 0.015f;

    public int horizontalRayCount = 4;//-----rayo horizontal
    public int verticalRayCount = 4;//-----rayo vertical

    public float maxClimbAngle = 80; // Maximo angulo de inclinacion
    float horizontalRaySpacing;
    float verticalRaySpacing;

    BoxCollider2D collider2d;
    RaycastOrigins raycastOrigins;

    public CollisionInfo collisions;

    void Start()
    {
        collider2d = GetComponent<BoxCollider2D>();
        CalculateRaySpacing();
    }

    void Update()
    {
        
    }
    public void Move(Vector3 velocity)
    {
        UpdateRaycastOrigin();

        collisions.Reset();//---------------reinicio las variables cada vez que me muevo

        if (velocity.x != 0)
        {
            HorizontalCollisions(ref velocity);
        }
        if (velocity.y != 0)
        {
            VerticalCollisions(ref velocity);
        }
        transform.Translate(velocity);
    }
    void HorizontalCollisions(ref Vector3 velocity)
    {
        float directionX = Mathf.Sign (velocity.x);
        float rayLength = Mathf.Abs(velocity.x) + skinWidth;

        for (int i = 0; i < horizontalRayCount; i++)
        {
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottonLeft : raycastOrigins.BottonRight;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin,
                Vector2.right * directionX, rayLength, 
                collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength,
                Color.red);

            if (hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up); //Tomar el angulo
                Debug.Log(slopeAngle);

                if (i == 0 && slopeAngle <= maxClimbAngle)
                {
                    float distanceToSlopeStart = 0;     //Distancia para empezar la inclinacion
                    if (slopeAngle != collisions.slopeAngleOld)
                    {
                        distanceToSlopeStart = hit.distance - skinWidth; //toma en valor del angulo
                        Debug.Log(distanceToSlopeStart);
                        velocity.x -= distanceToSlopeStart * directionX; // asigna la velocidad
                        Debug.Log(velocity.x);
                    }
                    ClimbSlope(ref velocity, slopeAngle); // angulo para trepar, toma referencias de velocidad y del angulo
                    velocity.x += distanceToSlopeStart * directionX;
                }
                if (!collisions.climbingSlope || slopeAngle > maxClimbAngle) //deniega la collision de la inclinacion
                {
                    velocity.x = (hit.distance - skinWidth) * directionX;
                    rayLength = hit.distance;

                    if (collisions.climbingSlope)          //condiciona la collision
                    {
                        velocity.y = Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x); //Deg2Rad es para convertir a radianes, Abs para obtener valor absoluto, es una funcion de Tangente para el angulo
                    }
                    collisions.left = directionX == -1;//---------almacena//-Condicion true o false              
                    collisions.right = directionX == 1;
                }               
            }
        }
    }
    void VerticalCollisions(ref Vector3 velocity)
    {
        float directionY = Mathf.Sign(velocity.y);
        float rayLength = Mathf.Abs(velocity.y) + skinWidth;

        for (int i = 0; i < verticalRayCount; i++)//----------
        {
            //GENERAMOS EL RAYCAST


            Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottonLeft : raycastOrigins.topLeft;//-----------------cambio
            rayOrigin += Vector2.right * (verticalRaySpacing* i);//----generando el rayo a la derecha
          /* recibo el rayo*/ RaycastHit2D hit = Physics2D.Raycast(rayOrigin,
                Vector2.up * directionY, rayLength,//generando rayos en vertical
                collisionMask);
            

            Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength,//origen y direccion y el color del rayo
                Color.red);

            if (hit)
            {               
                velocity.y = (hit.distance - skinWidth) * directionY;
                rayLength = hit.distance;

                if (collisions.climbingSlope)
                {
                    velocity.x = velocity.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) - Mathf.Abs(velocity.x);
                }

                collisions.above = directionY == 1;

                collisions.below = directionY == -1;
            }
        }
    }
    public void ClimbSlope(ref Vector3 velocity, float slopeAngle)
    {
        float moveDistance = Mathf.Abs(velocity.x);
        float climbVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

        if (velocity.y <= climbVelocityY)
        {
            velocity.y = climbVelocityY;
            velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
            collisions.below = true;
            collisions.climbingSlope = true;
            collisions.slopeAngle = slopeAngle;
        }
    }
    void UpdateRaycastOrigin()
    {
        Bounds bounds = collider2d.bounds;
        bounds.Expand(skinWidth * -2);

        raycastOrigins.bottonLeft = new Vector2(bounds.min.x, bounds.min.y);
        raycastOrigins.BottonRight = new Vector2(bounds.max.x, bounds.min.y);
        raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
    }
    void CalculateRaySpacing()
    {
        Bounds bounds = collider2d.bounds;  
        bounds.Expand(skinWidth * -2);

        horizontalRaySpacing = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
        verticalRaySpacing = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);

        horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
        verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
    }
    struct RaycastOrigins
    {
        public Vector2 topLeft, topRight;
        public Vector2 bottonLeft, BottonRight;
    }

    public struct CollisionInfo
    {

        //ARRIBA---ABAJO
        public bool above, below;
        public bool left, right;

        public bool climbingSlope;                               // Collision de inclinacion
        public float slopeAngle, slopeAngleOld;                   //Variables para almacenar el angulo nuevo y antiguo como un auxiliar

        public void Reset()
        {
            above = below = false;   
            left = right = false;        
            climbingSlope = false;

            slopeAngleOld = slopeAngle;                         //Asignamos el valor del angulo nuevo al angulo antiguo para que lo almacene 
            slopeAngle = 0;                                     //Asignamos un valor de 0 al angulo
        }
    }
}
