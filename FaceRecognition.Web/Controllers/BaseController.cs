using System;
using NLog.Web;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;

namespace FaceRecognition.Web.Controllers
{
    public abstract class BaseController : Controller
    {
        protected readonly NLog.ILogger Logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
    }    
}