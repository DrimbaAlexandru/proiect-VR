using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path
{
    public Node start { get; set; }
    public Node end   { get; set; }
    public bool isOccupied { get; set; }
    public List<Path> intersectedPaths = new List<Path>();

    public bool intersects( Path other )
    {
        if( this.start == other.start || this.start == other.end || this.end == other.start || this.end == other.end )
        {
            return false;
        }
        Vector2 p1 = new Vector2( this.start.position.x, this.start.position.z );
        Vector2 p2 = new Vector2( this.end.position.x, this.end.position.z );
        Vector2 p3 = new Vector2( other.start.position.x, other.start.position.z );
        Vector2 p4 = new Vector2( other.end.position.x, other.end.position.z );
        return Utils.segments_intersect( p1, p2, p3, p4 );
    }

    public static bool can_be_taken( Node origin, Node destination )
    {
        Path path = PathMap.getPath( origin, destination );
        bool intersects_occupied_paths = false;
        if( destination.isOccupied )
            return false;
        foreach( Path p in path.intersectedPaths )
        {
            intersects_occupied_paths |= p.isOccupied;
        }
        return !intersects_occupied_paths;
    }

}

public class PathMap
{
    public static Dictionary<Node, List<Path>> paths = new Dictionary<Node, List<Path>>();
    public static List<Path> all_paths = new List<Path>();

    private static void registerNodeIfNotExists(Node n)
    {
        if( !paths.ContainsKey( n ) )
        {
            paths.Add( n, new List<Path>() );
        }
    }

    public static void addPath( Path p )
    {
        registerNodeIfNotExists( p.start );
        registerNodeIfNotExists( p.end );
        if( getPath( p.start, p.end ) == null )
        {
            //Debug.Log( "Added path between " + p.start.Id + " and " + p.end.Id );
            paths[ p.start ].Add( p );
            paths[ p.end ].Add( p );
            all_paths.Add( p );
        }
    }

    public static Path getPath( Node n1, Node n2 )
    {
        foreach( Path p in paths[ n1 ] )
        {
            if( p.start == n2 || p.end == n2 )
            {
                return p;
            }
        }
        return null;
    }
}

public class NodeMap
{
    public static Dictionary<int, Node> nodes = new Dictionary<int, Node>();
    public static bool hasFinishedInitializing = false;
}

public class Node
{
    public Vector3 position;
    public float Yrotation;
    public bool isOccupied;
    public int Id;
    public static int lastId;
    public List<Node> neighbours;
    public GameObject obj;
        
    public Node( GameObject go )
    {
        Transform t = go.GetComponent<Transform>();
        if( t != null )
        {
            position = t.position;
            Yrotation = t.rotation.eulerAngles.y;
            Id = lastId++;
            isOccupied = false;
            neighbours = new List<Node>();
            obj = go;
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

    public float getProbability( Node direction, Vector3 current_position )
    {
        float path_Y_angle = 0;
        float approaching_angle = 0;
        float outgoing_angle = 0;
        float inverse_approaching_angle = 0;
        if( neighbours.Contains( direction ) )
        {
            if( !Path.can_be_taken( this, direction ) )
                return -1;
            path_Y_angle = Utils.getYAngle( this.position, direction.position );
            if( current_position != this.position )
            {
                approaching_angle = Utils.getYAngle( current_position, this.position );
                float inverse_YRotation = Utils.getInverseAngle( Yrotation );
                outgoing_angle = ( Mathf.Abs( Utils.getRotationAngle( approaching_angle, Yrotation ) ) < Mathf.Abs( Utils.getRotationAngle( approaching_angle, inverse_YRotation ) ) ) ? Yrotation : inverse_YRotation;
            }
            else
            {
                return 1;
            }
            inverse_approaching_angle = Utils.getInverseAngle( approaching_angle );
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
                    return ( path_Y_angle - inverse_approaching_angle ) / ( outgoing_angle + 360 - inverse_approaching_angle );
                }
            }
        }
        return -1;
    }
}

public class PathWalker : MonoBehaviour {
    public float neighbour_search_radius = 3.5f;

	// Use this for initialization
	void Start () {
        foreach( GameObject go in GameObject.FindGameObjectsWithTag( "PathNode" ) )
        {
            Node.tryRegisterNewNode( go );
        }
        foreach( Node n in NodeMap.nodes.Values )
        {
            //Debug.Log( n.Id + " - " + n.position );
            n.calculateNeighbours( neighbour_search_radius );
            foreach( Node n1 in n.neighbours )
            {
                Path path = new Path();
                path.start = n1;
                path.end = n;
                path.isOccupied = false;
                PathMap.addPath( path );
            }
        }
        foreach( Path p1 in PathMap.all_paths )
        {
            foreach( Path p2 in PathMap.all_paths )
            {
                if( p1 != p2 && p1.intersects( p2 ) )
                {
                    //Debug.Log( "Path between " + p1.start.Id + " and " + p1.end.Id + " intersects " + p2.start.Id + " and " + p2.end.Id );
                    p1.intersectedPaths.Add( p2 );
                }
            }
        }
        NodeMap.hasFinishedInitializing = true;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

public class Utils
{
    public static Vector3 getZeroYVector3( Vector3 v )
    {
        return new Vector3( v.x, 0, v.z );
    }

    public static float getYAngle( Vector3 pos1, Vector3 pos2 )
    {
        return Quaternion.FromToRotation( Vector3.forward, ( getZeroYVector3( pos2 ) - getZeroYVector3( pos1 ) ).normalized ).eulerAngles.y;
    }

    public static float getInverseAngle( float angle )
    {
        return ( angle >= 180 ) ? angle - 180 : angle + 180;
    }

    public static float normalizeAngle( float a )
    {
        while( a >= 360 )
            a -= 360;
        while( a < 0 )
            a += 360;
        return a;
    }

    public static float getRotationAngle( float inA, float outA )
    {
        float cwra = normalizeAngle( outA - inA );
        float acwra = -normalizeAngle( inA - outA );
        if( -acwra < cwra )
            return acwra;
        else
            return cwra;
    }

    public static bool segments_intersect( Vector2 s1a, Vector2 s1b, Vector2 s2a, Vector2 s2b )
    {
        /*bool isIntersecting = false;

        float denominator = ( s2b.y - s2a.y ) * ( s1b.x - s1a.x ) - ( s2b.x - s2a.x ) * ( s1b.y - s1a.y );

        //Make sure the denominator is > 0, if so the lines are parallel
        if( denominator != 0 )
        {
            float u_a = ( ( s2b.x - s2a.x ) * ( s1a.y - s2a.y ) - ( s2b.y - s2a.y ) * ( s1a.x - s2a.x ) ) / denominator;
            float u_b = ( ( s1b.x - s1a.x ) * ( s1a.y - s2a.y ) - ( s1b.y - s1a.y ) * ( s1a.x - s2a.x ) ) / denominator;

            //Is intersecting if u_a and u_b are between 0 and 1
            if( u_a >= 0 && u_a <= 1 && u_b >= 0 && u_b <= 1 )
            {
                isIntersecting = true;
            }
        }

        return isIntersecting;*/
        Vector3 inters;
        Vector3 p1 = new Vector3( s1a.x, 0, s1a.y ), v1 = new Vector3( s1b.x - s1a.x, 0, s1b.y - s1a.y ).normalized;
        Vector3 p2 = new Vector3( s2a.x, 0, s2a.y ), v2 = new Vector3( s2b.x - s2a.x, 0, s2b.y - s2a.y ).normalized;

        if(LineLineIntersection(out inters,p1,v1,p2,v2))
        {
            if(    inters.x >= Mathf.Min( s1a.x, s1b.x ) - 0.01f 
                && inters.x <= Mathf.Max( s1a.x, s1b.x ) + 0.01f
                && inters.z >= Mathf.Min( s1a.y, s1b.y ) - 0.01f
                && inters.z <= Mathf.Max( s1a.y, s1b.y ) + 0.01f 
                && inters.x >= Mathf.Min( s2a.x, s2b.x ) - 0.01f 
                && inters.x <= Mathf.Max( s2a.x, s2b.x ) + 0.01f
                && inters.z >= Mathf.Min( s2a.y, s2b.y ) - 0.01f
                && inters.z <= Mathf.Max( s2a.y, s2b.y ) + 0.01f )
            {
                return true;
            }
            else
                return false;
        }
        else
        {
            Ray ray = new Ray( p1, v1 );
            return ( Vector3.Cross( ray.direction, p2 - ray.origin ).magnitude ) < 0.001f;
        }
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
}