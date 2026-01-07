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
        [Display(Name = "Bulk Emails")]
        public string BulkEmail { get; set; }
        [DataType(DataType.MultilineText)]
        [Display(Name = "Bulk Kerberos Ids")]
        public string BulkKerb { get; set; }

        [Display(Name = "Bulk Student Ids")]
        public string BulkStudentIds { get; set; }

        [Display(Name = "Bulk PPS Ids")]
        public string BulkPpsIds { get; set; }

        [Display(Name = "Bulk IAM Ids")]
        public string BulkIamIds { get; set; }

        [Display(Name = "Bulk Last Names")]
        public string BulkLastnames { get; set; }

        [Display(Name = "Bulk Employee Ids")]
        public string BulkEmployeeId { get; set; }

        [Display(Name = "PPSA Department Code")]
        public string PpsaDeptCode { get; set; }

        public IList<SearchResult> Results { get; set; }
    }

    public class SearchResult
    {
        public string SearchValue { get; set; }
        public bool Found { get; set; } = false;
        public string FullName { get; set; }
        public string O_FullName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Pronouns { get;set;}
        public string KerbId { get; set; }
        public string IamId { get; set; }
        public string Email { get; set; }

        public string OtherEmails { get; set; }

        public bool IsEmployee { get; set; }
        public bool IsHSEmployee { get; set; }
        public bool IsFaculty { get; set; }
        public bool IsStudent { get; set; }
        public bool IsExternal { get; set; }
        public bool IsStaff { get; set; }
        public string PpsId { get; set; }
        public string StudentId { get; set;}
        public string BannerPidm { get; set; }
        public string EmployeeId { get; set; }
        public string MothraId { get;set;}

        public string Title { get; set; }
        public string ReportsToIamId { get; set; }
        public string WorkPhone { get;set;}
        
        public void HideSensitiveFields()
        {
            StudentId = null;
            BannerPidm = null;
            PpsId = null;
            EmployeeId = null;
            ExceptionMessage = null;
            ReportsToIamId = null;
            MothraId = null;
            O_FullName = null;
        }

        public string ExpandedAffiliation
        {
            get
            {
                var roles = new List<string>();
                if (IsStaff)
                {
                    roles.Add("Staff");
                }

                if (IsFaculty)
                {
                    roles.Add("Faculty");
                }

                if (IsStudent)
                {
                    roles.Add("Student");
                }

                return string.Join(", ", roles);
            }
        }

        public string Departments { get; set; }


        public string ErrorMessage { get; set; }

        public string ExceptionMessage { get; set; }
    }
}
