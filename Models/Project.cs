using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Newtonsoft.Json;

namespace WebApplication.Models
{
    public class Project 
    {
        [Key] //表示主键, 必须要有
		public int projectId { get; set; }
		public string projectName { get; set; }
		public string projectMarkDown { get; set; }
		public string projectMdText { get; set; }
		//忽略循环引用
		// [JsonIgnore]
		public  List<ProjectDepend> projectDepends { get; set; }
    }

}
