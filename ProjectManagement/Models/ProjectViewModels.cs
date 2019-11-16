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
        [Required]
        [Display(Name = "Collaborators")]
        public string emails { get; set; }

    }
    public class ProjectIndexViewModel
    {
        public int id { get; set; }
        [Display(Name = "Project Name")]
        public string name { get; set; }
        [Display(Name = "Project Admin")]
        public string admin { get; set; }
        public bool editable { get; set; }
    }


}