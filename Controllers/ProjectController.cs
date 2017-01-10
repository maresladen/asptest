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
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace WebApplication.Controllers
{
    // 角色认证
    // [Authorize(Roles = "NormalUser")]
    // claim认证 需要在中间件注册时，增加此身份信息
    [Authorize(Policy = "manager")]
    public class ProjectController : Controller
    {

        ApplicationDbContext Dbcon;
        DbContextOptions<ApplicationDbContext> dbconOption;

        private readonly SignInManager<ApplicationUser> _signinManager;
        public ProjectController(IServiceProvider service,SignInManager<ApplicationUser> signinManager)
        {
            this.dbconOption = service.GetRequiredService<DbContextOptions<ApplicationDbContext>>();
            this._signinManager =signinManager;
        }

        public IActionResult Manage()
        {
            // if(string.IsNullOrEmpty(User.Identity.Name)){
            //      return RedirectToAction(nameof(HomeController.Index), "Home");
            // }
            return View();
        }

        public IActionResult ProSave([FromBody]JObject  jobj){
            dynamic Jsondm =jobj;

            Project proEntity = Jsondm.proEntity.ToObject<Project>();

            ProjectDepend[] proDepLst = Jsondm.proDepLst.ToObject<ProjectDepend[]>();
            // using (var scope = new TransactionScope())
            // { }
            using (Dbcon = new ApplicationDbContext(dbconOption)){
                Dbcon.Database.BeginTransaction();
                try{
                    if (proEntity.projectId != 0)
                    {
                        //这里做update
                        Dbcon.Projects.Update(proEntity);
                        //考虑到性能，ef的批量删除性能太差，暂时用明文sql写，也可以使用ef utility工具，以后再做尝试
                        string strDel = string.Format( "delete from ProjectDepend where projectId ={0};",proEntity.projectId);
                        Dbcon.Database.ExecuteSqlCommand(strDel);

                        if (proDepLst.Length > 0)
                        {
                            foreach (ProjectDepend ent in proDepLst)
                            {
                                ent.projectId = proEntity.projectId;
                                Dbcon.ProjectDepends.Add(ent);
                            }
                        }
                    }
                    else{
                        Dbcon.Projects.Add(proEntity);
                        Dbcon.SaveChanges();
                        if (proDepLst.Length > 0)
                        {
                            foreach (ProjectDepend ent in proDepLst)
                            {
                                ent.projectId = proEntity.projectId;
                                Dbcon.ProjectDepends.Add(ent);
                            }
                        }
                    }
                    Dbcon.SaveChanges();
                    Dbcon.Database.CommitTransaction();
                }
                catch(Exception ex){
                    Dbcon.Database.RollbackTransaction();
                    throw ex;
                }
            }
            return Json("successs");
        }
   }
}