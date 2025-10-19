﻿using System;

namespace NitroSystem.Dnn.BusinessEngine.Abstractions.Studio.Engine.InstallExtension.Models
{
    public class ExtensionOwner
    {
        public Guid OwnerID { get; set; }
        public string OwnerName { get; set; }
        public string Organization { get; set; }
        public string Logo { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public string Telephone { get; set; }
        public string Address { get; set; }
    }
}