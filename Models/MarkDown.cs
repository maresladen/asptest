using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace WebApplication.Models
{
    public class MarkDown 
    {
        [Key] //表示主键, 必须要有
		public int MdId { get; set; }
		[Column(TypeName="mediumtext")]
		public string MdHTML { get; set; }
		[Column(TypeName="text")]
		public string MdText { get; set; }
		public  virtual int typeID { get; set; }
    }

}
