using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ListExtensions;

namespace Graphics.Delaunator
{
    /* Thing To do
     * voronoi cells
     * comments -> attempts to understand
     * 
     * 
     */
    
    public class Triangulator
    {
        
        public List<float> Positions;
        public List<int> TriangleIndexes;
        public List<int> halfedges;
        public List<int> ConvexHullIndexes;

        private List<int> hullPrev;
        private List<int> hullNext;
        private List<int> hullTri;
        private List<int> EdgeStack;
        private int _hashSize;
        private float _cx;
        private float _cy;
        private int hullStart;
        private int trianglesLen;



        /*
        public List<T1> ToVoronoi<T1>(Func<float, float, T1> PointPacker) 
        {
            List<T1> Voronois = new List<T1>();

        }
        */

        #region Custom Constructors
        public static Triangulator From<T>(List<T> points, Func<T, float> xExtractor, Func<T, float> yExtractor)
        {
            int n = points.Count;
            var coords = new List<float>(n * 2);
            for (int i = 0; i < n; i++)
            {
                var p = points[i];
                coords.Add(xExtractor(p));
                coords.Add(yExtractor(p));
            }
            return new Triangulator(coords);
        }

        public T1[] ToTriangles<T1>(Func<float, float, T1> PointPacker)
        {
            //triangles.ForEach((i) => Console.WriteLine($"X: {coords[i]} Y: {coords[i + 1]}"));
            List<T1> Tris = new List<T1>();
            for (int i = TriangleIndexes.Count - 1; i >= 0; i--) // reverse winding to make forward facing in openGl
            {
                Tris.Add(PointPacker(Positions[TriangleIndexes[i] * 2], Positions[TriangleIndexes[i] * 2 + 1]));
            }
            return Tris.ToArray();
        }
        #endregion
        public Triangulator(List<float> coords)
        {
            EdgeStack = new List<int>(512).Fill();
            int n = coords.Count >> 1;
            this.Positions = coords;

            #region arrays that will store the triangulation graph
            var maxTriangles = 2 * n - 5;
            var triangles = this.TriangleIndexes = new List<int>(maxTriangles * 3);
            var halfedges = this.halfedges = new List<int>(maxTriangles * 3);
            #endregion

            #region temporary arrays for tracking the edges of the advancing convex hull
            this._hashSize = (int)MathF.Ceiling(MathF.Sqrt(n));
            var hullPrev = this.hullPrev = new List<int>(n); // edge to prev edge
            var hullNext = this.hullNext = new List<int>(n); // edge to next edge
            var hullTri = this.hullTri = new List<int>(n); // edge to adjacent triangle
            var hullHash = new List<int>(this._hashSize).Fill(-1); // angular edge hash
            #endregion

            #region populate an array of point indices; calculate input data box
            var IDs = new List<int>(n);
            float minX = float.PositiveInfinity; float minY = float.PositiveInfinity;
            float maxX = float.NegativeInfinity; float maxY = float.NegativeInfinity;

            for (int i = 0; i < n; i++)
            {
                float x = coords[2 * i]; float y = coords[2 * i + 1];
                if (x < minX) minX = x;
                if (y < minY) minY = y;
                if (x > maxX) maxX = x;
                if (y > maxY) maxY = y;

                IDs.Add(i); //ids.SetSafely(i, i); //ids[i] = i;
            }
            float cx = (minX + maxX) / 2f;
            float cy = (minY + maxY) / 2f;

            float minDist = float.PositiveInfinity;
            int i0 = 0, i1 = 0, i2 = 0;
            #endregion

            #region pick a seed point close to the center
            for (int i = 0; i < n; i++)
            {
                float d = Distance(cx, cy, coords[2 * i], coords[2 * i + 1]);
                if (d < minDist)
                {
                    i0 = i;
                    minDist = d;
                }
            }
            float i0x = coords[2 * i0];
            float i0y = coords[2 * i0 + 1];

            minDist = float.PositiveInfinity;
            #endregion

            #region find the point closest to the seed
            for (int i = 0; i < n; i++)
            {
                if (i == i0) continue;

                float d = Distance(i0x, i0y, coords[2 * i], coords[2 * i + 1]);
                if (d < minDist && d > 0)
                {
                    i1 = i;
                    minDist = d;
                }
            }
            float i1x = coords[2 * i1];
            float i1y = coords[2 * i1 + 1];

            float minRadius = float.PositiveInfinity;
            #endregion

            #region find the third point which forms the smallest circumcircle with the first two
            for (int i = 0; i < n; i++)
            {
                if (i == i0 || i == i1)
                {
                    continue;
                }
                float r = Circumradius(i0x, i0y, i1x, i1y, coords[2 * i], coords[2 * i + 1]);
                if (r < minRadius)
                {
                    i2 = i;
                    minRadius = r;
                }
            }
            float i2x = coords[2 * i2];
            float i2y = coords[2 * i2 + 1];

            if (minRadius == float.PositiveInfinity)
            {
                throw new Exception("No Delaunay triangulation exists for this input");
            }
            #endregion

            #region swap the order of the seed points for counter-clockwise orientation
            if (Orient(i0x, i0y, i1x, i1y, i2x, i2y))
            {
                int i = i1;
                float x = i1x;
                float y = i1y;
                i1 = i2;
                i1x = i2x;
                i1y = i2y;
                i2 = i;
                i2x = x;
                i2y = y;
            }

            ValueTuple<float, float> center = Circumcenter(i0x, i0y, i1x, i1y, i2x, i2y);
            this._cx = center.Item1;
            this._cy = center.Item2;

            List<float> dists = new List<float>(n);
            for (int i = 0; i < n; i++)
            {
                dists.Add(Distance(coords[2 * i], coords[2 * i + 1], center.Item1, center.Item2));
            }
            #endregion

            Quicksort(IDs, dists, 0, n - 1); // sort the points by distance from the seed triangle circumcenter

            #region set up the seed triangle as the starting hull
            this.hullStart = i0;
            int hullSize = 3;

            hullNext.SetSafely(i0, i1);
            hullNext.SetSafely(i1, i2);
            hullNext.SetSafely(i2, i0);

            hullPrev.SetSafely(i2, i1);
            hullPrev.SetSafely(i0, i2);
            hullPrev.SetSafely(i1, i0);

            hullTri.SetSafely(i0, 0);
            hullTri.SetSafely(i1, 1);
            hullTri.SetSafely(i2, 2);

            hullHash[HashKey(i0x, i0y)] = i0;
            hullHash[HashKey(i1x, i1y)] = i1;
            hullHash[HashKey(i2x, i2y)] = i2;

            trianglesLen = 0;
            AddTriangle(i0, i1, i2, -1, -1, -1);
            #endregion


            float xp = 0;
            float yp = 0;
            for (int k = 0; k < IDs.Count; k++)
            {
                #region Check if skip and setup variables
                int i = IDs[k];
                float x = coords[2 * i];
                float y = coords[2 * i + 1];

                // skip near-duplicate points
                if (k > 0 && MathF.Abs(x - xp) <= float.Epsilon && MathF.Abs(y - yp) <= float.Epsilon)  continue;
                xp = x;
                yp = y;

                // skip seed triangle points
                if (i == i0 || i == i1 || i == i2) continue;
                #endregion

                #region find a visible edge on the convex hull using edge hash
                int start = 0;
                for (int j = 0, key = this.HashKey(x, y); j < this._hashSize; j++)
                {
                    start = hullHash[(key + j) % this._hashSize];
                    if (start != -1 && start != hullNext[start])
                        break;
                }

                start = hullPrev[start];
                int e = start;
                int q = hullNext[e];
                while (!Orient(x, y, coords[2 * e], coords[2 * e + 1], coords[2 * q], coords[2 * q + 1]))
                {
                    e = q;
                    if (e == start)
                    {
                        e = -1;
                        break;
                    }
                    q = hullNext[e];
                }
                if (e == -1) continue; // likely a near-duplicate point; skip it
                #endregion

                #region add the first triangle from the point
                int t = this.AddTriangle(e, i, hullNext[e], -1, -1, hullTri[e]);

                hullTri.SetSafely(i, this.Legalize(t + 2));
                hullTri.SetSafely(e, t); // keep track of boundary triangles on the hull
                hullSize++;
                #endregion

                #region walk forward through the hull, adding more triangles and flipping recursively
                n = hullNext[e];
                q = hullNext[n];
                while (Orient(x, y, coords[2 * n], coords[2 * n + 1], coords[2 * q], coords[2 * q + 1]))
                {
                    t = this.AddTriangle(n, i, q, hullTri[i], -1, hullTri[n]);
                    //hullTri[i] = this.Legalize(t + 2);
                    //hullNext[n] = n; // mark as removed
                    hullTri.SetSafely(i, this.Legalize(t + 2));
                    hullNext.SetSafely(n, n); // mark as removed
                    hullSize--;
                    n = q;
                    q = hullNext[n];
                }
                #endregion
                
                #region walk backward from the other side, adding more triangles and flipping
                if (e == start)
                {
                    q = hullPrev[e];
                    while (Orient(x, y, coords[2 * q], coords[2 * q + 1], coords[2 * e], coords[2 * e + 1]))
                    {
                        t = this.AddTriangle(q, i, e, -1, hullTri[e], hullTri[q]);
                        this.Legalize(t + 2);
                        hullTri.SetSafely(q, t);
                        hullNext.SetSafely(e, e); // mark as removed
                        hullSize--;
                        e = q;
                        q = hullPrev[e];
                    }
                }
                #endregion

                #region set hull next, previous, hash
                this.hullStart = e;
                hullPrev.SetSafely(i, e);
                hullNext.SetSafely(e, i);
                hullPrev.SetSafely(n, i);
                hullNext.SetSafely(i, n);

                hullHash.SetSafely(this.HashKey(x, y), i);
                hullHash.SetSafely(this.HashKey(coords[2 * e], coords[2 * e + 1]), e);
                #endregion
            }

            #region Identify ConvexHull
            this.ConvexHullIndexes = new List<int>(hullSize);
            for (int i = 0, e = this.hullStart; i < hullSize; i++)
            {
                this.ConvexHullIndexes.SetSafely(i, e);
                e = hullNext[e];
            }
            this.hullPrev = this.hullNext = this.hullTri = this.EdgeStack = null; // get rid of temporary arrays
            #endregion

            #region trim typed triangle mesh arrays
            this.TriangleIndexes = triangles.GetRange(0, this.trianglesLen);
            this.halfedges = halfedges.GetRange(0, this.trianglesLen);
            #endregion
        }
        
        #region methods
        private int AddTriangle(int i0, int i1, int i2, int a, int b, int c)
        {
            int t = this.trianglesLen;

            TriangleIndexes.SetSafely(t, i0);
            TriangleIndexes.SetSafely(t + 1, i1);
            TriangleIndexes.SetSafely(t + 2, i2);

            Link(t, a);
            Link(t + 1, b);
            Link(t + 2, c);

            trianglesLen += 3;

            return t;
        }
        private int Legalize(int a)
        {
            var triangles = this.TriangleIndexes;
            var coords = this.Positions;
            var halfedges = this.halfedges;

            int i = 0;
            int ar = 0;

            // recursion eliminated with a fixed-size stack
            while (true)
            {
                int b = halfedges[a];

                int a0 = a - a % 3;
                ar = a0 + (a + 2) % 3;

                if (b == -1)
                { // convex hull edge
                    if (i == 0)
                    {
                        break;
                    }
                    a = EdgeStack[--i];
                    continue;
                }

                int b0 = b - b % 3;
                int al = a0 + (a + 1) % 3;
                int bl = b0 + (b + 2) % 3;

                int p0 = triangles[ar];
                int pr = triangles[a];
                int pl = triangles[al];
                int p1 = triangles[bl];

                bool illegal = InCircle(
                    coords[2 * p0], coords[2 * p0 + 1],
                    coords[2 * pr], coords[2 * pr + 1],
                    coords[2 * pl], coords[2 * pl + 1],
                    coords[2 * p1], coords[2 * p1 + 1]);

                if (illegal)
                {
                    triangles[a] = p1;
                    triangles[b] = p0;

                    int hbl = halfedges[bl];

                    // edge swapped on the other side of the hull (rare); fix the halfedge reference
                    if (hbl == -1)
                    {
                        int e = this.hullStart;
                        do
                        {
                            if (this.hullTri[e] == bl)
                            {
                                this.hullTri[e] = a;
                                break;
                            }
                            e = this.hullNext[e];
                        } while (e != this.hullStart);
                    }
                    this.Link(a, hbl);
                    this.Link(b, halfedges[ar]);
                    this.Link(ar, bl);

                    int br = b0 + (b + 1) % 3;

                    // don't worry about hitting the cap: it can only happen on extremely degenerate input
                    if (i < EdgeStack.Count)
                    {
                        EdgeStack[i++] = br;
                    }
                }
                else
                {
                    if (i == 0)
                    {
                        break;
                    }
                    a = EdgeStack[--i];
                }
            }

            return ar;
        }

        

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Link(int a, int b)
        {
            halfedges.SetSafely(a, b);
            if (b != -1) halfedges.SetSafely(b, a);
        }

        private static float Circumradius(float ax, float ay, float bx, float by, float cx, float cy)
        {
            float dx = bx - ax;
            float dy = by - ay;
            float ex = cx - ax;
            float ey = cy - ay;

            float bl = dx * dx + dy * dy;
            float cl = ex * ex + ey * ey;
            float d = 0.5f / (dx * ey - dy * ex);

            float x = (ey * bl - dy * cl) * d;
            float y = (dx * cl - ex * bl) * d;

            return x * x + y * y;
        }

        private static ValueTuple<float, float> Circumcenter(float ax, float ay, float bx, float by, float cx, float cy)
        {
            float dx = bx - ax;
            float dy = by - ay;
            float ex = cx - ax;
            float ey = cy - ay;

            float bl = dx * dx + dy * dy;
            float cl = ex * ex + ey * ey;
            float d = 0.5f / (dx * ey - dy * ex);

            float x = ax + (ey * bl - dy * cl) * d;
            float y = ay + (dx * cl - ex * bl) * d;

            return ValueTuple.Create(x, y);
        }

        private static void Quicksort(List<int> ids, List<float> dists, int left, int right)
        {
            if (right - left <= 20)
            {
                for (int i = left + 1; i <= right; i++)
                {
                    int temp = ids[i];
                    float tempDist = dists[temp];
                    int j = i - 1;
                    while (j >= left && dists[ids[j]] > tempDist)
                    {
                        ids[j + 1] = ids[j--];
                    }
                    ids[j + 1] = temp;
                }
            }
            else
            {
                int median = (left + right) >> 1;
                int i = left + 1;
                int j = right;
                Swap(ids, median, i);
                if (dists[ids[left]] > dists[ids[right]])
                {
                    Swap(ids, left, right);
                }
                if (dists[ids[i]] > dists[ids[right]])
                {
                    Swap(ids, i, right);
                }
                if (dists[ids[left]] > dists[ids[i]])
                {
                    Swap(ids, left, i);
                }

                int temp = ids[i];
                float tempDist = dists[temp];
                while (true)
                {
                    do
                    {
                        i++;
                    }
                    while (dists[ids[i]] < tempDist);
                    do
                    {
                        j--;
                    }
                    while (dists[ids[j]] > tempDist);
                    if (j < i)
                    {
                        break;
                    }
                    Swap(ids, i, j);
                }
                ids[left + 1] = ids[j];
                ids[j] = temp;

                if (right - i + 1 >= j - left)
                {
                    Quicksort(ids, dists, i, right);
                    Quicksort(ids, dists, left, j - 1);
                }
                else
                {
                    Quicksort(ids, dists, left, j - 1);
                    Quicksort(ids, dists, i, right);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float Distance(float ax, float ay, float bx, float by)
        {
            float dx = ax - bx;
            float dy = ay - by;
            return dx * dx + dy * dy;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool Orient(float px, float py, float qx, float qy, float rx, float ry)
        {
            return (qy - py) * (rx - qx) - (qx - px) * (ry - qy) < 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Swap<T>(List<T> arr, int i, int j)
        {
            var tmp = arr[i];
            arr[i] = arr[j];
            arr[j] = tmp;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int HashKey(float x, float y)
        {
            return (int)MathF.Floor(PseudoAngle(x - _cx, y - _cy) * _hashSize) % _hashSize;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float PseudoAngle(float dx, float dy)
        {
            float p = dx / (MathF.Abs(dx) + MathF.Abs(dy));
            return (dy > 0 ? 3 - p : 1 + p) / 4; // [0..1]
        }

        private static bool InCircle(float ax, float ay, float bx, float by, float cx, float cy, float px, float py)
        {
            float dx = ax - px; float dy = ay - py;
            float ex = bx - px; float ey = by - py;
            float fx = cx - px; float fy = cy - py;

            float ap = dx * dx + dy * dy;
            float bp = ex * ex + ey * ey;
            float cp = fx * fx + fy * fy;

            return dx * (ey * cp - bp * fy) - dy * (ex * cp - bp * fx) + ap * (ex * fy - ey * fx) < 0;
        }
        #endregion region
    }
}