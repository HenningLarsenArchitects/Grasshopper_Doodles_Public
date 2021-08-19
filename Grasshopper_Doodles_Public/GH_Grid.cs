using System;
using System.Collections.Generic;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Data;


namespace Grasshopper_Doodles_Public
{
    /// <summary>
    /// CALLING THE Geometry.Grid class
    /// </summary>
    public class GhcGrid : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GhcGrid class.
        /// </summary>
        public GhcGrid()
          : base("Grid", "Grid",
              "GraceHopper Grid\nUse this for analysis (Doodles_HLA). Based on https://github.com/HenningLarsenArchitects/Grasshopper_Doodles_Public",
              Constants.GH_TAB, Constants.GH_GENERIC_SUBTAB)
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.primary; // puts the grid component in top.

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {

            // Do not change names. But ordering can be changed.
            pManager.AddGeometryParameter("Geometry", "Geo", "Input surfaces, meshes or curves", GH_ParamAccess.list);
            pManager.AddNumberParameter("GridSize [m]", "S [m]", "Grid size in meters", GH_ParamAccess.item, Units.ConvertFromMeter(1));
            pManager.AddNumberParameter("Vertical Offset [m]", "VO [m]", "Vertical offset", GH_ParamAccess.item, Units.ConvertFromMeter(0.75));
            pManager.AddNumberParameter("Edge Offset [m]", "EO [m]*", "*Not yet implemented.\nEdge offset", GH_ParamAccess.item, Units.ConvertFromMeter(0.5));
            int ic = pManager.AddBooleanParameter("IsoCurves", "IC?", "Leave default to make it work with the isocurves preview! For grid pixels, set to false.\nUsing vertice points for later using to generate isocurves. If false, it will use center of faces. Default is true.", GH_ParamAccess.item, true);
            pManager[ic].Optional = true;

            int ps = pManager.AddBooleanParameter("PerfectSquares?*", "PC?", "*Not yet implemented.\nIt's a attempt to do perfect squares, so you dont get extra points at corners.\nhttps://discourse.ladybug.tools/t/none-uniform-grid/2361/11", GH_ParamAccess.item, true);
            pManager[ps].Optional = true;
            // TODO: https://discourse.ladybug.tools/t/none-uniform-grid/2361/11

            pManager.AddBooleanParameter("GoLarge", "GoLarge", "Set to true if you accept grids larger than 60000 points. This is a safety check.", GH_ParamAccess.item, false);




        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Grids", "G", "Output grids. GreenScenario.Geometry.Grid class", GH_ParamAccess.list);
            pManager.AddMeshParameter("Mesh", "M", "Meshes", GH_ParamAccess.list);
            pManager.AddGenericParameter("Points", "Pt", "Simulation points (center of each face)", GH_ParamAccess.tree);
            
            //pManager.AddTextParameter("msg", "m", "msg", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            var gridSize = Units.ConvertFromMeter(DA.Fetch<double>("GridSize [m]"));
            var edgeOffset = Units.ConvertFromMeter(DA.Fetch<double>("Edge Offset [m]"));

            var offset = Units.ConvertFromMeter(DA.Fetch<double>("Vertical Offset [m]"));
            var useCenters = DA.Fetch<bool>("IsoCurves");
            var geometries = DA.FetchList<IGH_GeometricGoo>("Geometry");
            var goLarge = DA.Fetch<bool>("GoLarge");

            DataTree<Point3d> centers = new DataTree<Point3d>();
            List<Grid> myGrids = new List<Grid>();
            List<Mesh> meshes = new List<Mesh>();

            //List<Mesh> inMeshes = new List<Mesh>();
            List<Brep> inBreps = new List<Brep>();
            List<Curve> inCrvs = new List<Curve>();

            //string msg = "";
            useCenters = !useCenters;

            for (int i = 0; i < geometries.Count; i++)
            {
                if (geometries[i] == null)
                    continue;

                IGH_GeometricGoo shape = geometries[i].DuplicateGeometry();

                shape.Transform(Transform.Translation(0, 0, offset));

                if (shape is Mesh || shape is GH_Mesh)
                {
                    //inMeshes.Add(GH_Convert.ToGeometryBase(shape) as Mesh);
                    myGrids.Add(new Grid(GH_Convert.ToGeometryBase(shape) as Mesh, useCenters: useCenters));
                }
                else if (shape is Brep || shape is GH_Brep)
                {
                    //myGrids.Add(new Grid(GH_Convert.ToGeometryBase(shape) as Brep, gridSize, useCenters: useCenters));
                    inBreps.Add(GH_Convert.ToGeometryBase(shape) as Brep);
                }
                else if (shape is Curve || shape is GH_Curve)
                {
                    //myGrids.Add(new Grid(GH_Convert.ToGeometryBase(shape) as Curve, gridSize, useCenters: useCenters));
                    inCrvs.Add(GH_Convert.ToGeometryBase(shape) as Curve);
                }
                else
                {
                    myGrids.Add(null);
                    meshes.Add(null);
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "error on an input grid");
                }


            }

            List<Brep> breps = InputGeometries.CurvesToOffsetBreps(inCrvs, edgeOffset) ?? new List<Brep>();

            breps.AddRange(InputGeometries.BrepsToOffsetBreps(inBreps, edgeOffset));


            for (int i = 0; i < breps.Count; i++)
            {
                myGrids.Add(new Grid(breps[i], gridSize, useCenters: useCenters, goLarge));
            }

            for (int i = 0; i < myGrids.Count; i++)
            {
                meshes.Add(myGrids[i].SimMesh);
                GH_Path p = new GH_Path(i);
                centers.AddRange(myGrids[i].SimPoints, p);
            }


            DA.SetDataList(0, myGrids);
            DA.SetDataList(1, meshes);
            DA.SetDataTree(2, centers);
            //DA.SetData(3, msg);

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
        public override Guid ComponentGuid
        {
            get { return new Guid("fcbeb1dc-b545-4162-b280-12fec6a574c7"); }
        }
    }
}