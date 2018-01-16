using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveFromAtoB : MonoBehaviour
{
    public float speed = 1f;
    private Vector3 destination;
    private Vector3 intermediary;
    private Node intermediary_node;
    private Node destination_node;

    public float default_approach_radius;
    private float approach_radius;
    private float amount_left_to_rotate;
    private Vector3 rotation_pivot;
    private bool around_intermediary_point = false;
    private bool around_destination_point = false;

    private bool move = false;
    public float stationary_rotation_speed = 30;

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
    private static bool LineLineIntersection( out Vector3 intersection, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2 )
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
        StartCoroutine( _start() );
    }

    IEnumerator _start()
    {
        print( Time.time );
        while( !NodeMap.hasFinishedInitializing )
        {
            print( "Waiting" );
            yield return new WaitForSeconds( 0.1f );
        }
        print( "Finished " + Time.time );
        Node i = new Node( this.gameObject );
        i.position -= ( NodeMap.nodes[ 5 ].position - i.position ).normalized * 0.3f;

        resetParams( NodeMap.nodes[ 5 ], i );
        move = true;
    }

    private void resetParams( Node dest, Node interm )
    {
        Vector3 rotation_start_point, rotation_end_point;
        float incoming_angle, outgoing_angle;

        destination_node = dest;
        intermediary_node = interm;
        destination = dest.position;
        intermediary = interm.position;

        around_destination_point = false;
        around_intermediary_point = false;

        incoming_angle = getYAngle( transform.position, intermediary );
        outgoing_angle = getYAngle( intermediary, destination );
        approach_radius = Mathf.Min( default_approach_radius, Vector3.Distance( intermediary, this.transform.position ) );
        if( approach_radius < default_approach_radius )
        {
            around_intermediary_point = true;
        }

        this.transform.rotation = Quaternion.Euler( transform.rotation.x, incoming_angle, transform.rotation.z );

        rotation_start_point = intermediary + Quaternion.Euler( 0, getInverseAngle( incoming_angle ), 0 ) * Vector3.forward * approach_radius;
        rotation_end_point = intermediary + Quaternion.Euler( 0, outgoing_angle, 0 ) * Vector3.forward * approach_radius;

        if( !LineLineIntersection( out rotation_pivot, rotation_end_point, Quaternion.Euler( 0, ( outgoing_angle - 90 ), 0 ) * Vector3.forward, rotation_start_point, Quaternion.Euler( 0, normalizeAngle( incoming_angle - 90 ), 0 ) * Vector3.forward ) )
        {
            rotation_pivot = intermediary;
            amount_left_to_rotate = getRotationAngle( incoming_angle, outgoing_angle );
        }
        else
        {
            amount_left_to_rotate = getRotationAngle( incoming_angle, outgoing_angle );
        }
    }

    // Update is called once per frame
    void Update()
    {
        if( !move )
            return;

        if( Vector3.Distance( transform.position, intermediary ) <= approach_radius )
        {
            around_intermediary_point = true;
        }
        else
        {
            if( around_intermediary_point )
            {
                if( intermediary_node.isOccupied == true )
                {
                    intermediary_node.isOccupied = false;
                    intermediary_node.obj.GetComponent<Renderer>().material.color = Color.green;
                }
            }
            around_intermediary_point = false;
        }
        if( Vector3.Distance( transform.position, destination ) <= default_approach_radius )
        {
            around_destination_point = true;
        }
        else
        {
            around_destination_point = false;
        }
        if( around_destination_point )
        {
            if( intermediary_node.isOccupied == true )
            {
                intermediary_node.isOccupied = false;
                intermediary_node.obj.GetComponent<Renderer>().material.color = Color.green;
            }
            onApproachingDestination();
        }

        if( around_intermediary_point )
        {
            if( rotation_pivot == intermediary )
            {
                if( amount_left_to_rotate == 0 || Vector3.Distance( transform.position, rotation_pivot ) != 0f )
                {
                    moveOneStep();
                }
                else
                {
                    rotateAroundSelf();
                }
            }
            else
            {
                if( amount_left_to_rotate != 0 )
                {
                    rotateOneStep();
                }
                else
                {
                    moveOneStep();
                }
            }
        }
        else
        {
            if( around_destination_point )
            {
                moveOneStep();
                //intermediary_node.isOccupied = false;
            }
            else
            {
                moveOneStep();
            }
        }

    }

    private void moveOneStep()
    {
        Vector3 movement = Quaternion.Euler( 0, transform.rotation.y, 0 ) * Vector3.forward * Time.deltaTime * speed;
        if( around_destination_point && ( Vector3.Distance( this.transform.position, destination ) < movement.magnitude ) )
        {
            transform.position = destination;
        }
        else if( around_intermediary_point && ( Vector3.Distance( this.transform.position, intermediary ) < movement.magnitude ) && ( transform.position != intermediary ) )
        {
            transform.position = intermediary;
        }
        else
        {
            transform.Translate( movement );
        }
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

    private void rotateAroundSelf()
    {
        float amount_to_rotate = stationary_rotation_speed * Time.deltaTime * Mathf.Sign( amount_left_to_rotate );
        if( Mathf.Abs( amount_left_to_rotate ) < Mathf.Abs( amount_to_rotate ) )
        {
            amount_to_rotate = amount_left_to_rotate;
        }
        amount_left_to_rotate -= amount_to_rotate;
        transform.RotateAround( rotation_pivot, Vector3.up, amount_to_rotate );
    }

    private float transformProbability( float p )
    {
        return Mathf.Pow( p * 0.999f + 0.001f, 10 );
    }

    private void onApproachingDestination()
    {
        List<KeyValuePair<Node, float>> probs = new List<KeyValuePair<Node, float>>();
        float f;
        float sum = 0;
        float rand;
        foreach( Node n in destination_node.neighbours )
        {
            f = destination_node.getProbability( n, transform.position );
            if( f >= 0 )
            {
                f = transformProbability( f );
                probs.Add( new KeyValuePair<Node, float>( n, f ) );
                sum += f;
            }
        }
        rand = Random.value * sum;
        sum = 0;
        foreach( KeyValuePair<Node, float> kvp in probs )
        {
            sum += kvp.Value;
            if( rand <= sum )
            {
                resetParams( kvp.Key, destination_node );
                if( destination_node.isOccupied )
                {
                    Debug.Log( "!!!!!!!" );
                }
                destination_node.isOccupied = true;
                destination_node.obj.GetComponent<Renderer>().material.color = Color.red;
                break;
            }
        }

    }
}
