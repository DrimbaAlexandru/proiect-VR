using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class Path
{
    private Node start { get; set; }
    private Node end   { get; set; }
    private bool isOccupied { get; set; }
    private List<Path> intersectedPaths = new List<Path>();
}

public class NodeMap
{
    public static Dictionary<int, Node> nodes = new Dictionary<int, Node>();
    public static bool hasFinishedInitializing = false;
}

class Node
{
    public Vector3 position ;
    public float Yrotation ;
    public bool isOccupied;
    public int Id;
    public static int lastId;
    public List<Node> neighbours;
        
    private Node( GameObject go )
    {
        Transform t = go.GetComponent<Transform>();
        if( t != null )
        {
            position = t.position;
            Yrotation = t.rotation.eulerAngles.y;
            /*if( Yrotation >= 180 )
            {
                Yrotation -= 180;
            }*/
            Id = lastId++;
            isOccupied = false;
            neighbours = new List<Node>();
            //System.Console.WriteLine( go.name + ", rot: " + Yrotation + " pos: " + position + " Id: " + Id ); 
        }
    }

    public static void tryRegisterNewNode( GameObject go )
    {
        int oldId = lastId;
        Node n = new Node( go );
        if( oldId < lastId )
        {
            NodeMap.nodes.Add( n.Id, n );
        }
    }

    public void calculateNeighbours( float search_radius )
    {
        neighbours.Clear();
        foreach( Node n in NodeMap.nodes.Values )
        {
            if( ( this != n ) && ( Vector3.Distance( this.position, n.position ) < search_radius ) )
            {
                neighbours.Add( n );
            }
        }
    }

    private static float getYAngle( Vector3 pos1, Vector3 pos2 )
    {
        pos1.y = 0;
        pos2.y = 0;
        return Quaternion.FromToRotation( Vector3.forward, ( pos2 - pos1 ).normalized ).eulerAngles.y;
    }

    public float getProbability( Node direction, Vector3 current_position )
    {
        float path_Y_angle = 0;
        float approaching_angle = 0;
        float outgoing_angle = 0;
        float inverse_approaching_angle = 0;
        if( neighbours.Contains( direction ) )
        {
            path_Y_angle = getYAngle( this.position, direction.position );
            if( current_position != this.position )
            {
                approaching_angle = getYAngle( current_position, this.position );
                float inverse_YRotation = ( Yrotation >= 180 ) ? Yrotation - 180 : Yrotation + 180;
                outgoing_angle = ( Mathf.Abs( approaching_angle - Yrotation ) < Mathf.Abs( approaching_angle - inverse_YRotation ) ) ? Yrotation : inverse_YRotation;
            }
            else
            {
                return 1;
            }
            //return ( 180f - Mathf.Abs( path_Y_angle - outgoing_angle ) ) / 180;
            inverse_approaching_angle = ( approaching_angle >= 180 ) ? approaching_angle - 180 : approaching_angle + 180;
            if( inverse_approaching_angle < outgoing_angle )
            {
                if( path_Y_angle < inverse_approaching_angle )
                {
                    return ( inverse_approaching_angle - path_Y_angle ) / ( inverse_approaching_angle - outgoing_angle + 360 );
                }
                else if( path_Y_angle < outgoing_angle )
                {
                    return ( path_Y_angle - inverse_approaching_angle ) / ( outgoing_angle - inverse_approaching_angle );
                }
                else
                {
                    return ( inverse_approaching_angle + 360 - path_Y_angle ) / ( inverse_approaching_angle - outgoing_angle + 360 );
                }
            }
            else
            {
                if( path_Y_angle < outgoing_angle )
                {
                    return ( path_Y_angle - inverse_approaching_angle + 360 ) / ( outgoing_angle - inverse_approaching_angle + 360 );
                }
                else if( path_Y_angle < inverse_approaching_angle )
                {
                    return ( inverse_approaching_angle - path_Y_angle ) / ( inverse_approaching_angle - outgoing_angle );
                }
                else
                {
                    return ( path_Y_angle - inverse_approaching_angle ) / ( outgoing_angle + 360 + inverse_approaching_angle );
                }
            }
        }
        return -1;
    }
}

public class PathWalker : MonoBehaviour {

	// Use this for initialization
	void Start () {
        const float neighbour_search_radius = 5;
        foreach( GameObject go in GameObject.FindGameObjectsWithTag( "TestPathNode" ) )
        {
            Node.tryRegisterNewNode( go );
        }
        foreach( Node n in NodeMap.nodes.Values)
        {
            Debug.Log( n.Id + " - " + n.position );
            n.calculateNeighbours( neighbour_search_radius );
        }

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
