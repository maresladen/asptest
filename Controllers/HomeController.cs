using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using WebApplication.Data;
using WebApplication.Models;

namespace WebApplication.Controllers
{
    public class HomeController : Controller
    {
        ApplicationDbContext dbcon;
        DbContextOptions<ApplicationDbContext> dbconOption;

        private readonly UserManager<ApplicationUser> _userManager;
        public HomeController(IServiceProvider service,UserManager<ApplicationUser> userManager)
        {
            this.dbconOption = service.GetRequiredService<DbContextOptions<ApplicationDbContext>>();
            this._userManager =userManager;
            // dbcon = new TestDbContext(service.GetRequiredService<DbContextOptions<TestDbContext>>());
        }

        public async Task<IActionResult> MysqlEmp(string id)
        {
            using (dbcon = new ApplicationDbContext(dbconOption))
            {
                // var Emps = dbcon.Employees.ToList();
                // 获取url地址信息
                // string url= Request.Path.Value; 

                //Session的读写记录,Session的使用以后通过Identity替代
                // var tempBytes =System.Text.Encoding.Unicode.GetBytes(id);
                // HttpContext.Session.Set("id",tempBytes);

                // var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "奥巴马") }, CookieAuthenticationDefaults.AuthenticationScheme));
                // await HttpContext.Authentication.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, user);

                var loginId = _userManager.GetUserId(User);

                if(loginId != null){
                    ViewData["idtest"] = User.Claims.ToList()[2].Value;
                }
                else{
                    ViewData["idtest"] = "找不到";
                }


                var query = from e in dbcon.Users
                            join ur in dbcon.UserRoles on e.Id equals ur.UserId
                            join r in dbcon.Roles on ur.RoleId equals r.Id
                            where e.Id == loginId
                            select new { e, r.Name };


                            //通过传入的页和请求条目量取数据
                            // dbcon.Users.Skip(1*10).Take(10);


                var result = await query.ToListAsync();

                var finRes = JsonConvert.SerializeObject(result, Formatting.None);
                ViewData["testData"] = finRes;
                return View(await dbcon.Employees.ToListAsync());
            }
        }

        [HttpPostAttribute]
        public IActionResult UpLastName(Employee eEntity){
            using(dbcon = new ApplicationDbContext(dbconOption)){
                //更新部分字段
                dbcon.Employees.Attach(eEntity);
                // dbcon.Entry(eEntity).Property(x => x.LastName).IsModified =true;
                // dbcon.Entry(eEntity).State = EntityState.Unchanged;

                //更新部分字段的另一种方式
                dbcon.Entry(eEntity).State = EntityState.Modified;
                dbcon.Entry(eEntity).Property(x => x.Name).IsModified =false;

                dbcon.SaveChanges();
                return Json(eEntity);
            }
        }

        public IActionResult MdTest()
        {
            return View();
        }

        public IActionResult Index()
        {
            using (ApplicationDbContext dbcon = new ApplicationDbContext(dbconOption))
            {
                List<Project> prodata = new List<Project>();
                prodata = dbcon.Projects.Include(p => p.lstfeatures).ToList();
                return View(prodata);
            }
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
