using Rhino;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Grasshopper_Doodles_Public
{
    
    static class Units
    {
        /// <summary>
        /// Return document units
        /// </summary>
        /// <returns>doc units</returns>
        public static string GetUnits()
        {
            return Rhino.RhinoDoc.ActiveDoc.GetUnitSystemName(true, true, true, true);
        }

        private class UnitType
        {
            public string Name { get; set; }
            public double Factor { get; set; }
            public UnitSystem MySystem { get; set; }

            public UnitType(string name, double factor, UnitSystem unitSystem)
            {
                Name = name;
                Factor = factor;
                MySystem = unitSystem;
            }
        }

        private static readonly UnitType[] unitTypes = new UnitType[] {
            new UnitType("m", 1.0, UnitSystem.Meters),
            new UnitType("mm", 1000.0, UnitSystem.Millimeters),
            new UnitType("cm", 100.0, UnitSystem.Centimeters),
            new UnitType("ft", 3.2808399, UnitSystem.Feet),
            new UnitType("in", 39.3700787, UnitSystem.Inches)
        };

        public static bool SetUnits(string targetUnits, bool rescaleAll = true)
        {
            foreach (var unitType in unitTypes)
            {
                if (targetUnits == unitType.Name && targetUnits == Rhino.RhinoDoc.ActiveDoc.GetUnitSystemName(true, true, true, true))
                {
                    Rhino.RhinoDoc.ActiveDoc.AdjustModelUnitSystem(UnitSystem.Meters, rescaleAll);
                    return true;
                }
            }

            //Type x = MethodBase.GetCurrentMethod().DeclaringType;

            throw new KeyNotFoundException("Unit type not implemented. Currently implemented: " +
                String.Join(", ", unitTypes.Select(u => u.Name)));


        }

        public static double GetConversionFactor()
        {
            string units = GetUnits();

            foreach (var unitType in unitTypes)
            {
                if (units == Rhino.RhinoDoc.ActiveDoc.GetUnitSystemName(true, true, true, true))
                {
                    return unitType.Factor;
                }
            }

            throw new KeyNotFoundException("Unit type not implemented. Currently implemented: " +
                String.Join(", ", unitTypes.Select(u => u.Name)));
        }

        public static double ConvertToMeter()
        {
            return 1.0 / GetConversionFactor();
        }

        public static double ConvertToMeter(double distanceInDocUnits)
        {
            return ConvertToMeter() * distanceInDocUnits;
        }

        public static double ConvertFromMeter()
        {
            return GetConversionFactor();
        }

        public static double ConvertFromMeter(double distanceInDocUnits)
        {
            return ConvertFromMeter() * distanceInDocUnits;
        }
    }
}
