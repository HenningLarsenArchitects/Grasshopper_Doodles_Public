using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grasshopper_Doodles_Public
{
    class RTreeSolver
    {


        public static double[] FindClosestWeightedValues(Grid grid, IList<double> results, bool parallel = false)
        {
            if (grid.UseCenters == false)
                throw new Exception("should only be called on nonCenter grids");

            List<Point3d> searchPoints = grid.SimMesh.Vertices.ToPoint3dArray().ToList();
            double CollisionDistance = grid.GridDist * 2.5;
            var targetMesh = grid.SimMesh;
            RTree rTree = new RTree();

            List<List<int>> potentialTargetsPerPoint = new List<List<int>>();
            int[] closestTargetPerPoint = new int[searchPoints.Count].Populate(-1);
            int[][] closestTargetsPerPoint = new int[searchPoints.Count][];

            double[] finalResults = new double[searchPoints.Count];

            List<List<Point3d>> roomVertices = new List<List<Point3d>>();

            for (int i = 0; i < searchPoints.Count; i++)
            {
                potentialTargetsPerPoint.Add(new List<int>());
                closestTargetsPerPoint[i] = new int[targetMesh.Faces[i].IsQuad ? 4 : 3];
            }

            for (int i = 0; i < targetMesh.Vertices.Count; i++)
                rTree.Insert(targetMesh.Vertices[i], i);

            for (int i = 0; i < searchPoints.Count; i++)
            {
                rTree.Search(
                    new Sphere(searchPoints[i], CollisionDistance),
                    (sender, args) => { potentialTargetsPerPoint[i].Add(args.Id); });
            }

            for (int i = 0; i < potentialTargetsPerPoint.Count; i++)
                potentialTargetsPerPoint[i] = potentialTargetsPerPoint[i].Distinct().ToList();

            if (!parallel)
            {
                for (int i = 0; i < searchPoints.Count; i++)
                {

                    if (potentialTargetsPerPoint.Count > 0)
                    {

                        var list = new[]
                        {
                            new {dist = double.MaxValue, result = 0.0 },
                            new {dist = double.MaxValue, result = 0.0 },
                            new {dist = double.MaxValue, result = 0.0 }
                        }.ToList();
                        if (closestTargetsPerPoint[i].Length == 4) // isquad
                            list.Add(new { dist = double.MaxValue, result = 0.0 });

                        for (int j = 0; j < potentialTargetsPerPoint[i].Count; j++)
                        {

                            var targetPoint = targetMesh.Vertices[potentialTargetsPerPoint[i][j]];

                            double distance = searchPoints[i].DistanceTo(targetPoint);

                            if (distance < list[list.Count - 1].dist)
                            {

                                closestTargetPerPoint[i] = potentialTargetsPerPoint[i][j];

                                list[closestTargetsPerPoint[i].Length] = new { dist = distance, result = results[closestTargetPerPoint[i]] };

                                list.OrderBy(l => l.dist);

                            }
                        }

                        finalResults[i] = list.Select(l => l.result).Sum() / closestTargetsPerPoint[i].Length;

                    }

                }

            }
            else
            {
                Parallel.For(0, searchPoints.Count, i =>
                {
                    if (potentialTargetsPerPoint.Count > 0)
                    {

                        var list = new[]
                        {
                            new {dist = double.MaxValue, result = 0.0 },
                            new {dist = double.MaxValue, result = 0.0 },
                            new {dist = double.MaxValue, result = 0.0 }
                        }.ToList();
                        if (closestTargetsPerPoint[i].Length == 4) // isquad
                            list.Add(new { dist = double.MaxValue, result = 0.0 });

                        for (int j = 0; j < potentialTargetsPerPoint[i].Count; j++)
                        {

                            var targetPoint = targetMesh.Vertices[potentialTargetsPerPoint[i][j]];

                            double distance = searchPoints[i].DistanceTo(targetPoint);

                            if (distance < list[list.Count - 1].dist)
                            {

                                closestTargetPerPoint[i] = potentialTargetsPerPoint[i][j];

                                list[closestTargetsPerPoint[i].Length] = new { dist = distance, result = results[closestTargetPerPoint[i]] };

                                list.OrderBy(l => l.dist);

                            }
                        }

                        finalResults[i] = list.Select(l => l.result).Sum() / closestTargetsPerPoint[i].Length;

                    }


                });
            }


            return finalResults;

        }


        public static int[] ConnectPointsToPoints(List<Point3d> searchPoints, List<Point3d> targets, double CollisionDistance = 10.0, bool parallel = false)
        {

            RTree rTree = new RTree();

            List<List<int>> potentialTargetsPerPoint = new List<List<int>>();

            int[] closestTargetPerPoint = new int[searchPoints.Count].Populate(-1);


            List<List<Point3d>> roomVertices = new List<List<Point3d>>();


            for (int i = 0; i < searchPoints.Count; i++)
            {
                potentialTargetsPerPoint.Add(new List<int>());


            }

            for (int i = 0; i < targets.Count; i++)
            {


                rTree.Insert(targets[i], i);

            }


            for (int i = 0; i < searchPoints.Count; i++)
            {

                rTree.Search(
                    new Sphere(searchPoints[i], CollisionDistance),
                    (sender, args) => { potentialTargetsPerPoint[i].Add(args.Id); });
            }


            for (int i = 0; i < potentialTargetsPerPoint.Count; i++)
            {
                potentialTargetsPerPoint[i] = potentialTargetsPerPoint[i].Distinct().ToList();

            }

            if (!parallel)
            {
                for (int i = 0; i < searchPoints.Count; i++)
                {

                    double dist = double.MaxValue;

                    if (potentialTargetsPerPoint.Count > 0)
                    {

                        for (int j = 0; j < potentialTargetsPerPoint[i].Count; j++)
                        {

                            Point3d windowCenter = searchPoints[i];

                            var targetPoint = targets[potentialTargetsPerPoint[i][j]];

                            double distance = windowCenter.DistanceTo(targetPoint);

                            if (distance < dist)
                            {
                                closestTargetPerPoint[i] = potentialTargetsPerPoint[i][j];
                                dist = distance;
                            }
                        }
                    }

                }

            }
            else
            {
                Parallel.For(0, searchPoints.Count, i =>
                {
                    double dist = double.MaxValue;

                    if (potentialTargetsPerPoint.Count > 0)
                    {

                        for (int j = 0; j < potentialTargetsPerPoint[i].Count; j++)
                        {

                            Point3d windowCenter = searchPoints[i];

                            var targetPoint = targets[potentialTargetsPerPoint[i][j]];

                            double distance = windowCenter.DistanceTo(targetPoint);

                            if (distance < dist)
                            {
                                closestTargetPerPoint[i] = potentialTargetsPerPoint[i][j];
                                dist = distance;
                            }
                        }
                    }


                });
            }


            return closestTargetPerPoint;

        }
    }
}
