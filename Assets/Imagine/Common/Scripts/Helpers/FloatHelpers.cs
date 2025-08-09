using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Imagine.WebAR
{
    public static class FloatExtensions
    {
        //this is needed to properly convert floating point strings for some languages to JSON
        public static string ToStringInvariantCulture(this float f)
        {
            return f.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
