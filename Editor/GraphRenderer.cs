using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UnityGraphs
{
    public static class GraphRenderer
    {
        //////////////////////////////////
        //
        //  Graph GUI wrappers 
        //

        public static void RenderGUI( List<GraphItem> group )
        {
            float height = group._GetHeight();

            using( new EditorGUI.DisabledGroupScope( true ) )
            {
                GUILayout.TextArea( "", GUILayout.Height( height ) );
            }

            Rect area = GUILayoutUtility.GetLastRect();

            if( Event.current.type == EventType.Repaint )

                RenderGraphGroup( area, group );

            var mouse_area = new Rect( area.x, area.y + area.height / 2, area.width, area.height + 30 );

            var mouse_over = mouse_area.Contains( Event.current.mousePosition );
            
            var scroll = group.Select( data => data._.scroll ).Aggregate( (a,b) => a > b ? a : b );

            var ratio = group.Select( data => data._.overflow_ratio ).Aggregate( (a,b) => a < b ? a : b );

            if( mouse_over && scroll > -1 ) scroll = GraphScrollbarGUI( area, scroll, ratio );

            group.ForEach( data => data._.scroll = scroll );
            
            GUILayout.Space( 14 );
        }

        public static float GraphScrollbarGUI( Rect area, float scroll, float ratio = 0.5f )
        {
            return GUI.HorizontalScrollbar( new Rect( area.x + 1, area.yMax, area.width - 2, 16 ), scroll, ratio, 0, 1 );
        }

        /////////////////////////
        //
        //  Render Handles 
        //

        /// TODO: add method to override graph light / dark style
        static bool IsDark { get { return true; }} // EditorGUIUtility.isProSkin; } }

        static float GridSize = 10f;

        static Color GetAxisColor() { return IsDark ? new Color( 1, 1, 1, 0.5f ) : new Color( 0, 0, 0, 0.5f ); }
        static Color GetGridColor() { return IsDark ? new Color( 1, 1, 1, 0.2f ) : new Color( 0, 0, 0, 0.2f ); }

        public static void RenderLabel( string label, float x, float y, Color color, TextAnchor anchor = TextAnchor.UpperLeft, bool background = false, int fontSize = -1 )
        {
            RenderLabel( label, x, y, "#" + ColorUtility.ToHtmlStringRGBA( color ), anchor, background, fontSize );
        }
        
        public static void RenderLabel( string label, float x, float y, string cssColor = "", TextAnchor anchor = TextAnchor.UpperLeft, bool background = false, int fontSize = -1 )
        {
            GUIStyle style = new GUIStyle( GUI.skin.label );
            Color color;

            if( ! string.IsNullOrEmpty( cssColor ) && ColorUtility.TryParseHtmlString( cssColor, out color ) )
            {
                style.normal.textColor = color;
            }

            style.alignment = anchor;

            if( fontSize > -1 ) style.fontSize = fontSize;

            if( background && ColorUtility.TryParseHtmlString( IsDark ? "#00000022" : "#FFFFFF22", out color ) )
            {
                var size = style.CalcSize( new GUIContent( label ) );

                Rect rect = new Rect( x - 4, y - 1, size.x + 8, size.y + 1 );
                
                Handles.color = color;

                Handles.DrawSolidRectangleWithOutline( rect, Color.white, Color.clear );
            }
            			
            Handles.Label( new Vector3( x, y, 0 ), label, style );
        }

        public static void RenderLine( float x1, float y1, float x2, float y2, Color c )
        {
            Handles.color = c;
            Handles.DrawLine( new Vector3( x1, y1, 0 ) , new Vector3( x2, y2, 0 ) );
        }

        public static void RenderLines( float width, Color c, Vector3[] points ) 
        {
            Handles.color = c;
            Handles.DrawAAPolyLine( width, points );
        }
        
        public static void RenderGraphData( Rect area, GraphItem data )
        {
            RenderGraphGroup( area, new List<GraphItem> { data } );
        }

        public static void RenderGraphGroup( Rect area, List<GraphItem> data )
        {
            Vector2 mouse = Event.current.mousePosition;
            float DPI = EditorGUIUtility.pixelsPerPoint;

            data.ForEach( d => d._.color = d._.color == default( Color ) ? ( IsDark ? Color.white : Color.black ) : d._.color );

			Handles.BeginGUI();

            Handles.color = IsDark ? Color.black : Color.white;
            Handles.DrawSolidRectangleWithOutline( area, Handles.color, Color.grey );

            float grid_offset = - Mathf.Clamp01( data[ 0 ]._.scroll ) * area.width;
            grid_offset = grid_offset % GridSize;
            if( grid_offset < 0 ) grid_offset += GridSize;

            string group = data[ 0 ]._.group;

            for( var x = area.x + grid_offset; x < area.xMax; x += GridSize )
                RenderLine( x, area.y, x, area.yMax, GetGridColor() );

            for( var y = area.y; y < area.yMax; y += GridSize )
                RenderLine( area.x, y, area.xMax, y, GetGridColor() );

            bool mouseInBounds = area.Contains( mouse );

            if( mouseInBounds ) RenderLine( mouse.x, area.y, mouse.x, area.yMax, GetAxisColor() );

            for( var i = 0; i < data.Count; ++ i )
            {
                // poker face variables 
                int totalPoints = data[ i ]._.points.Count;
                float step = area.width / totalPoints;
                step = step < data[ i ]._.step_size ? data[ i ]._.step_size : step;
                var data_scroll = data[ i ]._.scroll < 0 ? 0 : data[ i ]._.scroll;
                int visiblePoints = Mathf.FloorToInt( area.width / step );
                float scroll_space = ( totalPoints - 1 ) * step - area.width;
                float position = scroll_space * data_scroll;
                float delta_space = position % step;
                bool overflow = scroll_space > 0;
                
                // Default starting indexes for [ j ] loop
                int startIndex = 0;
                int lastIndexPlus1 = totalPoints;
                
                // If graph bigger then the draw area, adjust the indexes
                if( overflow )
                {
                    // Enable scrolling & auto scroll to the end 
                    if( data[ i ]._.scroll < 0 ) data[ i ]._.scroll = 1;
                }
                // If there is no overflow, disable scrolling
                else data[ i ]._.scroll = -1;

                startIndex = Mathf.RoundToInt( ( totalPoints - visiblePoints ) * data_scroll );
                lastIndexPlus1 = visiblePoints + startIndex;

                var visible_points = data[ i ]._.points.GetRange( startIndex, visiblePoints );

                // quick fix ? 
                if( visible_points.Count == 0 ) visible_points = data[ i ]._.points;

                // Calculate point limits
                float maxValue = 1f;
                float minValue = 0f;

                if( visible_points.Count > 2 )
                {
                    maxValue = visible_points.Aggregate( (a,b) => a > b ? a : b );
                    minValue = visible_points.Aggregate( (a,b) => a < b ? a : b );
                }
                
                if( data[ i ]._.limits != Vector2.zero )
                {
                    minValue = data[ i ]._.limits.x;
                    maxValue = data[ i ]._.limits.y;
                }

                int mouseIndex = -1;

                if( mouseInBounds )
                {
                    float mouse_shift = position;

                    float relative_mouse = mouse.x + ( overflow ? mouse_shift : 0 );

                    mouseIndex = Mathf.RoundToInt( ( relative_mouse - area.x ) / step );
                }

                bool draw_hover = false;
                float hover_value = 0f;
                Vector2 point = Vector2.zero;

                var points = new List<Vector3>();
                
                for( var j = startIndex; j < lastIndexPlus1; ++ j )
                {   
                    int index_shift = overflow ? startIndex : 0;
                    float x_shift = overflow ? - delta_space : 0;
                    float value = data[ i ]._.points[ j ];

                    /// Normalize 
                    float x = ( j - index_shift ) * step;
                    float y = 1 - ( value - minValue ) / ( maxValue - minValue );

                    // Prevent render overflow
                    y = Mathf.Clamp01( y );

                    /// Relative to draw area 
                    x += area.x + x_shift;
                    y = y * area.height + area.y;


                    points.Add( new Vector3( x, y ) );
                    
                    /// Display point data at mouse X
                    if( mouseInBounds && mouseIndex == j )
                    {
                        point = new Vector2( x, y );
                        draw_hover = true;
                        hover_value = value;
                    } 
                }

                RenderLines( data[ i ]._.line_width, data[ i ]._.color, points.ToArray() );

                // Hover values and display their values
                if( draw_hover )
                {
                    Handles.color = data[ i ]._.color;
                    Handles.DrawSolidDisc( new Vector3( point.x,point.y,0 ), Vector3.forward, 5 );
                    
                    string s = hover_value.ToString("N3");
                    RenderLabel( s, area.xMax - 9 * s.Length, area.y + 5  + 15 * i, data[ i ]._.color );
                }
                // Display latest values on multiple items graph
                else if( ! draw_hover && data.Count > 1 )
                {

                }
                // Display limits if there is only single item in graph ( groups not supported )
                else if( data.Count == 1 && ! mouseInBounds )
                {
                    string s1 = maxValue.ToString("N3");
			        RenderLabel( s1, area.xMax - 9 * s1.Length, area.y + 5, GetAxisColor() );   
                    string s2 = minValue.ToString("N3");
			        RenderLabel( s2, area.xMax - 9 * s2.Length, area.yMax - 20, GetAxisColor() );
                }
            }

            if( data.Count > 1 && data[ 0 ].inGroup ) for( var i = 0; i < data.Count; ++ i )
            {
                string title = data[ i ]._.title;

                if( title.IndexOf( group ) == 0 && title != group )
                
                    title = title.Remove( 0, group.Length );

                RenderLabel( title, area.x + 6f , area.y + 3f + i * 14, data[ i ]._.color );
            }

			Handles.EndGUI();
        }
    }
}