using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityGraphs
{
    [InitializeOnLoad]
    class DrawGraphHook
    {
        static DrawGraphHook () 
        { 
            EditorApplication.update += () => DrawGraph.onInput += delegate 
            {
                if( ! GraphWindow.GetData().autoOpen ) return;

                if( GraphWindow.instance == null )
                {
                    if( ! GraphWindow.DismissAutoOpen && GraphWindow.GetData().autoOpenCounter < 5 )
                    {
                        GraphWindow.GetData().autoOpenCounter ++ ;

                        GraphWindow.WasAutoOpen = true;
                    }

                    GraphWindow.OpenWindow();
                }
            };
        }
    }

    public class GraphWindow : EditorWindow, IHasCustomMenu
    {
        static string TITLE = "Graph Window";

        public static GraphWindow instance;

        [MenuItem("Window/Analysis/Graph Window")]
        public static void OpenWindow()
        {
            instance = GetWindow<GraphWindow>( false, TITLE, true );
            instance.minSize = new Vector2( 320, 220 );
        }

        public static bool WasAutoOpen = false;
        public static bool DismissAutoOpen = false;

        public void AddItemsToMenu( GenericMenu menu )
        {
            menu.AddItem(EditorGUIUtility.TrTextContent( "Auto Open on DrawGraph.Add(...)" ),
                data.autoOpen, () => { data.autoOpen = ! data.autoOpen; SaveData(); } );

            menu.AddItem(EditorGUIUtility.TrTextContent( "Display graph data length" ),
                data.showCounts, () => { data.showCounts = ! data.showCounts; SaveData(); } );

            menu.AddItem(EditorGUIUtility.TrTextContent( "Preview values in header" ),
                data.showValues, ()=> { data.showValues = ! data.showValues; SaveData(); } );

            menu.AddSeparator("");

            menu.AddItem(EditorGUIUtility.TrTextContent("Expend All"), false, () => 
                DrawGraph.Singelton().data.Values.ToList().ForEach( item => item._.visible = true ) );

            menu.AddItem(EditorGUIUtility.TrTextContent("Collapse All"), false, () =>
                DrawGraph.Singelton().data.Values.ToList().ForEach( item => item._.visible = false ) );
        }

        ////////////////////////////////////
        //
        //  Editor preference
        //

        [Serializable]
        public class GraphWindowEditorData
        {
            public Vector2 scroll = new Vector2();    
            public bool autoOpen = true;   
            public int autoOpenCounter = 0;      
            public bool autoFocus = false;
            public bool showCounts = false;
            public bool showValues = true;
        }

        static GraphWindowEditorData data;

        public static GraphWindowEditorData GetData() 
        {
            if( data == null ) LoadData();

            return data; 
        }

        static string GetDataKey() { return "GraphWindowEditorData_EditorPrefsKey"; }

        static void LoadData()
        {
            data = new GraphWindowEditorData();

            var json = EditorPrefs.GetString( GetDataKey() , "");

            if ( json != "") data = JsonUtility.FromJson<GraphWindowEditorData>( json );

            else SaveData();
        }

        static void SaveData()
        {
            var json = JsonUtility.ToJson( data );
            EditorPrefs.SetString( GetDataKey(), json );
        }
        
        //////////////////////////////////////
        //
        //  Event Cycle
        //

        bool focused = false;
        
        void OnFocus() { focused = true; }
        
        void OnLostFocus() { focused = false; }

        void OnEnable()
        {
            LoadData();

            DrawGraph.onInput += OnInput;            
            // DrawGraph.onData += OnData;
        }

        void OnDisable()
        {
            SaveData();

            DrawGraph.Clear();

            DrawGraph.onInput -= OnInput;            
            // DrawGraph.onData -= OnData;
        }

        public bool needsRepaint = false;

        // void OnData( float[] data )
        // {
        //     Debug.Log("Data input: " + string.Join( ",", data ) );
        // }

        void OnInput()
        {
            needsRepaint = true;

            if( data.autoFocus && ! focused ) Focus();
        }

        int repaintTimer = 0;

        void Update() 
        {
            if( needsRepaint || ( focused && ++ repaintTimer > 5 ) )
            {
                needsRepaint = false;
                repaintTimer = 0;
                Repaint();
            }
        }

        // void OnInspectorUpdate() { Repaint(); }
        // void OnSceneGUI() { }

        /// https://unitylist.com/p/5c3/Unity-editor-icons
        GUIContent IconGUI( string name, bool darkSkinSupported = true )
        {
            string prefix = darkSkinSupported ? ( EditorGUIUtility.isProSkin ? "d_" : "" ) : "";
            return new GUIContent( EditorGUIUtility.IconContent( prefix + name ) );
        }
        
        void OnGUI()
        {
            if( DrawGraph.Singelton().data.Count == 0 ) 
            {
                EditorGUILayout.HelpBox( "No data", MessageType.Info );
                return;
            }

            if( WasAutoOpen && ! DismissAutoOpen )
            {
                using( new GUILayout.HorizontalScope() )
                {
                    EditorGUILayout.HelpBox( "Window was auto opened, you can disable this in tab menu", MessageType.Info );

                    if( GUILayout.Button( IconGUI("LookDevClose") , GUILayout.Width( 30 ),  GUILayout.Height( 40 ) ) ) 
                    
                        DismissAutoOpen = true;

                }
            }
            
            float ppp = EditorGUIUtility.pixelsPerPoint;
            bool isWide = Screen.width / ppp > 400;

            using( var scope = new GUILayout.ScrollViewScope( data.scroll ) )
            {
                GUILayout.Space( 10 );
                
                data.scroll = scope.scrollPosition;

                var groups = DrawGraph.Singelton().data._GetDataWGroups();
                var groups_count = groups.Count;

                for( var i = 0; i < groups_count; ++i )
                {
                    if( isWide && groups_count > 1 && ! ( i == groups_count - 1 && groups_count % 2 == 1 ) )
                    {
                        if( i % 2 == 0 ) GUILayout.BeginHorizontal();

                        GraphGroupGUI( groups[ i ] , Screen.width / ppp / 2 - 5 );   

                        if( i % 2 != 0 ) GUILayout.EndHorizontal();
                    }

                    else GraphGroupGUI( groups[ i ] , Screen.width );
                }

                var graphs = DrawGraph.Singelton().data._GetDataWoGroups();
                var graphs_count = graphs.Count;

                for( var i = 0; i < graphs_count; ++i )
                {
                    if( isWide && graphs_count > 1 && ! ( i == graphs_count - 1 && graphs_count % 2 == 1 ) )
                    {
                        if( i % 2 == 0 ) GUILayout.BeginHorizontal();

                        GraphGUI( graphs[ i ], Screen.width / ppp / 2 - 5 );   

                        if( i % 2 != 0 ) GUILayout.EndHorizontal();
                    }

                    else GraphGUI( graphs[ i ], Screen.width );
                }
                
                GUILayout.Space( 20 );
            } 
        }

        void GraphGUI( DrawGraph.Points graph, float width )
        {
            var foldoutHeader = new GUIStyle( EditorStyles.foldoutHeader );
            // foldoutHeader.fixedWidth = width - 10;
            // foldoutHeader.fixedWidth = Screen.width / EditorGUIUtility.pixelsPerPoint - 25;

            var graphContianerLayout =  GUILayout.MaxWidth( Screen.width );

            var group = new List<DrawGraph.Points> { graph } ;
            
            using( new GUILayout.VerticalScope( GUILayout.MaxWidth( width ) ) )
            {
                var header = graph._.title;

                if( data.showValues ) header += " " + graph._GetLastValues();
                if( data.showCounts ) header += " " + graph._GetCounts();

                graph._.visible = EditorGUILayout.BeginFoldoutHeaderGroup( graph._.visible, header, foldoutHeader );
                EditorGUILayout.EndFoldoutHeaderGroup();
                
                if( graph._.visible ) // using( new GUILayout.HorizontalScope( graphContianerLayout ) )

                    GraphRenderer.RenderGUI( group );
                
                else GUILayout.Space( 4 );
            }
        }

        void GraphGroupGUI( List<DrawGraph.Points> group, float width )
        {
            var foldoutHeader = new GUIStyle( EditorStyles.foldoutHeader );
            // foldoutHeader.fixedWidth = width - 10;
            // foldoutHeader.fixedWidth = Screen.width / EditorGUIUtility.pixelsPerPoint - 25;
            
            var graphContianerLayout =  GUILayout.MaxWidth( Screen.width );
            
            using( new GUILayout.VerticalScope( GUILayout.MaxWidth( width ) ) )
            {    
                var header = group[ 0 ]._.group;

                if( data.showCounts ) header += " " + group._GetCounts();
                if( data.showValues ) header += " " + group._GetLastValues();

                var visible = group._IsVisible();

                visible = EditorGUILayout.BeginFoldoutHeaderGroup( visible, header, foldoutHeader );
                EditorGUILayout.EndFoldoutHeaderGroup();
                group._SetVisible( visible );
                
                if(  visible ) // using( new GUILayout.HorizontalScope( graphContianerLayout ) )
                
                    GraphRenderer.RenderGUI( group );

                else GUILayout.Space( 4 );
            }
        }
    }
}