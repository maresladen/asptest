using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace WebApplication.Models
{
    public class Project 
    {
        [Key] //表示主键, 必须要有
		public int projectId { get; set; }
		public string projectName { get; set; }
		public string projectMarkDown { get; set; }
    }
}
