
using UnityGraphs;
using System.Linq;
using UnityEngine;

namespace UnityGraphs.Demo
{
    [ExecuteInEditMode]
    public class TestDrawGraph : MonoBehaviour
    {
        public bool updateGraph = false;

        public Rigidbody body;

        void Start()
        {
            DrawGraph.Get("c1").SetGroup("GroupC");
            DrawGraph.Get("c2").SetGroup("GroupC");

            // Supported types: float, Quaternion, Vector3
            DrawGraph.Add( "Vec3Test", Vector3.zero ).ForEach( graph => graph
                .SetLineWidth( 2f )
                .SetGraphHeight( 120f )
                .SetStepSize( 0.25f )
            );
        }

        void Update()
        {
            if( ! updateGraph ) return;

            DrawGraph.Add( "A", Random.value );
            DrawGraph.Add( "B", Random.value );

            DrawGraph.Add( "Vec3Test", new Vector3(
                Mathf.Sin( Time.time / 10 ),
                Mathf.Tan( 1 / Time.time ),
                Mathf.Cos( Time.time / 10 )
            ) );

            DrawGraph.Add( "c1", Mathf.Sin( 1 / Time.time ) );
            DrawGraph.Add( "c2", Mathf.Tan( 1 / Time.time ) );
        }
    }
}