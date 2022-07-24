﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Veg.API.Client
{
    public class FilterPagingOptions
    {
        public Dictionary<string, string> FilterParameters { get; set; } = new Dictionary<string, string>();
        public string SortProperty { get; set; }
        
        public string[] Includes { get; set; }

        public int Page { get; set; } = 1;

        public int ItemsPerPage { get; set; } = 1;

        public string ToQueryString()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static FilterPagingOptions FromString(string json)
        {
            return JsonConvert.DeserializeObject<FilterPagingOptions>(json);
        }
    }
}