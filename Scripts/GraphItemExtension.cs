using System.Collections.Generic;
using System.Linq;

namespace UnityGraphs
{
    public static class GraphItemExtension 
    {
        public static bool _IsVisible( this List<GraphItem> group )
        {
            return group.Where( item => item._.visible ).Count() > 0;
        }
        
        public static void _SetVisible( this List<GraphItem> group, bool value )
        {
            group.ForEach( item => item._.visible = value );
        }

        public static float _GetHeight( this List<GraphItem> group )
        {
            return group.Select( d => d._.graph_height ).Aggregate( (a,b) => a > b ? a : b );
        }

        public static string _GetCounts( this List<GraphItem> group )
        {
            return "(" + string.Join( ", ", group.Select( item => item._.points.Count ) ) + ")";
        }

        public static string _GetLastValues( this List<GraphItem> group )
        {
            return "(" + string.Join( ", ", group.Select( graph => graph._.points.LastOrDefault().ToString("N2") ) ) + ")";
        }

        public static string _GetLastValues( this GraphItem graph )
        {
            return "(" + graph._.points.LastOrDefault().ToString("N2") + ")";
        }

        public static string _GetCounts( this GraphItem graph )
        {
            return "(" + graph._.points.Count + ")";
        }
        
        /// Get list of groups of data items 
        public static List<List<GraphItem>> _GetDataWGroups( this Dictionary<string,GraphItem> data )
        {
            return data
                .Where( kv => kv.Value.inGroup )
                .GroupBy( kv => kv.Value._.group )
                .ToDictionary( 
                    grp => grp.Key, 
                    grp => grp.ToList() )
                .Select( grp => grp.Value
                    .Select( kv => kv.Value )
                    .ToList() )
                .ToList();
        }

        /// Get data items without a group 
        public static List<GraphItem> _GetDataWoGroups( this Dictionary<string,GraphItem> data )
        {
            return data.Where( kv => ! kv.Value.inGroup ).Select( kv => kv.Value ).ToList();
        }
    }
}