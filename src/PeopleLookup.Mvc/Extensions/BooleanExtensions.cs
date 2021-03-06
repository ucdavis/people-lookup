﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PeopleLookup.Mvc.Extensions
{
    public static class BooleanExtensions
    {
        public static string ToYesNoString(this bool value)
        {
            return value ? "Yes" : "No";
        }

        public static string ToYesNoString(this bool? value)
        {
            if (!value.HasValue)
            {
                return string.Empty;
            }

            return value.Value.ToYesNoString();
        }
    }
}
