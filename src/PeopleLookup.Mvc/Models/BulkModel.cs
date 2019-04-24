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
            Results = new List<SearchResult>();
        }

        [DataType(DataType.Text)]
        public string BulkEmail { get; set; }
        [DataType(DataType.MultilineText)]
        public string BulkKerb { get; set; }

        public IList<SearchResult> Results { get; set; }
    }

    public class SearchResult
    {
        public string SearchValue { get; set; }
        public bool Found { get; set; } = false;
        public string FullName { get; set; }
        public string KerbId { get; set; }
        public string IamId { get; set; }
        public string Email { get; set; }

        public bool IsEmployee { get; set; }
        public bool IsHSEmployee { get; set; }
        public bool IsFaculty { get; set; }
        public bool IsStudent { get; set; }
        public bool IsExternal { get; set; }


        public string ErrorMessage { get; set; }
    }
}
