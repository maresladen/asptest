using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace WebApplication.Models
{
    public class Employee 
    {
        [Key] //表示主键, 必须要有
		public int id { get; set; }
		public string Name { get; set; }
		public string LastName { get; set; }
    }
}
