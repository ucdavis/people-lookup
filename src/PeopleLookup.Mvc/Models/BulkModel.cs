using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PeopleLookup.Mvc.Models
{
    public class BulkModel
    {
        [DataType(DataType.Text)]
        public string BulkEmail { get; set; }
        [DataType(DataType.MultilineText)]
        public string BulkKerb { get; set; }
    }
}
