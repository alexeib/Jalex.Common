﻿using System;
using System.Diagnostics;
using Jalex.Infrastructure.Attributes;

namespace Jalex.Authentication.Objects
{
    [DebuggerDisplay("User: {UserId} - Device: {DeviceId}")]
    public class AuthenticationToken
    {
        [Id(IsAutoGenerated = true)]
        public string Id { get; set; }
        [Indexed("UserAndDevice", 0)]
        public string UserId { get; set; }
        [Indexed("UserAndDevice", 1)]
        public string DeviceId { get; set; }
        public DateTime Created { get; set; }
    }
}