using System;
using Microsoft.EntityFrameworkCore;
using WebApplication.Data;

namespace WebApplication.Controllers
{
    internal enum saveType
    {
        insert = 0,
        update = 1,
        delete = 2
    }


    internal class DBUtility
    {
        // internal delegate void saveFun(params object[] args);


        // internal static bool Dbtrans(saveFun dlgFun, ApplicationDbContext dbcon,params object[] objlist)
        // {
        //     dbcon.Database.BeginTransaction();

        //     try
        //     {
        //         dlgFun.Invoke(objlist);
        //         dbcon.Database.CommitTransaction();
        //     }
        //     catch (Exception)
        //     {
        //         dbcon.Database.RollbackTransaction();
        //         return false;
        //     }

        //     return true;
        // }
    }

}