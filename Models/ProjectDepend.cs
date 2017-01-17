using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Newtonsoft.Json;

namespace WebApplication.Models
{
    public class ProjectDepend 
    {
        [Key] //表示主键, 必须要有
		public int dependId { get; set; }
		public int projectId { get; set; }
		public string fileName { get; set; }
		public string filePath { get; set; }
		public string fileType { get; set; }
		public string fileMarkdown { get; set; }

		//忽略循环引用
		[JsonIgnore]
        public  Project project { get; set; }
    }
}
