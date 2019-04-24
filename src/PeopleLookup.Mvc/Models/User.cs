using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PeopleLookup.Mvc.Models
{
public class User
    {
        // kerb ID
        public string Id { get; set; }

        // TODO: make a key?
        public string Iam {get; set;}

        [Required]
        [StringLength(50)]
        [Display(Name = "First Name")]        
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Last Name")]        
        public string LastName { get; set; }

        [StringLength(256)]
        [Display(Name = "Name")]        
        public string Name { 
            get {
                return FirstName + " " + LastName;
            }
        }

        [Required]
        [StringLength(256)]
        [EmailAddress]
        public string Email { get; set; }
        

    }
}
