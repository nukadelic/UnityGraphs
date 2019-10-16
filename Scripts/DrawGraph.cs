using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityGraphs
{
    [ExecuteInEditMode]
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

        [System.Serializable]
        public class AbstractedData
        {
            public float overflow_ratio = 0.5f;
            public float line_width = 1.5f;
            public string group = "";
            public string title = "";
            public List<float> points = new List<float>();
            public Color color = default;
            public float scroll = 0;
            public float step_size = 5f;
            public bool visible = true;   
            public float graph_height = 50;    
            public int max_points = 1000;
        }

        [System.Serializable]
        public class Points
        {
            /// Variables data. Avoid using this when possible
            public AbstractedData _;
            /// Must declare title 
            public Points( string title ) { this._ = new AbstractedData { title = title }; }
            /// Is this item part of a group
            public bool inGroup { get { return ! string.IsNullOrEmpty( _.group ); } }
            /// Clear group from this item
            public Points RemoveFromGroup() { _.group = ""; return this; }
            public Points SetColor( Color c ) { _.color = c; return this; }
            /// Use this to combine multiple items in the same graph
            public Points SetGroup( string s ) { _.group = s; return this; }
            /// Set minimum step size
            public Points SetStepSize( float f ) { _.step_size = f; return this; }
            /// Line thickness
            public Points SetLineWidth( float f ) { _.line_width = f; return this; }
            /// Set single item render height, or in a group use the max value 
            public Points SetGraphHeight( float f ) { _.graph_height = f; return this; }
            /// If the limit is reached the graph will pop the first element to avoid exceeding
            public Points SetLengthLimit( int i ) { _.max_points = i; return this; }
        }

        public Dictionary<string,Points> data;

        void OnEanble()
        {
            if( data == null ) data = new Dictionary<string, Points>();
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
                
                instance.data = new Dictionary<string, Points>();
            }

            return instance;
        }

        static Points ReadOrWrite( string title, float value = float.NaN, Color color = default, string group = "" )
        {
            var data = Singelton().data;

            if( ! data.ContainsKey( title ) ) data.Add( title, new Points( title ) );

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

        // TODO: support for Vecto3, Quaternion, ... set Title+="X"/"Y"... and combine in the same group

        public static List<Points> Add( string title, Quaternion rotation )
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
            
            var points = new List<Points> {
                ReadOrWrite( title + "X", rotation.x ),
                ReadOrWrite( title + "Y", rotation.y ),
                ReadOrWrite( title + "Z", rotation.z ),
                ReadOrWrite( title + "W", rotation.w )
            };

            onInput?.Invoke();
            onData?.Invoke( new float[] { rotation.x, rotation.y, rotation.z, rotation.w } );

            return points;
        }

        public static List<Points> Add( string title, Vector3 vector )
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

            var points = new List<Points> {
                ReadOrWrite( title + "X", vector.x ),
                ReadOrWrite( title + "Y", vector.y ),
                ReadOrWrite( title + "Z", vector.z )
            };

            onInput?.Invoke();
            onData?.Invoke( new float[] { vector.x, vector.y, vector.z } );

            return points;
        }

        public static Points Add( string title, float value )
        {
            var output = ReadOrWrite( title, value );

            onInput?.Invoke();
            onData?.Invoke( new float[] { value } );

            return output;
        }

        public static Points Add( string title, float value, Color color = default )
        {
            var output = ReadOrWrite( title, value, color );

            onInput?.Invoke();
            onData?.Invoke( new float[] { value } );

            return output;
        }

        public static Points Get( string title )
        {
            return ReadOrWrite( title );
        }

        public static void Clear()
        {
            Singelton().data = new Dictionary<string, Points>();
            
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
}