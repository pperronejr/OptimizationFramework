
//
// Copyright 2017 Paul Perrone.  All rights reserved.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IDA.Optimization
{
    // If IOptimization is changed to not be based on IObjectiveItem then this constraint
    // should be changed to not include IObjectiveItem.
    public static class OptimizationItems<T> where T : OptimizationItem
    {
        public static List<OptimizationItem> Convert(List<T> items)
        {
            return items.ConvertAll<OptimizationItem>(OptimizationItemConverter);
        }

        private static Converter<T, OptimizationItem> OptimizationItemConverter =
            new Converter<T, OptimizationItem>
                (delegate(T item) { return (OptimizationItem)item; });
    }
}
