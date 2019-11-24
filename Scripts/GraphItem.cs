using UnityEngine;

namespace UnityGraphs
{
    [System.Serializable]
    public class GraphItem
    {
        /// Variables data. Avoid using this when possible
        public ItemData _;
        /// Must declare title 
        public GraphItem( string title ) { this._ = new ItemData { title = title }; }
        /// Is this item part of a group
        public bool inGroup { get { return ! string.IsNullOrEmpty( _.group ); } }
        /// Clear group from this item
        public GraphItem RemoveFromGroup() { _.group = ""; return this; }
        public GraphItem SetColor( Color c ) { _.color = c; return this; }
        /// Use this to combine multiple items in the same graph
        public GraphItem SetGroup( string s ) { _.group = s; return this; }
        /// Set minimum step size
        public GraphItem SetStepSize( float f ) { _.step_size = f; return this; }
        /// Line thickness
        public GraphItem SetLineWidth( float f ) { _.line_width = f; return this; }
        /// Set single item render height, or in a group use the max value 
        public GraphItem SetGraphHeight( float f ) { _.graph_height = f; return this; }
        /// If the limit is reached the graph will pop the first element to avoid exceeding
        public GraphItem SetLengthLimit( int i ) { _.max_points = i; return this; }
        /// Limit graph min & max values 
        public GraphItem SetLimits( float min, float max ) { _.limits = new Vector2( min, max ); return this; }
    }
}