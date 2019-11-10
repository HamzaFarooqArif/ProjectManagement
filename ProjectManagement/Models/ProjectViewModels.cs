using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ProjectManagement.Models
{
    public class ProjectCreateViewModel
    {
        [Required]
        [Display(Name = "Project Name")]
        public string name { get; set; }
        public string emails { get; set; }

    }
}