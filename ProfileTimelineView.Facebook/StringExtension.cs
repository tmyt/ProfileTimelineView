﻿using System.Collections.Generic;
using System.Linq;

namespace ProfileTimelineView.Facebook
{
    static class StringExtension
    {
        public static IDictionary<string, string> ParseQueryString(this string enumerable)
        {
            return enumerable.Split('&')
                .Select(s => s.Split('=').Concat(new[]{""}).ToArray())
                .ToDictionary(s => s[0], s => s[1]);
        }
    }
}
