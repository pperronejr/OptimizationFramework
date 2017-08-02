
//
// Copyright 2017 Paul Perrone.  All rights reserved.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IDA.Optimization
{
    public static class ObjectiveItems<T> where T : ObjectiveItem
    {
        public static List<ObjectiveItem> Convert(List<T> items)
        {
            return items.ConvertAll<ObjectiveItem>(ObjectiveItemConverter);
        }

        private static Converter<T, ObjectiveItem> ObjectiveItemConverter =
            new Converter<T, ObjectiveItem>
                (delegate(T item) { return (ObjectiveItem)item; });
    }
}
