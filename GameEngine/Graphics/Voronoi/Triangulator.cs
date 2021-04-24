using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Linq;

namespace Graphics.DelaunayVoronoi
{
    class Voronoi
    {
        private Vector2 Min;
        private Vector2 Max;
        private List<Point> Points;
        private List<Triangle> Triangles;
        private List<Cell> Cells;
        private List<Point> Border;
        
        
        public Rectangle Size
        {
            get;
        }

        public List<Point> VoronoiCells
        {
            get;
        }
        public List<Triangle> DelaunayTriangles
        {
            get;
        }


        public Voronoi(Vector2[] Points, Vector2 min, Vector2 max)
        {
            Min = min;
            Max = max;

                       

        }

        private void BowerWatson()
        {

        }
        private HashSet<Triangle> FindBadTriangles(Point Point) => new HashSet<Triangle>(Triangles.Where(that => that.IsPointInsideCircumcircle(Point)));
    }
}
