
//
// Copyright 2017 Paul Perrone.  All rights reserved.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IDA.Logging;

namespace IDA.Optimization
{
    internal class OptimizationLogging : ApplicationLogging
    {
        internal OptimizationLogging()
        {
            LogFileName = "Optimization.log";
            DetailedLogging = false;
        }

        public bool DetailedLogging { get; set; }
    }
}
