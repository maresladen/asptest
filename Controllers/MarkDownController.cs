using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebApplication.Data;
using WebApplication.Models;
using WebApplication.Models.ManageViewModels;
using WebApplication.Services;



namespace WebApplication.Controllers
{
    // 角色认证
    // [Authorize(Roles = "NormalUser")]
    // claim认证 需要在中间件注册时，增加此身份信息
    // [Route("Project")]
    [Authorize(Policy = "manager")]
    public class MarkDownController : Controller
    {
        private const string strProject = "Project";
        private const string strFeature = "Feature";
        // ApplicationDbContext Dbcon;
        DbContextOptions<ApplicationDbContext> dbconOption;

        public MarkDownController(IServiceProvider service)
        {
            this.dbconOption = service.GetRequiredService<DbContextOptions<ApplicationDbContext>>();
        }



#region markDown获取

        [HttpGetAttribute]
        [RouteAttribute("/Project/{pfid}/MarkDown/{id?}")]
        public IActionResult PMarkDown(int pfid, int id)
        {
            MarkDown mdEntity;

            using (ApplicationDbContext dbcon = new ApplicationDbContext(dbconOption))
            {
                mdEntity = dbcon.MarkDowns.Where(d => d.MdId == id).FirstOrDefault();
            }
            ViewData["pfType"] =  strProject;
            ViewData["pfId"] =  pfid;
            if(mdEntity == null){
                mdEntity =new MarkDown();
                mdEntity.MdText="";
                mdEntity.MdHTML="";
            }
            return View("MarkDown",mdEntity);
        }

        [HttpGetAttribute]
        [RouteAttribute("/Feature/{pfid}/MarkDown/{id?}")]
        public IActionResult FMarkDown(int pfid, int id)
        {
            MarkDown mdEntity;

            using (ApplicationDbContext dbcon = new ApplicationDbContext(dbconOption))
            {
                mdEntity = dbcon.MarkDowns.Where(d => d.MdId == id).FirstOrDefault();
            }
            ViewData["pfType"] =  strFeature;
            ViewData["pfId"] =  pfid;
            if(mdEntity == null){
                mdEntity =new MarkDown();
                mdEntity.MdText="";
                mdEntity.MdHTML="";
            }
            return View("MarkDown",mdEntity);
        }

        [HttpGetAttribute]
        [RouteAttribute("/HELP/MarkDown")]
        [AllowAnonymousAttribute]
        public IActionResult MarkDownHelp(int pfid, int id)
        {
            ViewData["HELP"] =  "TRUE";
            return View("MarkDown",new MarkDown());
        }

#endregion

#region markDown新增

        [HttpPostAttribute]
        [RouteAttribute("/MarkDown/{jsonObj?}")]
        public IActionResult post([FromBody]JObject jsonObj)
        {
            dynamic Jsondm = jsonObj;

            MarkDown mdEntity = Jsondm.mdEntity.ToObject<MarkDown>();
            mdEntity.MdText = mdEntity.MdText.Replace(@"\", "|BETAFUN|");
            string strtype = Jsondm.saveType.ToObject<string>();
            int typeId = Jsondm.saveId.ToObject<int>();

            using (ApplicationDbContext dbcon = new ApplicationDbContext(dbconOption))
            {
                //开始事物
                dbcon.Database.BeginTransaction();
                try
                {
                    dbcon.MarkDowns.Add(mdEntity);
                    dbcon.SaveChanges();
                    int mdId = mdEntity.MdId;
                    if (strtype == strProject)
                    {
                        Project tempEntity = new Project();
                        tempEntity.projectId = typeId;
                        tempEntity.mdId = mdId;
                        dbcon.Projects.Attach(tempEntity);
                        dbcon.Entry(tempEntity).State = EntityState.Unchanged;
                        dbcon.Entry(tempEntity).Property(x => x.mdId).IsModified = true;

                    }
                    else if (strtype == strFeature)
                    {
                        Features tempEntity = new Features();
                        tempEntity.featuresId = typeId;
                        tempEntity.mdId = mdId;
                        dbcon.Features.Attach(tempEntity);
                        dbcon.Entry(tempEntity).State = EntityState.Unchanged;
                        dbcon.Entry(tempEntity).Property(x => x.mdId).IsModified = true;
                    }
                    dbcon.SaveChanges();
                }
                catch (Exception ex)
                {
                    dbcon.Database.RollbackTransaction();
                    throw ex;
                }
                dbcon.Database.CommitTransaction();
            }

            return Json("successs");
        }

#endregion

#region markDwon修改
        [HttpPut]
        [Route("/MarkDown/{mdEntity?}")]
        public IActionResult MdPutTest(MarkDown mdEntity)
        {
            using (ApplicationDbContext dbcon = new ApplicationDbContext(dbconOption))
            {
                try
                {
                    mdEntity.MdText = mdEntity.MdText.Replace(@"\","|BETAFUN|");
                    dbcon.MarkDowns.Update(mdEntity);
                    dbcon.SaveChanges();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return Json("successs");
        }



 #endregion


        #region API接口,未开始

        [HttpGetAttribute]
        [RouteAttribute("/API/MarkDown/{id?}")]
        [AllowAnonymousAttribute]
        public IActionResult JsonMarkDown(int id)
        {
            MarkDown mdEntity;

            using (ApplicationDbContext dbcon = new ApplicationDbContext(dbconOption))
            {
                mdEntity = dbcon.MarkDowns.Where(d => d.MdId == id).FirstOrDefault();
                mdEntity.MdText ="";
            }
            if(mdEntity == null){
                mdEntity =new MarkDown();
                mdEntity.MdText="";
                mdEntity.MdHTML="";
            }
            return Json(mdEntity);
        }

        #endregion

    }

}