using System.ComponentModel.DataAnnotations;
using ControllRR.Application.Dto;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ControllRR.Presentation.ViewModels;
public class RoleViewModel
{
      public List<SelectListItem> Roles { get; set; }
      public ApplicationUserDto? applicationUserDto {get; set;}
        public RoleViewModel()
        {
            Roles = new List<SelectListItem>();
            Roles.Add(new SelectListItem(){Value = "1", Text = "Admin" });
            Roles.Add(new SelectListItem(){Value = "2", Text = "Manager" });
            Roles.Add(new SelectListItem(){Value = "3", Text = "Member" });
        }
        
     
}