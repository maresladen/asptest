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
using Newtonsoft.Json.Linq;
using WebApplication.Data;
using WebApplication.Models;
using Microsoft.AspNetCore.Authorization;

namespace WebApplication.Controllers
{
    // 角色认证
    // [Authorize(Roles = "NormalUser")]
    // claim认证 需要在中间件注册时，增加此身份信息
    [Route("Project")]
    [Authorize(Policy = "manager")]
    public class ProjectController : Controller
    {

        // ApplicationDbContext Dbcon;
        DbContextOptions<ApplicationDbContext> dbconOption;

        private readonly SignInManager<ApplicationUser> _signinManager;
        public ProjectController(IServiceProvider service,SignInManager<ApplicationUser> signinManager)
        {
            this.dbconOption = service.GetRequiredService<DbContextOptions<ApplicationDbContext>>();
            this._signinManager =signinManager;
        }

        [HttpGetAttribute]
        [RouteAttribute("/Project/{id?}")]
        public IActionResult Manage(int id)
        {
            using (ApplicationDbContext dbcon = new ApplicationDbContext(dbconOption)){
                // var query = from p in dbcon.Projects
                //                 var query = from e in dbcon.Users
                //             join ur in dbcon.UserRoles on e.Id equals ur.UserId
                //             join r in dbcon.Roles on ur.RoleId equals r.Id
                //             where e.Id == loginId
                //             select new { e, r.Name };


                //             //通过传入的页和请求条目量取数据
                //             // dbcon.Users.Skip(1*10).Take(10);


                // var result = await query.ToListAsync();

                // var finRes = JsonConvert.SerializeObject(result, Formatting.None);
                // ViewData["testData"] = finRes;
                // return View(await dbcon.Employees.ToListAsync());
                ViewData["proData"] = dbcon.Projects.ToListAsync();
            }
            return View();
        }
        
        [HttpPost]
        [Route("/Project/{id}")]
        public IActionResult Post([FromBody]JObject  jsonObj)
        {
            dynamic Jsondm = jsonObj;

            Project proEntity = Jsondm.proEntity.ToObject<Project>();

            ProjectDepend[] proDepLst = Jsondm.proDepLst.ToObject<ProjectDepend[]>();
            using (ApplicationDbContext dbcon = new ApplicationDbContext(dbconOption))
            {
                dbcon.Database.BeginTransaction(); 
                try{
                    dbcon.Projects.Add(proEntity);
                    dbcon.SaveChanges();
                    if (proDepLst.Length > 0)
                    {
                        foreach (ProjectDepend ent in proDepLst)
                        {
                            ent.projectId = proEntity.projectId;
                            dbcon.ProjectDepends.Add(ent);
                        }
                    }
                    dbcon.SaveChanges();
                    dbcon.Database.CommitTransaction();
                }
                catch(Exception){
                   dbcon.Database.RollbackTransaction(); 
                   return Json("faild");
                }
            }
            
            //这里应该把重新封装json然后返回回去
            return Json("successs");
        }


        [HttpPut]
        [Route("/Project/{id}")]
        public IActionResult Put([FromBody]JObject  jsonObj)
        {
            dynamic Jsondm = jsonObj;

            Project proEntity = Jsondm.proEntity.ToObject<Project>();

            ProjectDepend[] proDepLst = Jsondm.proDepLst.ToObject<ProjectDepend[]>();

            using (ApplicationDbContext dbcon = new ApplicationDbContext(dbconOption))
            {
                dbcon.Database.BeginTransaction();
                try
                {
                    dbcon.Projects.Update(proEntity);
                    //考虑到性能，ef的批量删除性能太差，暂时用明文sql写，也可以使用ef utility工具，以后再做尝试
                    string strDel = string.Format("delete from ProjectDepend where projectId ={0};", proEntity.projectId);
                    dbcon.Database.ExecuteSqlCommand(strDel);

                    if (proDepLst.Length > 0)
                    {
                        foreach (ProjectDepend ent in proDepLst)
                        {
                            ent.projectId = proEntity.projectId;
                            dbcon.ProjectDepends.Add(ent);
                        }
                    }
                }
                catch (Exception)
                {
                    dbcon.Database.RollbackTransaction();
                    return Json("faild");
                }
            }
            return Json("successs");
        }

        [HttpDelete]
        [Route("/Project/{id}")]
        public IActionResult delete(Project proEntity)
        {
            using (ApplicationDbContext dbcon = new ApplicationDbContext(dbconOption))
            {
                dbcon.Database.BeginTransaction();
                try
                { 
                    dbcon.Projects.Remove(proEntity);
                    //考虑到性能，ef的批量删除性能太差，暂时用明文sql写，也可以使用ef utility工具，以后再做尝试
                    string strDel = string.Format("delete from ProjectDepend where projectId ={0};", proEntity.projectId);
                    dbcon.Database.ExecuteSqlCommand(strDel);
                }
                catch (Exception)
                {
                    dbcon.Database.RollbackTransaction();
                }
            }
            return Json("successs");
        }


        #region API接口

        [HttpGet]
        [RouteAttribute("/API/Project/{id?}")]
        public IActionResult ApiGet(int id){
            return Json("ok");
        }


        #endregion
   }
}