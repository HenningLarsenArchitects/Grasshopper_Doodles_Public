using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace Grasshopper_Doodles_Public
{
    public class GhGridViewer : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GhGridViewer class.
        /// </summary>
        public GhGridViewer()
          : base("GridViewer", "GridViewer",
              "Gridviewer (Doodles_HLA)",
              Constants.GH_TAB, Constants.GH_GENERIC_SUBTAB)
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.primary; // puts the grid component in top.

        int inputGradient, inputSelectorMin, inputSelecterMax, inputStepSize, inputStep = 0;
        int steps = 0;
        double stepSize = 0;

        //ToolStripMenuItem menuItemCaps = new ToolStripMenuItem();

        private bool caps = false;

        //public bool Caps
        //{
        //    get { return caps; }
        //    set
        //    {
        //        caps = value;
        //        if ((caps))
        //        {
        //            Message = "Set Caps Off";
        //        }
        //        else
        //        {
        //            Message = "Set Caps On";
        //        }
        //    }
        //}


        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            //0
            pManager.AddGenericParameter("Grids", "Grids", "Grids", GH_ParamAccess.list);

            //1
            pManager.AddNumberParameter("Results", "Results", "Results", GH_ParamAccess.tree);

            //2
            inputGradient = pManager.AddColourParameter("Gradient", "Gradient", "", GH_ParamAccess.item);
            pManager[inputGradient].Optional = true;

            //3
            var inputRange = pManager.AddTextParameter("Range", "Range*", "input a domain '2 to 10' or a number as max '10'\nBUGGY ON THE ISOCURVES. LEAVE DEFAULT.", GH_ParamAccess.item, String.Empty);
            pManager[inputRange].Optional = true;

            //4
            //pManager.AddBooleanParameter("Cap", "Cap*", "Cap min and max?\nBUGGY ON THE ISOCURVES. LEAVE DEFAULT.", GH_ParamAccess.item, false);

            //5
            inputSelectorMin = pManager.AddColourParameter("-", "-", "MinColor", GH_ParamAccess.item, Color.Purple);
            pManager[inputSelectorMin].Optional = true;

            //6
            inputSelecterMax = pManager.AddColourParameter("-", "-", "MaxColor", GH_ParamAccess.item, Color.Pink);
            pManager[inputSelecterMax].Optional = true;

            //7
            inputStepSize = pManager.AddIntegerParameter("StepSize", "S", "Steps of colors, default = 10", GH_ParamAccess.item, 1);
            pManager[inputStepSize].Optional = true;
            var cfParam = pManager[inputStepSize] as Param_Integer;
            cfParam.AddNamedValue("Auto", 0);
            cfParam.AddNamedValue("0.1", -1);
            cfParam.AddNamedValue("1", 1);
            cfParam.AddNamedValue("10", 10);
            cfParam.AddNamedValue("100", 100);

            //8
            inputStep = pManager.AddIntegerParameter("Steps", "S", "Total steps. This will overrule the StepSize. Do not connect both!", GH_ParamAccess.item, 1);
            pManager[inputStep].Optional = true;


        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("coloredMesh", "coloredMesh", "mesh", GH_ParamAccess.list);


            pManager.AddMeshParameter("layeredMesh", "layeredMesh", "mesh", GH_ParamAccess.tree);


            int c = pManager.AddCurveParameter("curves", "curves", "planes", GH_ParamAccess.tree);
            pManager.HideParameter(c);

            int p = pManager.AddPlaneParameter("Planes", "P", "planes", GH_ParamAccess.list);
            pManager.HideParameter(p);

            int m = pManager.AddMeshParameter("TempMeshes", "TM", "planes", GH_ParamAccess.list);
            pManager.HideParameter(m);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            #region updateInputs
            //if (!cap && this.Params.Input.Count ==7)
            //{
            //    this.Params.Input[5].RemoveAllSources();
            //    this.Params.UnregisterInputParameter(this.Params.Input[5]);
            //    this.Params.Input[6].RemoveAllSources();
            //    this.Params.UnregisterInputParameter(this.Params.Input[6]);

            //    Params.OnParametersChanged();
            //}
            //if (cap && this.Params.Input.Count == 5)
            //{
            //    this.Params.RegisterInputParam(new Param_Colour
            //    {
            //        Name = "MinColor",
            //        NickName = "MinColor",
            //        Description = "MinColor",
            //        Access = GH_ParamAccess.item,
            //        Optional = true
            //    });
            //    this.Params.RegisterInputParam(new Param_Colour
            //    {
            //        Name = "MaxColor",
            //        NickName = "MaxColor",
            //        Description = "MinColor",
            //        Access = GH_ParamAccess.item,
            //        Optional = true
            //    });

            //    Params.OnParametersChanged();
            //}

            #endregion updateInputs

            //bool caps = DA.Fetch<bool>("Cap");
            var maxColor = DA.Fetch<Color>(inputSelecterMax);
            var minColor = DA.Fetch<Color>(inputSelectorMin);
            var allResults = DA.FetchTree<GH_Number>("Results");
            var grids = DA.FetchList<Grid>("Grids");
            var range = DA.Fetch<string>("Range");
            var inStepSize = DA.Fetch<int>("StepSize");
            var inSteps = DA.Fetch<int>("Steps");

            if (allResults.Branches.Count != grids.Count)
                throw new Exception("Grid count doesnt match results");

            if (!caps)
            {
                this.Params.Input[inputSelectorMin].NickName = "-";
                this.Params.Input[inputSelectorMin].Name = "-";
                this.Params.Input[inputSelecterMax].NickName = "-";
                this.Params.Input[inputSelecterMax].Name = "-";
            }
            else
            {
                this.Params.Input[inputSelectorMin].NickName = "MinColor";
                this.Params.Input[inputSelectorMin].Name = "MinColor";
                this.Params.Input[inputSelecterMax].NickName = "MaxColor";
                this.Params.Input[inputSelecterMax].Name = "MaxColor";
            }

            var domain = Misc.AutoDomain(range, allResults);
            //Rhino.RhinoApp.WriteLine($"{range}  ->  {domain[0]} to {domain[1]}");

            GH_GradientControl gc;
            try
            {
                gc = (GH_GradientControl)Params.Input[inputGradient].Sources[0].Attributes.GetTopLevel.DocObject;

            }
            catch (System.ArgumentOutOfRangeException)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Remember to add a gradient component in grasshopper!");
                gc = null;


            }

            GradientParser gp = new GradientParser(gc)
            {
                Cap = caps,
                AboveMax = maxColor,
                BelowMin = minColor,
                Min = domain[0],
                Max = domain[1],
                Reverse = Params.Input[inputGradient].Reverse
            };


            //Rhino.RhinoApp.WriteLine($"Probing {domain[0]} to the value of {gp.GetColors(new List<double> { domain[0] })[0]}");
            //Rhino.RhinoApp.WriteLine($"Probing {domain[1]} to the value of {gp.GetColors(new List<double> { domain[1] })[0]}");

            #region coloredMesh
            var outMeshes = new List<Mesh>();



            for (int i = 0; i < grids.Count; i++)
            {
                //AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, $"Mesh vertices: {grids[i].SimMesh.Vertices.Count}, colors = {gp.GetColors(allResults.Branches[i].Select(p => p.Value).ToArray()).Length} f");


                outMeshes.Add(grids[i].GetColoredMesh(gp.GetColors(allResults.Branches[i].Select(p => p.Value).ToArray())));

                Mesh m = grids[i].SimMesh;
                Point3d[] points = grids[i].SimPoints.ToArray();
                outMeshes[outMeshes.Count - 1].Translate(0, 0, Units.ConvertFromMeter(0.001));
            }


            DA.SetDataList(0, outMeshes);

            #endregion coloredMesh



            #region layeredMesh

            if (grids[0].UseCenters == true)
            {

                return;

            }

            //Outputs
            GH_Structure<GH_Mesh> layeredMeshes = new GH_Structure<GH_Mesh>();
            List<GH_Mesh> tempMeshes = new List<GH_Mesh>();
            List<GH_Plane> outPlanes = new List<GH_Plane>();
            GH_Structure<GH_Curve> outCurves = new GH_Structure<GH_Curve>();

            const double SCALAR = 1; // don't change.
            const float OFFSET = 0.0001f;

            double allMin = double.MaxValue;
            double allMax = -double.MaxValue;

            for (int i = 0; i < allResults.Branches.Count; i++)
            {
                //System.Collections.IList results = allResults.get_Branch(i);

                for (int j = 0; j < allResults[i].Count; j++)
                {
                    double result = allResults[i][j].Value;
                    if (result < allMin)
                        allMin = result;
                    if (result > allMax)
                        allMax = result;
                }


            }

            stepSize = inStepSize;
            double roundToNearest = 1;
            if (inStepSize == 0) // auto
            {
                //double digits = Math.Round(Math.Log10((domain[1] - domain[0]))) + 1;
                //double multiplier = Math.Pow(10, digits);
                //stepSize = Math.Log10((domain[1] - domain[0]));
                //if (allMax > 1000)
                //    stepSize = 100;
                //else if (allMax > 100)
                //    stepSize = 10;
                //else if (allMax > 10)
                //    stepSize = 1;
                //else
                //    stepSize = 0.1;
                stepSize = Misc.AutoStep(domain, out roundToNearest); // <-- TODO: We can set each slice in exactly the "round to nearest" number.

            }
            else if (inStepSize < 0) // fragment
            {
                stepSize = 1 / Math.Abs(inStepSize);
            }

            steps = Convert.ToInt32((domain[1] - domain[0]) / stepSize);


            for (int g = 0; g < grids.Count; g++)
            {

                //GH_Structure<GH_Curve> curves = new GH_Structure<GH_Curve>();
                Grid grid = grids[g];
                Mesh meshToCut = grids[g].SimMesh.DuplicateMesh();
                //Mesh meshToCut = grids[g].SimMesh;

                List<double> results = ((List<GH_Number>)allResults.get_Branch(g)).Select(r => r.Value).ToList();

                if (grids[g].UseCenters == true)
                {
                    results = RTreeSolver.FindClosestWeightedValues(grids[g], results, true).ToList();
                    // ADD CONVERSION TODO:
                }




                //Rhino.RhinoApp.WriteLine($"min = {allMin}, max = {allMax}, steps = {steps}, stepsize = {stepSize}");

                if (steps <= 1 || steps > 100)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"too {(steps < 4 ? "few" : "many")} steps (should be between 1 to 100)");
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"min = {allMin}, max = {allMax}, steps = {steps}, stepsize = {stepSize}");
                    continue;
                }

                if (allMax == allMin)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"max==min");
                    continue;
                }
                meshToCut.Normals.ComputeNormals();

                Plane cuttingPlane = new Plane(meshToCut.Vertices[0], meshToCut.FaceNormals[0]);

                //var planeOut = new Plane(plane);

                var planeBottom = new Plane(cuttingPlane);

                //List<int> belongsToWhichLayer = new List<int>();

                Vector3f normal = (Vector3f)(cuttingPlane.ZAxis);
                //AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"normal  x = {normal.X}, Y = {normal.Y}, Z = {normal.Z}");

                planeBottom.Transform(Transform.Translation(-cuttingPlane.ZAxis));

                //Moving the bottom down
                meshToCut.Translate(-normal * OFFSET);

                //Moving the vertices up
                for (int i = 0; i < results.Count; i++)
                {
                    meshToCut.Vertices[i] += (normal) * (float)SCALAR * (OFFSET + (float)results[i]);
                }

                Mesh topMesh = meshToCut.DuplicateMesh();

                tempMeshes.Add(new GH_Mesh(topMesh));

                Mesh edgeMesh = new Mesh();

                List<Point3d> ptOut = new List<Point3d>();
                Polyline[] edges = meshToCut.GetNakedEdges();

                double totalLength = 0;

                for (int i = 0; i < edges.Length; i++)
                {
                    totalLength += edges[i].Length;
                }

                Polyline[] edgesProjected = new Polyline[edges.Length];

                Transform p = Transform.PlanarProjection(planeBottom);

                for (int i = 0; i < edges.Length; i++)
                {
                    for (int j = 0; j < edges[i].SegmentCount; j++)
                    {
                        Mesh msh = new Mesh();
                        Point3d[] pts = new Point3d[4];

                        int id = (j == edges[i].SegmentCount - 1) ? 0 : j + 1;

                        pts[0] = new Point3d(edges[i].X[j], edges[i].Y[j], edges[i].Z[j]);
                        pts[1] = new Point3d(edges[i].X[id], edges[i].Y[id], edges[i].Z[id]);
                        pts[2] = new Point3d(pts[1]);
                        pts[3] = new Point3d(pts[0]);
                        pts[2].Transform(p);
                        pts[3].Transform(p);

                        msh.Vertices.AddVertices(pts);
                        var fc = new MeshFace(3, 2, 1, 0);
                        ptOut.AddRange(pts);
                        msh.Faces.AddFace(fc);

                        edgeMesh.Append(msh);
                    }
                }

                meshToCut.Append(edgeMesh);
                meshToCut.Weld(Math.PI);

                tempMeshes.Add(new GH_Mesh(meshToCut));

                //Transform t = Transform.Translation(new Vector3d(0, 0, inStepSize * SCALAR));

                Vector3f v = normal * (float)(stepSize.RoundTo(roundToNearest) * SCALAR);

                Transform t = Transform.Translation(v);

                //AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Vector v = {v.X}, {v.Y}, {v.Z},   instep = ");

                Mesh meshPerArea = new Mesh();
                MeshingParameters mp = new MeshingParameters(0);


                //double resultValue = inputMin;
                //stepSize = (inputMax - inputMin) / (float)steps;












                double currentValue = domain[0];
                int cuttingCount = -1;

                while (currentValue <= domain[1])
                {



                    cuttingCount++;

                    if (cuttingCount == 0)
                        currentValue = domain[0];


                    if (cuttingCount == 1)
                    {
                        cuttingPlane.Translate(new Vector3d(0, 0, domain[0].RoundTo(roundToNearest)));
                        //currentValue = domain[0];
                    }

                    if (cuttingCount > 0)
                        currentValue += stepSize;




                    if (cuttingCount > 80)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "ERROR CUT THE CUTTINGCOUNT");
                        break;
                    }

                    //Rhino.RhinoApp.WriteLine($"CurrentValue = {currentValue}, cuttingCount = {cuttingCount}");


                    //var resultValue = (double)cuttingCount / steps * (allMax - allMin) + allMin;


                    //resultValue = (double)cuttingCount / steps * (domain[1] - domain[0]) + domain[0];
                    //resultValue = (double)cuttingCount / steps * (domain[1] - domain[0]) + domain[0];

                    //var resultValue = currentValue; // new

                    //Rhino.RhinoApp.WriteLine($"Cutting {cuttingCount}, {resultValue}, {currentValue}, {allMin} - {allMax}");
                    //if (resultValue < domain[0])
                    //    continue;
                    //if (resultValue > domain[1])
                    //    break;



                    Polyline[] pl = Rhino.Geometry.Intersect.Intersection.MeshPlane(meshToCut, cuttingPlane);
                    outPlanes.Add(new GH_Plane(cuttingPlane));

                    if (pl == null)
                    {

                        break;

                    }



                    Color col = gp.GetColors(new List<double>() { cuttingCount == 0 ? double.MinValue : currentValue.RoundTo(roundToNearest) })[0];
                    //Rhino.RhinoApp.WriteLine($"Probing value {currentValue} to {col}");

                    //Mesh meshPerCut = new Mesh();

                    GH_Path path = new GH_Path(g, cuttingCount);

                    if (pl.Length > 0)
                    {

                        List<Curve> curves = new List<Curve>();
                        for (int j = 0; j < pl.Length; j++)
                        {

                            Curve curve = new PolylineCurve(pl[j]);

                            if (cuttingCount <= 0)
                            {
                                curve.Translate(normal * (float)(domain[0] - stepSize));
                            }


                            curve.Translate(-normal * (float)(currentValue * 0.95 - stepSize)); // was 0.95 nice

                            //curve.Translate(-normal * (float)allMin + normal * (float)(cuttingCount * Units.ConvertFromMeter(0.01)));



                            curves.Add(curve); // to create brep later

                            outCurves.Append(new GH_Curve(curve), path); // for output
                        }

                        Brep[] breps2 = Brep.CreatePlanarBreps(curves, Units.ConvertFromMeter(0.001));
                        for (int j = 0; j < breps2.Length; j++)
                        {
                            Mesh[] mesh2 = Mesh.CreateFromBrep(breps2[j], mp);

                            for (int k = 0; k < mesh2.Length; k++)
                            {
                                mesh2[k].VertexColors.CreateMonotoneMesh(col);

                                //meshPerCut.Append(mesh2[k]);

                                layeredMeshes.Append(new GH_Mesh(mesh2[k]), path);
                            }
                        }
                    }

                    //meshPerCut.VertexColors.CreateMonotoneMesh(col);



                    if (cuttingCount > 0)
                        cuttingPlane.Transform(t);
                }

                //layeredMeshes.Append(new GH_Mesh(meshPerArea), new GH_Path(g, );

            }



            //for (int j = 0; j < pl.Length; j++)
            //{
            //    Curve curve = pl[j].ToNurbsCurve();
            //    GH_Path path = new GH_Path(g, cuttingCount);

            //    outCurves.Append(new GH_Curve(curve), path);


            //    Brep[] breps = Brep.CreatePlanarBreps(curve, Units.ConvertFromMeter(0.001));

            //    if (breps == null)
            //        continue;

            //    Brep brep = breps[0];

            //    var area = AreaMassProperties.Compute(brep);
            //    if (area.Area > maxSize)
            //    {
            //        maxSize = area.Area;
            //        outerIndex = j;
            //    }
            //}

            //boundaryEdge = pl[outerIndex];


            //for (int j = 0; j < pl.Length; j++)
            //{
            //    if (j != outerIndex)
            //        holes.Add(pl[j].ToNurbsCurve());
            //}

            //Mesh mesh = null;
            //if (boundaryEdge.IsClosed)
            //{
            //    mesh = Mesh.CreatePatch(boundaryEdge, Math.PI / 2.0, null, holes, null, null, false, 0);

            //}
            //else
            //{

            //    AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, $"Curve is not closed");
            //}




            //outPlanes.Add(new GH_Plane(new Plane(cuttingPlane)));



            //int curvesCount = pl.Length;
            //int[] pointsRanges = new int[curvesCount];
            //Point3d[][] pts = new Point3d[curvesCount][];

            //for (int j = 0; j < pl.Length; j++)
            //{
            //    //Mesh mesh = GreenScenario.MeshUtil.CreateMeshWithHoles(pl);
            //    //Mesh mesh = Mesh.CreateFromTessellation(points, pl, Plane.WorldXY, false);
            //    //var mesh = Mesh.CreateFromClosedPolyline(pl[j]);


            //    if (mesh == null)
            //        continue;

            //    //outCurves.Append(new GH_Curve(pl[j].ToNurbsCurve()));

            //    //List<Color> colorList = new List<Color>();

            //    ////for (int i = 0; i < mesh.Faces.Count; i++)
            //    ////{
            //    ////    colorList.Add(col);
            //    ////    colorList.Add(col);
            //    ////    colorList.Add(col);
            //    ////    if (mesh.Faces[i].IsQuad)
            //    ////        colorList.Add(col);
            //    ////}

            //    //for (int i = 0; i < mesh.Vertices.Count; i++)
            //    //{
            //    //    colorList.Add(col);
            //    //}



            //    ////mesh.VertexColors.SetColors(colorList.ToArray());
            //    //AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, $"Vertices = {mesh.Vertices.Count}, colors = {mesh.VertexColors.Count}");

            //    //    mesh.VertexColors.CreateMonotoneMesh(col);
            //    //mesh.Translate(-normal * inStepSize * (float)SCALAR * cuttingCount * 0.90f);

            //    //   meshPerArea.Append(mesh);


            //    // we don't have more heights to cut off.
            //    //if (brep == null)
            //    //    continue;


            //    //for (int i = 0; i < brep.Length; i++)
            //    //{

            //    //    belongsToWhichLayer.Add(count);

            //    //}
            //    //pts.Add(polylinecrv.PointAtStart);


            //}




            // By now curves are moved to different elevations.
            //crvs = crvs;




            //Rhino.RhinoApp.WriteLine("adding a mesh");



            //oNumbers = outNumbers;



            //B = breps;
            //meshOut = mesh;


            Message = $"Cap = {(this.caps ? "on" : "off")} | Steps = {steps} | Step = {stepSize:0.0}";

            DA.SetDataTree(1, layeredMeshes);
            DA.SetDataTree(2, outCurves);
            DA.SetDataList("Planes", outPlanes);
            DA.SetDataList("TempMeshes", tempMeshes);

            #endregion layeredMesh


        }


        public override bool Read(GH_IO.Serialization.GH_IReader reader)
        {
            //targetPanelComponentGuid = reader.GetGuid("targetPanelComponentGuid");

            caps = reader.GetBoolean("caps");

            return base.Read(reader);
        }

        public override bool Write(GH_IO.Serialization.GH_IWriter writer)
        {
            //writer.SetGuid("targetPanelComponentGuid", targetPanelComponentGuid);

            writer.SetBoolean("caps", caps);


            return base.Write(writer);
        }


        //public void UpdateMessage()
        //{
        //    Message = $"Cap = {( caps ? "on" : "off")} \nSteps = {steps} \nStepSize = {stepSize:0.0}"; 
        //}


        private void OnMenu(object sender, EventArgs e)
        {
            //Rhino.RhinoApp.WriteLine($"e is {e}, id is {id}");
            //menu.Items[id].Visible = !menu.Items[id].Visible;
            //menu.Items[id].Name = "on";
            // menuItemCaps.Name = caps ? "Disable cap" : "Enable cap";
            caps = !caps;
            //Rhino.RhinoApp.WriteLine($"caps is now {(caps ? "on" : "off")}");
            this.ExpireSolution(true);
            //UpdateMessage();






        }

        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);
            Menu_AppendItem(menu, caps ? "Disable cap" : "Enable cap", OnMenu);


            //Menu_AppendGenericMenuItem(menu, "First item");
            //Menu_AppendGenericMenuItem(menu, "Second item");
            //Menu_AppendGenericMenuItem(menu, "Third item");
            //Menu_AppendSeparator(menu);
            //Menu_AppendGenericMenuItem(menu, "Fourth item");
            //Menu_AppendGenericMenuItem(menu, "Fifth item");
            //Menu_AppendGenericMenuItem(menu, "Sixth item");
        }



        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override System.Guid ComponentGuid
        {
            get { return new Guid("{ed148ced-bbd1-4f18-9768-32876b024930}"); }
        }
    }
}