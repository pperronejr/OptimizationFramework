
//
// Copyright 2017 Paul Perrone.  All rights reserved.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IDA.Optimization
{
    public class OptimizationItemNoValidValueException : Exception
    {
        public OptimizationItemNoValidValueException(string message) :
            base( message)
        { }
        public OptimizationItemNoValidValueException() :
            base( DefaultMessage)
        { }

        public static string DefaultMessage = "Valid OptimizationItem value cannot be found.  Solution space is overconstrained.";
    }
}
