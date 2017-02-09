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
        public FeatureController(IServiceProvider service, SignInManager<ApplicationUser> signinManager)
        {
            this.dbconOption = service.GetRequiredService<DbContextOptions<ApplicationDbContext>>();
            this._signinManager = signinManager;
        }

#region 功能点获取

        /// <summary>
        /// 获取功能点
        /// </summary>
        /// <param name="projectId">功能块ID</param>
        /// <param name="id">功能点ID</param>
        /// <returns></returns>
        [HttpGetAttribute]
        [RouteAttribute("/Project/{projectId}/Feature/{id?}")]
        public IActionResult FManage(int projectId, int id)
        {
            using (ApplicationDbContext dbcon = new ApplicationDbContext(dbconOption))
            {
                List<Features> data = new List<Features>();
                //优先级为单个的功能点ID,若功能点ID为0,则取功能点ID
                data = dbcon.Features.Where(p => (id == 0 ? p.projectId == projectId : p.featuresId == id)).Include(p => p.featuresDepends).ToList();
                if (data.Count == 0)
                {
                    ViewData["FeatureData"] = "{}";
                    ViewData["ProjectId"] = projectId;
                }
                else
                {
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
        public IActionResult Post([FromBody]JObject jsonObj)
        {
            dynamic Jsondm = jsonObj;

            Features FeatureEntity = Jsondm.FeatureEntity.ToObject<Features>();

            FeaturesDepend[] FeatureDepLst = Jsondm.FeatureDepLst.ToObject<FeaturesDepend[]>();
            using (ApplicationDbContext dbcon = new ApplicationDbContext(dbconOption))
            {
                dbcon.Database.BeginTransaction();
                try
                {
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

#region 功能点修改

        [HttpPut]
        [Route("/Feature/{jsonObj?}")]
        public IActionResult Put([FromBody]JObject jsonObj)
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
                    dbcon.Entry(FeatureEntity).Property(x => x.featuresHtml).IsModified = false;
                    dbcon.Entry(FeatureEntity).Property(x => x.featuresScript).IsModified = false;
                    dbcon.Entry(FeatureEntity).Property(x => x.featuresCss).IsModified = false;

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

#region HTML编辑器获取
        [HttpGetAttribute]
        [RouteAttribute("/Feature/HTML/{fid}")]
        public IActionResult HtmlEdit(int fid)
        {
            using (ApplicationDbContext dbcon = new ApplicationDbContext(dbconOption))
            {
                Features entity = dbcon.Features.Where(f => f.featuresId == fid).FirstOrDefault();
                //TODO: 如果没有查到对象,返回到前台会报错,这里需要做个非空处理,跳转到异常的页面
                return View(entity);
            }
        }

#endregion


#region 获取依赖项内容


        [HttpGetAttribute]
        [RouteAttribute("/Feature/Depends/{fEntity?}")]
        public IActionResult GetDepends(Features fEntity){
            Dictionary<string, List<FeatureDepModel>> dict = new Dictionary<string, List<FeatureDepModel>>();
            //  dictModel =new List<FeatureDepModel>();

            using (ApplicationDbContext dbcon = new ApplicationDbContext(dbconOption))
            {
                //获取功能点依赖项目列表
                List<FeaturesDepend> fDepList = dbcon.FeaturesDepends.Where(d => d.featuresId == fEntity.featuresId).ToList();
                //获取功能块依赖屏蔽列表
                List<FeaIgnoreProDepend> depIgnoreList =  dbcon.FeaIgnoreProDepends.Where(d => d.featuresId == fEntity.featuresId).ToList();
                //获取功能块依赖列表
                List<ProjectDepend> pDepList = dbcon.ProjectDepends.Where(d => d.projectId == fEntity.projectId).ToList();

                dict.Add("text/javascript",new List<FeatureDepModel>());
                dict.Add("text/css",new List<FeatureDepModel>());
                foreach(ProjectDepend pd in pDepList){

                    if(depIgnoreList.Where(d => d.projectDependid == pd.dependId).Count() == 0){

                        if(pd.filePath ==null || pd.fileName ==null){
                            continue;
                        }
                        FeatureDepModel tempEntity = new FeatureDepModel();
                        tempEntity.depSrc =pd.filePath;
                        tempEntity.depName =pd.fileName;
                        if(!dict[pd.fileType].Contains(tempEntity)){
                            dict[pd.fileType].Add(tempEntity);
                        }
                    }
                }

                foreach (FeaturesDepend fd in fDepList)
                {
                    if (fd.filePath == null || fd.fileName == null)
                    {
                        continue;
                    }
                    FeatureDepModel tempEntity = new FeatureDepModel();
                    tempEntity.depSrc = fd.filePath;
                    tempEntity.depName = fd.fileName;
                    if (!dict[fd.fileType].Contains(tempEntity))
                    {
                        dict[fd.fileType].Add(tempEntity);
                    }
                }
            }
            return Json(dict);
        }

#endregion

#region HTML代码保存

        [HttpPut]
        [Route("/Feature/HTML/{FeatureEntity?}")]
        public IActionResult HTMLPut(Features FeatureEntity)
        {

            using (ApplicationDbContext dbcon = new ApplicationDbContext(dbconOption))
            {
                try
                {
                    dbcon.Features.Attach(FeatureEntity);
                    dbcon.Entry(FeatureEntity).State = EntityState.Unchanged;
                    dbcon.Entry(FeatureEntity).Property(x => x.featuresHtml).IsModified = true;
                    dbcon.Entry(FeatureEntity).Property(x => x.featuresScript).IsModified = true;
                    dbcon.Entry(FeatureEntity).Property(x => x.featuresCss).IsModified = true;
                    
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



#region API接口,未开始

        [HttpGet]
        [RouteAttribute("/API/Project/{id?}")]
        public IActionResult ApiGet(int id)
        {
            return Json("ok");
        }


#endregion

    }

}