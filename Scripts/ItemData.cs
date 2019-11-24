using UnityEngine;
using System.Collections.Generic;

namespace UnityGraphs
{
    [System.Serializable]
    public class ItemData
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
        public Vector2 limits = Vector3.zero;
    }
}
