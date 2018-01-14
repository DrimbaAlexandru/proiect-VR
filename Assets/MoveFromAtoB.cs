using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveFromAtoB : MonoBehaviour
{
    private float speed = 1f;
    private Vector3 destination;
    private Vector3 intermediary;
    private float approach_radius;
    private float amount_left_to_rotate = 0;
    private Vector3 rotation_pivot;

    private static float getYAngle( Vector3 pos1, Vector3 pos2 )
    {
        pos1.y = 0;
        pos2.y = 0;
        return Quaternion.FromToRotation( Vector3.forward, ( pos2 - pos1 ).normalized ).eulerAngles.y;
    }

    private static float getInverseAngle( float angle )
    {
        return ( angle >= 180 ) ? angle - 180 : angle + 180;
    }

    private static float normalizeAngle( float a )
    {
        while( a >= 360 )
            a -= 360;
        while( a < 0 )
            a += 360;
        return a;
    }

    private static float getRotationAngle( float inA, float outA )
    {
        float cwra = normalizeAngle( outA - inA );
        float acwra = -normalizeAngle( inA - outA );
        if( -acwra < cwra )
            return acwra;
        else
            return cwra;
    }

    //Calculate the intersection point of two lines. Returns true if lines intersect, otherwise false.
    //Note that in 3d, two lines do not intersect most of the time. So if the two lines are not in the 
    //same plane, use ClosestPointsOnTwoLines() instead.
    public static bool LineLineIntersection( out Vector3 intersection, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2 )
    {
        Vector3 lineVec3 = linePoint2 - linePoint1;
        Vector3 crossVec1and2 = Vector3.Cross( lineVec1, lineVec2 );
        Vector3 crossVec3and2 = Vector3.Cross( lineVec3, lineVec2 );

        float planarFactor = Vector3.Dot( lineVec3, crossVec1and2 );

        //is coplanar, and not parrallel
        if( Mathf.Abs( planarFactor ) < 0.0001f && crossVec1and2.sqrMagnitude > 0.0001f )
        {
            float s = Vector3.Dot( crossVec3and2, crossVec1and2 ) / crossVec1and2.sqrMagnitude;
            intersection = linePoint1 + ( lineVec1 * s );
            return true;
        }
        else
        {
            intersection = Vector3.zero;
            return false;
        }
    }

    // Use this for initialization
    void Start()
    {
        Vector3 rotation_start_point, rotation_end_point;
        float incoming_angle, outgoing_angle;

        destination = new Vector3( -210, 0, 0 );
        intermediary = new Vector3( -220, 0, 0 );

        incoming_angle = getYAngle( transform.position, intermediary );
        outgoing_angle = getYAngle( intermediary, destination );
        approach_radius = 2;

        this.transform.rotation = Quaternion.Euler( transform.rotation.x, incoming_angle, transform.rotation.z );

        rotation_start_point = intermediary + Quaternion.Euler( 0, getInverseAngle( incoming_angle ), 0 ) * Vector3.forward * approach_radius;
        rotation_end_point = intermediary + Quaternion.Euler( 0, outgoing_angle, 0 ) * Vector3.forward * approach_radius;

        if( !LineLineIntersection( out rotation_pivot, rotation_end_point, Quaternion.Euler( 0, ( outgoing_angle - 90 ), 0 ) * Vector3.forward, rotation_start_point, Quaternion.Euler( 0, normalizeAngle( incoming_angle - 90 ), 0 ) * Vector3.forward ) )
        {
            rotation_pivot = intermediary;
        }
        else
        {
            amount_left_to_rotate = getRotationAngle( incoming_angle, outgoing_angle );
        }

    }

    // Update is called once per frame
    void Update()
    {
        if( Vector3.Distance( transform.position, destination ) < 0.1f )
            return;

        //Around the intermediary point. Rotate around pivot or move straigt forward.
        if( Vector3.Distance( transform.position, intermediary ) < approach_radius )
        {
            if( rotation_pivot == intermediary )
            {
                moveOneStep();
            }
            else
            {
                rotateOneStep();
            }
        }
        else
        {
            moveOneStep();
        }

    }

    private void moveOneStep()
    {
        transform.Translate( Quaternion.Euler( 0, transform.rotation.y, 0 ) * Vector3.forward * Time.deltaTime * speed );
    }

    private void rotateOneStep()
    {
        float amount_to_rotate = 360f / ( 2 * Mathf.PI * Vector3.Distance( rotation_pivot, intermediary ) / speed ) * Time.deltaTime * Mathf.Sign( amount_left_to_rotate );
        if( Mathf.Abs( amount_left_to_rotate ) < Mathf.Abs( amount_to_rotate ) )
        {
            amount_to_rotate = amount_left_to_rotate;
        }
        amount_left_to_rotate -= amount_to_rotate;
        transform.RotateAround( rotation_pivot, Vector3.up, amount_to_rotate );
    }
}
