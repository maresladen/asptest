using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace WebApplication.Models
{
    public class FeaIgnoreProDepend 
    {
        [Key] //表示主键, 必须要有
		public int freaturesDependId { get; set; }
		public int featuresId { get; set; }
		public int projectDependid  { get; set; }
    }
}
