using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace Grasshopper_Doodles_Public
{
    public class GH_SectionType_Range : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GH_SectionType_Range class.
        /// </summary>
        public GH_SectionType_Range()
          : base("GH_SectionType_Range", "Section_Range",
              "Description",
               Constants.GH_TAB, Constants.GH_GENERIC_SUBTAB)
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("From", "From", "From", GH_ParamAccess.item);
            pManager.AddNumberParameter("To", "To", "To", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Steps", "Steps", "Steps", GH_ParamAccess.item);

            for (int i = 0; i < pManager.ParamCount - 1; i++)
            {
                pManager[i].Optional = true;
            }
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Input Selector", "Input Selector", "Input Selector", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double? from = DA.Fetch<double?>("From");
            double? to = DA.Fetch<double?>("To");
            int steps = DA.Fetch<int>("Steps");

            InputSelector inputSelector = new InputSelector(steps, from, to);

            DA.SetData(0, inputSelector);
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
            get { return new Guid("53a7e289-6178-48f2-be6d-68362245dd08"); }
        }
    }
}