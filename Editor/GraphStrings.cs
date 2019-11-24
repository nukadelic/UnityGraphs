using UnityEditor;
using UnityEngine;

namespace UnityGraphs
{
    public static class GraphStrings
    {
        
        public static GUIContent MenuAutoOpen, MenuDataLength, MenuHeaderValues, MenuClearAll, 
            MenuExpandAll, MenuCollapseAll, MenuDefaultItems;

        public static void MenuStrings()
        {
            if( MenuAutoOpen != null ) return;

            MenuAutoOpen = EditorGUIUtility.TrTextContent( $"Auto Open on {MainFunc}" );
            MenuDataLength = EditorGUIUtility.TrTextContent( "Display graph data length" );
            MenuHeaderValues = EditorGUIUtility.TrTextContent( "Preview values in header" );
            MenuClearAll = EditorGUIUtility.TrTextContent( "Clear All" );
            MenuExpandAll = EditorGUIUtility.TrTextContent( "Expend All" );
            MenuCollapseAll = EditorGUIUtility.TrTextContent( "Collapse All" );
            MenuDefaultItems = EditorGUIUtility.TrTextContent( "Default Items:" );
        }
        
        static readonly string MainFunc = "DrawGraph.Add( ... )";

        public static readonly string NoData = "No data"
         + "\n\n* " + $"Use {MainFunc} to draw graphs"
         + "\n\n* " + "To view values in edit mode, add [ExecuteInEditMode] to MonoBehaviour"; 
        
        public static readonly string AutoOpened = 
        "Window was auto opened, you can disable this in tab menu";

    }
}