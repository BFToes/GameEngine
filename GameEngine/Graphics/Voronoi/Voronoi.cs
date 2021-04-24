using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
namespace Graphics.DelaunayVoronoi
{
    class Point
    {
        public HashSet<Triangle> AdjacentTriangles = new HashSet<Triangle>();
        public readonly Vector2 Position;

        public float X => Position.X;
        public float Y => Position.Y;
            
        public Point(float x, float y) => Position = new Vector2(x, y);
        public Point(Vector2 position) => Position = position;
    }

    class Edge 
    {
        public Point P1;
        public Point P2;
        public Edge(Point point1, Point point2)
        {
            P1 = point1;
            P2 = point2;
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj.GetType() != GetType()) return false;
            Edge edge = obj as Edge;
            
            return (P1 == edge.P1 && P2 == edge.P2) || (P1 == edge.P2 && P2 == edge.P1);
        }
        public override int GetHashCode() => ((int)P1.X ^ (int)P1.Y ^ (int)P2.X ^ (int)P2.Y).GetHashCode();
    }
       

    class Triangle
    {
        public Point[] Vertices = new Point[3];
        public Point Circumcenter { get; private set; }
        private float RadiusSquared;
        public IEnumerable<Triangle> TrianglesWithSharedEdge
        {
            get
            {
                HashSet<Triangle> Neighbors = new HashSet<Triangle>();

                foreach (Point P in Vertices)
                {
                    var Tris = P.AdjacentTriangles.Where(that => that != this && this.SharesEdgeWith(that));
                    Neighbors.UnionWith(Tris);
                }

                return Neighbors;
            }
        }

        public Triangle(Point point1, Point point2, Point point3)
        {
            if (point1 == point2 || point1 == point3 || point2 == point3) throw new Exception("Must be 3 distinct points");

            if (!IsCounterClockwise(point1, point2, point3))
            {
                Vertices[0] = point1;
                Vertices[1] = point3;
                Vertices[2] = point2;
            }
            else
            {
                Vertices[0] = point1;
                Vertices[1] = point2;
                Vertices[2] = point3;
            }

            Vertices[0].AdjacentTriangles.Add(this);
            Vertices[1].AdjacentTriangles.Add(this);
            Vertices[2].AdjacentTriangles.Add(this);

            UpdateCircumcircle();
        }
        private void UpdateCircumcircle()
        {
            Vector2 p1 = Vertices[0].Position;
            Vector2 p2 = Vertices[1].Position;
            Vector2 p3 = Vertices[2].Position;

            float A = p1.X * p1.X + p1.Y * p1.Y;
            float B = p2.X * p2.X + p2.Y * p2.Y;
            float C = p3.X * p3.X + p3.Y * p3.Y;

            float auxX = A * (p3.Y - p2.Y) + B * (p1.Y - p3.Y) + C * (p2.Y - p1.Y);
            float auxY = -(A * (p3.X - p2.X) + B * (p1.X - p3.X) + C * (p2.X - p1.X));
            float div = (2 * (p1.X * (p3.Y - p2.Y) + p2.X * (p1.Y - p3.Y) + p3.X * (p2.Y - p1.Y)));

            if (div == 0) throw new DivideByZeroException();

            Circumcenter = new Point(auxX / div, auxY / div);
            RadiusSquared = (Circumcenter.X - p1.X) * (Circumcenter.X - p1.X) + (Circumcenter.Y - p1.Y) * (Circumcenter.Y - p1.Y);
        }
        public bool SharesEdgeWith(Triangle triangle) => Vertices.Where(o => triangle.Vertices.Contains(o)).Count() == 2;
        public bool IsPointInsideCircumcircle(Point point) => (point.X - Circumcenter.X) * (point.X - Circumcenter.X) + (point.Y - Circumcenter.Y) * (point.Y - Circumcenter.Y) < RadiusSquared;
        private bool IsCounterClockwise(Point point1, Point point2, Point point3) => (point2.X - point1.X) * (point3.Y - point1.Y) - (point3.X - point1.X) * (point2.Y - point1.Y) > 0;

    }
    class Cell
    {
        public readonly Point Center;
        public readonly Point[] Vertices;
        public IEnumerable<Cell> CellsWithSharedEdge 
        {
            get;
        }
        public Cell(List<Point> Vertices, Point Center)
        {
            this.Center = Center;
            this.Vertices = ConvexHull(Vertices).ToArray();
        }
        private float cross(Point O, Point A, Point B) => (A.X - O.X) * (B.Y - O.Y) - (A.Y - O.Y) * (B.X - O.X);
        private List<Point> ConvexHull(List<Point> points)
        {
            if (points.Count() <= 1) throw new Exception("not enough points to create convex hull.");

            int n = points.Count(), k = 0;
            List<Point> H = new List<Point>(new Point[2 * n]);

            points.Sort((a, b) => a.X == b.X ? a.Y.CompareTo(b.Y) : a.X.CompareTo(b.X));

            // Build lower hull
            for (int i = 0; i < n; ++i)
            {
                while (k >= 2 && cross(H[k - 2], H[k - 1], points[i]) <= 0) k--;
                H[k++] = points[i];
            }

            // Build upper hull
            for (int i = n - 2, t = k + 1; i >= 0; i--)
            {
                while (k >= t && cross(H[k - 2], H[k - 1], points[i]) <= 0) k--;
                H[k++] = points[i];
            }

            return H.Take(k - 1).ToList();
        }

        private bool SharedEdgeWith(Cell Cell) => Vertices.Where(o => Cell.Vertices.Contains(o)).Count() == 2;

    }
}
