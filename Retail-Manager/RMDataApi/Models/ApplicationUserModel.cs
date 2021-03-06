﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RMDataApi.Models
{
    public class ApplicationUserModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public Dictionary<string, string> Roles { get; set; } = new Dictionary<string, string>();
    }
}
