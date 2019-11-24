using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityGraphs;

[ExecuteInEditMode]
// Keep class at top level namespace for ease of use
public class DrawGraph : MonoBehaviour
{
    static readonly string GameObjectName = "DrawGraphMonoBehaviour";

    public static event System.Action<float[]> onData;
    public static event System.Action onInput;

    static void InitEvents()
    {
        if( onInput != null ) return;
        onInput += () => {};
    }

    public Dictionary<string,GraphItem> data;

    void OnEanble()
    {
        if( data == null ) data = new Dictionary<string, GraphItem>();
    }


    static DrawGraph instance;
    public static DrawGraph Singelton()
    {
        if( instance == null ) 
        {
            InitEvents();

            instance = GameObject.Find( GameObjectName )?.GetComponent<DrawGraph>();

            if( instance == null ) 
            {
                var go = new GameObject( GameObjectName );
                
                go.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
                
                instance = go.AddComponent<DrawGraph>();
            }
            
            instance.data = new Dictionary<string, GraphItem>();
        }

        return instance;
    }

    public bool hasData { get { return data != null && data.Count > 0; } }

    static GraphItem ReadOrWrite( string title, float value = float.NaN, Color color = default, string group = "" )
    {
        var data = Singelton().data;

        if( ! data.ContainsKey( title ) ) data.Add( title, new GraphItem( title ) );

        var item = data[ title ];

        if( ! object.Equals( color, default( Color )) ) item._.color = color;

        if( ! string.IsNullOrEmpty( group ) ) item._.group = group;

        if( ! float.IsNaN( value ) ) item._.points.Add( value );

        while( item._.points.Count > item._.max_points ) item._.points.RemoveAt( 0 );

        return item;
    }

    public static List<float> GetData( string title )
    {
        return ReadOrWrite( title )._.points;
    }

    public static List<GraphItem> Add( string title, Quaternion rotation )
    {
        if( ! Singelton().data.ContainsKey( title ) ) 
        {  
            ReadOrWrite( title + "X" ).SetColor( Color.red );
            ReadOrWrite( title + "Y" ).SetColor( Color.green );
            ReadOrWrite( title + "Z" ).SetColor( Color.cyan );
            ReadOrWrite( title + "W" ).SetColor( Color.yellow )

                .SetGraphHeight( 65 );
            
            foreach( var s in new string[] { "X", "Y", "Z", "W" } )
            {
                ReadOrWrite( title + s )
                    .SetGroup( title )
                    .SetStepSize( 0.5f );
            }
        }
        
        var points = new List<GraphItem> {
            ReadOrWrite( title + "X", rotation.x ),
            ReadOrWrite( title + "Y", rotation.y ),
            ReadOrWrite( title + "Z", rotation.z ),
            ReadOrWrite( title + "W", rotation.w )
        };

        onInput?.Invoke();
        onData?.Invoke( new float[] { rotation.x, rotation.y, rotation.z, rotation.w } );

        return points;
    }

    public static List<GraphItem> Add( string title, Vector3 vector )
    {
        if( ! Singelton().data.ContainsKey( title ) ) 
        {  
            ReadOrWrite( title + "X" ).SetGroup( title ).SetColor( Color.red );
            ReadOrWrite( title + "Y" ).SetGroup( title ).SetColor( Color.green );
            ReadOrWrite( title + "Z" ).SetGroup( title ).SetColor( Color.cyan )

                .SetGraphHeight( 65 );
            
            foreach( var s in new string[] { "X", "Y", "Z" } )
            {
                ReadOrWrite( title + s )
                    .SetGroup( title )
                    .SetStepSize( 0.5f );
            }
        }

        var points = new List<GraphItem> {
            ReadOrWrite( title + "X", vector.x ),
            ReadOrWrite( title + "Y", vector.y ),
            ReadOrWrite( title + "Z", vector.z )
        };

        onInput?.Invoke();
        onData?.Invoke( new float[] { vector.x, vector.y, vector.z } );

        return points;
    }

    public static GraphItem Add( string title, float value )
    {
        var output = ReadOrWrite( title, value );

        onInput?.Invoke();
        onData?.Invoke( new float[] { value } );

        return output;
    }

    public static GraphItem Add( string title, float value, Color color = default )
    {
        var output = ReadOrWrite( title, value, color );

        onInput?.Invoke();
        onData?.Invoke( new float[] { value } );

        return output;
    }

    public static GraphItem Get( string title )
    {
        return ReadOrWrite( title );
    }

    // Clear everything
    public static void Clear()
    {
        Singelton().data = new Dictionary<string, GraphItem>();
        
        onInput?.Invoke();
    }

    // Clear group or item
    public static void Clear( string title )
    {
        var data = Singelton().data;

        if( ! data.ContainsKey( title ) ) 
        {
            foreach( var item in data.Values.Where( item => item._.group == title ).ToArray() )
            
                data.Remove( item._.title );
            
            onInput?.Invoke();
            
            return; 
        }
        
        data.Remove( title );

        onInput?.Invoke();
    }
}