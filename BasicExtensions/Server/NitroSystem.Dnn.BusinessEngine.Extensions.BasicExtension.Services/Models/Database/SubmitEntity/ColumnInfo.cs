using NitroSystem.Dnn.BusinessEngine.Data.Entities.Tables;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.BasicExtensions.Services.Models.Database.SubmitEntity
{
    public class ColumnInfo
    {
        public Guid Id { get; set; }
        public string ColumnName { get; set; }
        public string ColumnValue { get; set; }
        public bool IsSelected { get; set; }
    }
}