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
    public class FeatureController : Controller
    {
        // ApplicationDbContext Dbcon;
        DbContextOptions<ApplicationDbContext> dbconOption;

        private readonly SignInManager<ApplicationUser> _signinManager;
        public FeatureController(IServiceProvider service,SignInManager<ApplicationUser> signinManager)
        {
            this.dbconOption = service.GetRequiredService<DbContextOptions<ApplicationDbContext>>();
            this._signinManager =signinManager;
        }

#region 功能点获取

        [HttpGetAttribute]
        [RouteAttribute("/Project/{projectId}/Feature/{id?}")]
        //就是一个注解
        public IActionResult FManage(int projectId,int id)
        {
            using (ApplicationDbContext dbcon = new ApplicationDbContext(dbconOption)){
                List<Features> data =new List<Features>();
                //优先级为单个的功能点ID,若功能点ID为0,则取功能点ID
                data = dbcon.Features.Where(p => (id == 0 ? p.projectId == projectId : p.featuresId == id)).Include(p => p.featuresDepends).ToList();
                if (data.Count == 0)
                {
                    ViewData["FeatureData"] = "{}";
                    ViewData["ProjectId"] = projectId;
                }
                else{
                    ViewData["FeatureData"] = JsonConvert.SerializeObject(data);
                    ViewData["ProjectId"] = projectId;
                }
            }
            return View();
        }

#endregion
        
#region 功能点新增

        [HttpPost]
        [Route("/Feature/{jsonObj?}")]
        public IActionResult Post([FromBody]JObject  jsonObj)
        {
            dynamic Jsondm = jsonObj;

            Features FeatureEntity = Jsondm.FeatureEntity.ToObject<Features>();

            FeaturesDepend[] FeatureDepLst = Jsondm.FeatureDepLst.ToObject<FeaturesDepend[]>();
            using (ApplicationDbContext dbcon = new ApplicationDbContext(dbconOption))
            {
                dbcon.Database.BeginTransaction(); 
                try{
                    dbcon.Features.Add(FeatureEntity);
                    dbcon.SaveChanges();
                    if (FeatureDepLst.Length > 0)
                    {
                        foreach (FeaturesDepend ent in FeatureDepLst)
                        {
                            ent.featuresId = FeatureEntity.featuresId;
                            dbcon.FeaturesDepends.Add(ent);
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
#endregion

#region 功能点修改

        [HttpPut]
        [Route("/Feature/{jsonObj?}")]
        public IActionResult Put([FromBody]JObject  jsonObj)
        {
            dynamic Jsondm = jsonObj;

            Features FeatureEntity = Jsondm.FeatureEntity.ToObject<Features>();

            FeaturesDepend[] FeatureDepLst = Jsondm.FeatureDepLst.ToObject<FeaturesDepend[]>();

            using (ApplicationDbContext dbcon = new ApplicationDbContext(dbconOption))
            {
                try
                {
                    dbcon.Features.Attach(FeatureEntity);
                    dbcon.Entry(FeatureEntity).State = EntityState.Modified;
                    dbcon.Entry(FeatureEntity).Property(x => x.featuresMdText).IsModified = false;
                    dbcon.Entry(FeatureEntity).Property(x => x.featuresMardDown).IsModified = false;

                    //考虑到性能，ef的批量删除性能太差，暂时用明文sql写，也可以使用ef utility工具，以后再做尝试
                    string strDel = string.Format("delete from FeaturesDepend where featuresId ={0};", FeatureEntity.featuresId);
                    dbcon.Database.ExecuteSqlCommand(strDel);

                    if (FeatureDepLst.Length > 0)
                    {
                        foreach (FeaturesDepend ent in FeatureDepLst)
                        {
                            ent.featuresId = FeatureEntity.featuresId;
                            dbcon.FeaturesDepends.Add(ent);
                        }
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

#region 功能点删除

        [HttpDelete]
        [Route("/Feature/{id}")]
        public IActionResult delete(int id)
        {
            using (ApplicationDbContext dbcon = new ApplicationDbContext(dbconOption))
            {
                try
                { 
                    Features featureEntity = new Features();
                    featureEntity.featuresId = id;
                    dbcon.Features.Remove(featureEntity);
                    //考虑到性能，ef的批量删除性能太差，暂时用明文sql写，也可以使用ef utility工具，以后再做尝试
                    string strDel = string.Format("delete from FeaturesDepend where featuresId ={0};", featureEntity.featuresId);
                    dbcon.Database.ExecuteSqlCommand(strDel);
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

#region markDwon获取

        [HttpGetAttribute]
        [RouteAttribute("/FMarkDown/{id}")]
        public IActionResult FMarkDown(int id)
        {
            //如果id为simple,忽略大小写,传入内容选择调用simple
            //否则则通过id到数据库中查询,查询数据表为projectDepend表
            Features featureEntity = new Features();
            if(id == -1){
                ViewData["simple"] = "1";
            }
            else{
                ViewData["simple"] = "0";
                using (ApplicationDbContext dbcon = new ApplicationDbContext(dbconOption))
                {
                     featureEntity = dbcon.Features.Where(d => d.featuresId == id).FirstOrDefault();
                }
            }
           return View(featureEntity);
        }

#endregion

#region markDwon修改
        [HttpPut]
        [Route("/FMarkDown/{featureEntity?}")]
        public IActionResult MdPutTest(Features featureEntity)
        {
            using (ApplicationDbContext dbcon = new ApplicationDbContext(dbconOption))
            {
                try
                {
                    featureEntity.featuresMdText = featureEntity.featuresMdText.Replace(@"\","|BETAFUN|");
                    dbcon.Features.Attach(featureEntity);

                    dbcon.Entry(featureEntity).State = EntityState.Unchanged;
                    dbcon.Entry(featureEntity).Property(x => x.featuresMdText).IsModified = true;
                    dbcon.Entry(featureEntity).Property(x => x.featuresMardDown).IsModified = true;
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

        [HttpGet]
        [RouteAttribute("/API/Project/{id?}")]
        public IActionResult ApiGet(int id){
            return Json("ok");
        }


#endregion
        
    }

}