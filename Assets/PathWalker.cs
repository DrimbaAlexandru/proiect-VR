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

class NodeMap
{
    public static Dictionary<int, Node> nodes = new Dictionary<int, Node>();
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
            if( Yrotation > 180 )
            {
                Yrotation -= 180;
            }
            Id = lastId++;
            isOccupied = false;
            neighbours = new List<Node>();
            System.Console.WriteLine( go.name + ", rot: " + Yrotation + " pos: " + position + " Id: " + Id ); 
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
            float distance = Mathf.Sqrt( Mathf.Pow( this.position.x - n.position.x, 2 ) + Mathf.Pow( this.position.y - n.position.y, 2 ) + Mathf.Pow( this.position.z - n.position.z, 2 ) );
            if( ( this != n ) && ( distance < search_radius ) )
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
        if( neighbours.Contains( direction ) )
        {
            path_Y_angle = getYAngle( this.position, direction.position );
            if( current_position != this.position )
            {
                approaching_angle = getYAngle( current_position, this.position );
                float inverse_YRotation = ( Yrotation > 180 ) ? Yrotation - 180 : Yrotation + 180;
                outgoing_angle = ( Mathf.Abs( approaching_angle - Yrotation ) < Mathf.Abs( approaching_angle - inverse_YRotation ) ) ? Yrotation : inverse_YRotation;
            }
            else
            {
                return 1;
            }
            return Mathf.Pow( ( 180f - Mathf.Abs( path_Y_angle - outgoing_angle ) ) / 180, 3 );
        }
        return 0;
    }
}

public class PathWalker : MonoBehaviour {

	// Use this for initialization
	void Start () {
        System.Console.WriteLine( "Here" );
        foreach( GameObject go in GameObject.FindGameObjectsWithTag( "TestPathNode" ) )
        {
            Node.tryRegisterNewNode( go );
        }
        foreach( Node n in NodeMap.nodes.Values)
        {
            n.calculateNeighbours( 5 );
        }
        Node node3, node0;
        NodeMap.nodes.TryGetValue( 3, out node3 );
        NodeMap.nodes.TryGetValue( 0, out node0 );
        foreach( Node n in NodeMap.nodes.Values )
        {
            Debug.Log( node3.getProbability( n, node0.position ) );
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
