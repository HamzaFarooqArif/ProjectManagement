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
        [MinLength(1)]
        public string name { get; set; }

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
    public class ProjectDetailsViewModel
    {
        public int id { get; set; }
        [Display(Name = "Project Name")]
        public string name { get; set; }
        [Display(Name = "Project Admin")]
        public string admin { get; set; }
        public bool editable { get; set; }
        [Display(Name = "Team")]
        public List<ProjectUserViewModel> users { get; set; }
    }
    public class ProjectUserViewModel
    {
        public string id { get; set; }
        [Display(Name = "Email")]
        public string email { get; set; }
        public string role { get; set; }
        public bool isAdmin { get; set; }
        public bool confirmed { get; set; }
    }
}