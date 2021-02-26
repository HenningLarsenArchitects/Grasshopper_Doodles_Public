using System;
using System.Collections.Generic;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Drawing;
using Grasshopper.Kernel.Types;



namespace Grasshopper_Doodles_Public
{
    // Source primarily from : https://discourse.mcneel.com/t/get-values-from-a-gradient-component/108532/6


    public class GhcGradientParser : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GhcGradientParser class.
        /// </summary>
        public GhcGradientParser()
          : base("GradientParser", "Grad",
              "Parse a gradient to colors (Doodles_HLA)",
              Constants.GH_TAB, Constants.GH_GENERIC_SUBTAB)
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddColourParameter("gradient", "G", "", GH_ParamAccess.tree);
            pManager[0].Optional = true;


            pManager.AddNumberParameter("numbers", "N", "", GH_ParamAccess.item, 10);
            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddColourParameter("outColors", "C", "", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            double outputs = 0;
            DA.GetData(1, ref outputs);

            GH_GradientControl gc = (GH_GradientControl)this.Params.Input[0].Sources[0].Attributes.GetTopLevel.DocObject;
            bool reverse = Params.Input[0].Reverse;
            var colors = new GradientParser(gc) { Reverse = reverse }.GetDefaultColors(Convert.ToInt32(outputs));

            DA.SetDataList(0, colors);



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
            get { return new Guid("38a025a4-7c76-bbbf-9932-c6a3bbf7d2e2"); }
        }
    }
}