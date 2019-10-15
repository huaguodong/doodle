using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebMP4.MP4Generated;
using WebMP4.Models;


namespace WebMP4.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        public void mp4Generated()
        {
            AnimeInfo animeinfo = new AnimeInfo();
            animeinfo.IntFPS = Request["frame"];
            animeinfo.Keyframe = Request["keyframe"];
            animeinfo.Channel = Request.Form.Get("channel");
            animeinfo.Bitrate = Request.Form.Get("bitrate");
            animeinfo.SamplingRate = Request.Form.Get("SamplingRate");
            string folderName = @"D:\BaiduNetdiskDownload\WebMP4\WebMP4\MP4Generated\BGImg\";
            string savePath = @"D:\BaiduNetdiskDownload\WebMP4\WebMP4\MP4Generated\Results\";
            string curTime = DateTime.Now.ToString("hh_mm_ss_ms");
            animeinfo.FileOut = string.Format(@"{0}{1}.mp4", savePath, curTime);
            Size size = new Size(1600, 900); //视频宽高
            double time = 3;//每张图片播放时间
            Mp4Handler mp4handler = new Mp4Handler(folderName, size, time);
            mp4handler.CreateMP4(animeinfo);
        }
    }
}