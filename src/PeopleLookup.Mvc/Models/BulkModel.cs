using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PeopleLookup.Mvc.Models
{
    public class BulkModel
    {
        public BulkModel()
        {
            Results = new List<SearchResults>();
        }

        [DataType(DataType.Text)]
        public string BulkEmail { get; set; }
        [DataType(DataType.MultilineText)]
        public string BulkKerb { get; set; }

        public IList<SearchResults> Results { get; set; }
    }

    public class SearchResults
    {
        public string SearchValue { get; set; }
        public bool Found { get; set; } = false;
        public string FullName { get; set; }
    }
}
