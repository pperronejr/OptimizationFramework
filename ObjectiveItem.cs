
//
// Copyright 2017 Paul Perrone.  All rights reserved.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IDA.Optimization
{
    /// <summary>
    /// Protocol for an item to be included in a solution's ObjectiveFunction
    /// </summary>
    abstract public class ObjectiveItem
    {
        abstract public double GetCost();

        /// <summary>
        /// This method is used as a conditional check to validate that the item is in a valid state.
        /// It is called as part of the system level validation check in OptimizationSolution.
        /// This method should be specialized if the item has constraints or performance
        /// criteria that need to be met.
        /// </summary>
        /// <returns></returns>
        abstract public bool IsValid();

        /// <summary>
        /// If true then this objective item is included in the search.  
        /// If false then item is effectively removed from the search.
        /// 
        /// This property should be defaulted to true.
        /// 
        /// This field should preferably be public.  It was changed to protected internal
        /// so a public version can be defined in a class that inherits this so its XML attributes
        /// can be specialized.  If a new public propery with the same name is defined in an inheriting
        /// class then its get and set should reference this field (see IDA.DAPO.Annotation).
        /// When DAPO is rewritten to separate out the XML serialization then this field should be made public.
        /// </summary>
        protected internal bool IsActive = true;
    }
}
