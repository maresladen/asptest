using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Newtonsoft.Json;

namespace WebApplication.Models
{
    public class Features 
    {
        [Key] //表示主键, 必须要有
		public int featuresId { get; set; }
		public int projectId { get; set; }
		public string featuresName { get; set; }
		public string featuresHtml { get; set; }
		public string featuresScript { get; set; }
		public string featuresCss { get; set; }

        public int mdId { get; set; }
		public virtual List<FeaturesDepend> featuresDepends {get;set;}

		[JsonIgnore]
        public virtual Project project { get; set; }
    }
}
