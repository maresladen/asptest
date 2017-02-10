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
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace WebApplication.Controllers
{
    // 角色认证
    // [Authorize(Roles = "NormalUser")]
    // claim认证 需要在中间件注册时，增加此身份信息
    // [Route("Project")]
    [Authorize(Policy = "manager")]
    public class ProjectController : Controller
    {

        const string upPath ="http://localhost:5000/uponloads/Project/";

        // ApplicationDbContext Dbcon;
        DbContextOptions<ApplicationDbContext> dbconOption;
        IHostingEnvironment _env;
        private readonly SignInManager<ApplicationUser> _signinManager;
        public ProjectController(IHostingEnvironment env,IServiceProvider service,SignInManager<ApplicationUser> signinManager)
        {
            this._env = env;
            this.dbconOption = service.GetRequiredService<DbContextOptions<ApplicationDbContext>>();
            this._signinManager =signinManager;
        }

#region 功能块获取

        [HttpGetAttribute]
        [RouteAttribute("/Project/{id?}")]
        //就是一个注解
        public IActionResult Manage(int? id)
        {
            using (ApplicationDbContext dbcon = new ApplicationDbContext(dbconOption)){
                
                // List<Project> prodata = dbcon.Projects.Include(p => p.projectDepends).ToList();
                List<Project> prodata =new List<Project>();
                if (id == null)
                {
                    prodata = dbcon.Projects.Include(p => p.projectDepends).ToList();                }
                else
                {
                    prodata = dbcon.Projects.Where(p => p.projectId == id).Include(p => p.projectDepends).ToList();
                }
                if (prodata.Count == 0)
                {
                    ViewData["proData"] = "{}";
                }
                else{
                    ViewData["proData"] = JsonConvert.SerializeObject(prodata);
                }
            }
            return View();
        }

#endregion
        
#region 功能块新增

        // [HttpPost]
        // [Route("/Project/{jsonObj?}")]
        // public IActionResult Post([FromBodyAttribute]JObject  jsonObj)
        // {
        //     dynamic Jsondm = jsonObj;

        //     Project proEntity = Jsondm.proEntity.ToObject<Project>();

        //     ProjectDepend[] proDepLst = Jsondm.proDepLst.ToObject<ProjectDepend[]>();


        //     //todo: 接收文件上传,保存到静态文件夹,文件名通过guid生成,guid比较靠谱
        //     //todo: 将地址赋值给depend的内容,然后保存
        //     //depFinfo
        //     //depFile

        //     using (ApplicationDbContext dbcon = new ApplicationDbContext(dbconOption))
        //     {
        //         dbcon.Database.BeginTransaction(); 
        //         try{
        //             dbcon.Projects.Add(proEntity);
        //             dbcon.SaveChanges();
        //             if (proDepLst.Length > 0)
        //             {
        //                 foreach (ProjectDepend ent in proDepLst)
        //                 {
        //                     // var strName =this.saveFiles(ent);
                            
        //                     ent.projectId = proEntity.projectId;
        //                     dbcon.ProjectDepends.Add(ent);
        //                 }
        //             }
        //             dbcon.SaveChanges();
        //             dbcon.Database.CommitTransaction();
        //         }
        //         catch(Exception){
        //            dbcon.Database.RollbackTransaction(); 
        //            return Json("faild");
        //         }
        //     }
            
        //     //这里应该把重新封装json然后返回回去
        //     return Json("successs");
        // }
#endregion

#region 功能块修改

        [HttpPut]
        [Route("/Project/{jsonObj?}")]
        public IActionResult Put([FromFormAttribute]IFormCollection theForm)
        {

            Project proEntity =new Project();
            proEntity.projectId = Convert.ToInt32(theForm["proId"]);
            proEntity.projectName = theForm["proName"];

            var files = theForm.Files;

            using (ApplicationDbContext dbcon = new ApplicationDbContext(dbconOption))
            {
                try
                {
                    dbcon.Projects.Attach(proEntity);
                    dbcon.Entry(proEntity).State = EntityState.Modified;
                    dbcon.Entry(proEntity).Property(x => x.projectName).IsModified = false;

                    //考虑到性能，ef的批量删除性能太差，暂时用明文sql写，也可以使用ef utility工具，以后再做尝试
                    string strDel = string.Format("delete from ProjectDepend where projectId ={0};", proEntity.projectId);
                    dbcon.Database.ExecuteSqlCommand(strDel);

                    string savePath = Path.Combine(_env.ContentRootPath, "wwwroot/uponloads/Project/" + proEntity.projectId.ToString());
                    if(Directory.Exists(savePath)){
                        Directory.Delete(savePath);
                        string[] fileinfo = Directory.GetFiles(savePath);
                        foreach(string strFileName in fileinfo){
                            System.IO.File.Delete(strFileName);
                        }
                    }
                    foreach (var file in files){
                        var fileType = file.ContentType;
                        var fileName = file.FileName;
                        var guidName = Guid.NewGuid().ToString() + (fileType == "text/javascript" ? ".js" : ".css");
                        using (var stream = new FileStream( Path.Combine(savePath ,guidName), FileMode.CreateNew))
                        {
                            file.CopyTo(stream);
                            stream.Flush();
                        }
                       ProjectDepend  ent =new ProjectDepend();
                        ent.projectId = proEntity.projectId;
                        ent.fileName =fileName;
                        ent.filePath = upPath +proEntity.projectId.ToString() +"/" + guidName;
                        ent.fileType = fileType;
                        dbcon.ProjectDepends.Add(ent);
                    }
                    dbcon.SaveChanges();
                }
                catch (Exception)
                {
                    return Json("faild");
                }
            }
            return Json("successs");
        }

#endregion

#region 功能块删除

        [HttpDelete]
        [Route("/Project/{id}")]
        public IActionResult delete(int id)
        {
            using (ApplicationDbContext dbcon = new ApplicationDbContext(dbconOption))
            {
                try
                { 
                    Project proEntity = new Project();
                    proEntity.projectId = id;
                    dbcon.Projects.Remove(proEntity);
                    //考虑到性能，ef的批量删除性能太差，暂时用明文sql写，也可以使用ef utility工具，以后再做尝试
                    string strDel = string.Format("delete from ProjectDepend where projectId ={0};", proEntity.projectId);
                    dbcon.Database.ExecuteSqlCommand(strDel);
                    dbcon.SaveChanges();

                    string savePath = Path.Combine(_env.ContentRootPath, "wwwroot/uponloads/Project/" + proEntity.projectId.ToString());
                    if(Directory.Exists(savePath)){
                        Directory.Delete(savePath,true);
                    }
                }
                catch (Exception ex)
                {
                    return Json(ex.Message);
                }
            }
            return Json("successs");
        }

#endregion

#region markDwon获取

        // [HttpGetAttribute]
        // [RouteAttribute("/MarkDown/{id}")]
        // public IActionResult MarkDown(int id)
        // {
        //     //如果id为simple,忽略大小写,传入内容选择调用simple
        //     //否则则通过id到数据库中查询,查询数据表为projectDepend表
        //     Project proEntity = new Project();
        //     if(id == -1){
        //         ViewData["simple"] = "1";
        //     }
        //     else{
        //         ViewData["simple"] = "0";
        //         using (ApplicationDbContext dbcon = new ApplicationDbContext(dbconOption))
        //         {
        //              proEntity = dbcon.Projects.Where(d => d.projectId == id).FirstOrDefault();
        //             // if (proEntity != null)
        //             // {
        //             //     // proEntity.projectMarkDown =string.Empty;
                        
        //             //     ViewData["prodata"] = proEntity.projectMdText;
        //             // }
        //         }
        //     }
        //    return View(proEntity);
        // }

#endregion

#region markDwon修改


        // [HttpPut]
        // [Route("/MarkDownTest/{id?}")]
        // public IActionResult MdPut(int id)
        // {
        //     using (ApplicationDbContext dbcon = new ApplicationDbContext(dbconOption))
        //     {
        //         try
        //         {
        //             var tempForm = Request.Form;

        //             Project proEntity =  new Project();
        //             proEntity.projectId = id;
        //             proEntity.projectMdText = tempForm["projectMdText"];
        //             proEntity.projectMarkDown =  tempForm["projectMarkDown"];

        //             dbcon.Projects.Attach(proEntity);
        //             dbcon.Entry(proEntity).State = EntityState.Unchanged;
        //             dbcon.Entry(proEntity).Property(x => x.projectMdText).IsModified = true;
        //             dbcon.Entry(proEntity).Property(x => x.projectMarkDown).IsModified = true;
        //             dbcon.SaveChanges();
        //         }
        //         catch (Exception ex)
        //         {
        //             throw ex;
        //         }
        //     }
        //     return Json("successs");
        // }

        // [HttpPut]
        // [Route("/MarkDown/{proEntity?}")]
        // public IActionResult MdPutTest(Project proEntity)
        // {
        //     using (ApplicationDbContext dbcon = new ApplicationDbContext(dbconOption))
        //     {
        //         try
        //         {
        //             // var tempForm = Request.Form;

        //             // Project proEntity =  new Project();
        //             // proEntity.projectId = id;
        //             // proEntity.projectMdText = tempForm["projectMdText"];
        //             // proEntity.projectMarkDown =  tempForm["projectMarkDown"];

        //             proEntity.projectMdText = proEntity.projectMdText.Replace(@"\","|BETAFUN|");
        //             dbcon.Projects.Attach(proEntity);

        //             dbcon.Entry(proEntity).State = EntityState.Unchanged;
        //             dbcon.Entry(proEntity).Property(x => x.projectMdText).IsModified = true;
        //             dbcon.Entry(proEntity).Property(x => x.projectMarkDown).IsModified = true;
        //             dbcon.SaveChanges();
        //         }
        //         catch (Exception ex)
        //         {
        //             throw ex;
        //         }
        //     }
        //     return Json("successs");
        // }



#endregion


#region API接口,未开始

        [HttpGetAttribute]
        [RouteAttribute("/API/Project/{id?}")]
        [AllowAnonymous]
        public IActionResult ApiGet(int? id){
            using (ApplicationDbContext dbcon = new ApplicationDbContext(dbconOption))
            {
                List<Project> prodata = new List<Project>();
                if (id == null)
                {
                    prodata = dbcon.Projects.Include(p => p.lstfeatures).ToList();
                }
                else
                {
                    prodata = dbcon.Projects.Where(p => p.projectId == id).Include(p => p.lstfeatures).ToList();
                }
                return Json(prodata);
            }
        }



        [RouteAttribute("/Project")]
        [HttpPostAttribute]
        public IActionResult PostFile([FromFormAttribute]IFormCollection theForm)
        {
            var files = theForm.Files;
            Project proEntity = new Project();
            proEntity.projectName = theForm["proName"];
            using (ApplicationDbContext dbcon = new ApplicationDbContext(dbconOption))
            {
                dbcon.Database.BeginTransaction();
                try
                {
                    dbcon.Projects.Add(proEntity);
                    dbcon.SaveChanges();
                    string savePath = Path.Combine(_env.ContentRootPath, "wwwroot/uponloads/Project/" + proEntity.projectId.ToString());
                    if(!Directory.Exists(savePath)){
                        Directory.CreateDirectory(savePath);
                    }
                    foreach (var file in files)
                    {
                        var fileType = file.ContentType;
                        var fileName = file.FileName;
                        var guidName = Guid.NewGuid().ToString() + (fileType == "text/javascript" ? ".js" : ".css");
                        using (var stream = new FileStream( Path.Combine(savePath ,guidName), FileMode.CreateNew))
                        {
                            file.CopyTo(stream);
                            stream.Flush();
                        }
                       ProjectDepend  ent =new ProjectDepend();
                        ent.projectId = proEntity.projectId;
                        ent.fileName =fileName;
                        ent.filePath = upPath +proEntity.projectId.ToString() +"/" + guidName;
                        ent.fileType = fileType;
                        dbcon.ProjectDepends.Add(ent);

                    }
                    dbcon.SaveChanges();
                    dbcon.Database.CommitTransaction();
                }
                catch (Exception)
                {
                    dbcon.Database.RollbackTransaction();
                    return Json("faild");
                }
            }

            //这里应该把重新封装json然后返回回去
            return Json("successs");

        }

#endregion
   }
}