
using UnityGraphs;
using System.Linq;
using UnityEngine;

namespace UnityGraphs.Demo
{
    // Graphs can be called while play mode is off, by updating the MonoBehaviour transform for example 
    // or within another editor script or a window
    [ExecuteInEditMode]
    
    public class DemoDrawGraph : MonoBehaviour
    {
        public bool updateGraph = true;

        void Start()
        {
            // Supported types: float, Quaternion, Vector3
            DrawGraph.Add( "Vec3Test", Vector3.zero )
                // Use System.Linq::ForEach( ... ) to assing group style values
                .ForEach( graph => graph
                    .SetLineWidth( 2f )
                    .SetGraphHeight( 120f )
                    .SetStepSize( 0.25f )
                    .SetLimits( -2f, 2f )
                );

            DrawGraph.Add( "Empty graph test", 0f );

            //Combine two graphs in the same view 
            DrawGraph.Get("c1").SetGroup("GroupC");
            DrawGraph.Get("c2").SetGroup("GroupC");

        }

        void Update()
        {
            if( ! updateGraph ) return;

            // Add values to be drawn in the graph
            // ( seperate graphs, not in a group )
            // Since no settings are assigned, default style
            // values are used for A & B
            DrawGraph.Add( "A", Random.value ); 
            DrawGraph.Add( "B", Random.value );

            // Automaticlly create a group for Vector3 and 
            // assign each of the x,y,z componenets into the
            // same group
            DrawGraph.Add( "Vec3Test", new Vector3(
                Mathf.Sin( Time.time / 10 ),
                Mathf.Tan( 1 / Time.time ),
                Mathf.Cos( Time.time / 10 )
            ) );

            // c1 & c2 will be drawn in the same group
            DrawGraph.Add( "c1", Mathf.Sin( 1 / Time.time ) );

            // If you are feeling particulary lazy, style values 
            // can be assined during update
            DrawGraph.Add( "Random red", Random.value ).SetLengthLimit( 250 )
                .SetColor( Color.red )
                .SetLineWidth( 4f )
                .SetStepSize( 2f );

            // Grouped items don't need to be updated in the same place or time
            if( Random.value > 0.7 )
            {
                // c1 & c2 will be drawn in the same group
                DrawGraph.Add( "c2", Mathf.Tan( 1 / Time.time ) );
            }
        }
    }
}