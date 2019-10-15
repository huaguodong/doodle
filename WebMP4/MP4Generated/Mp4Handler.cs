
using MP4Generated;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using WebMP4.Models;

namespace WebMP4.MP4Generated
{
    public class Mp4Handler
    {
        private string fileText = "";
        private List<string> BGList = new List<string>();
        public Mp4Handler(string folderFullName,Size size, double time)
        {
            int i = 0;
            string imgName = "";
            string fileName = folderFullName + "temp\\";
            try
            {
                if (!Directory.Exists(fileName))
                {
                    Directory.CreateDirectory(fileName);
                }
                fileText = string.Format("{0}ImageList{1} ", fileName, ".txt");
                FileStream fs = new FileStream(fileText, FileMode.Create);
                StreamWriter sw = new StreamWriter(fs);
                DirectoryInfo folder = new DirectoryInfo(folderFullName);
                foreach (FileInfo file in folder.GetFiles())
                {
                    if (file.Extension.Equals(".png") ||
                        file.Extension.Equals(".jpg"))
                    {
                        Bitmap img = new Bitmap(file.FullName);
                        Bitmap bg = new Bitmap(img, size.Width, size.Height);
                        imgName = fileName + i + ".png";
                        i++;
                        img.Dispose();
                        bg.Save(imgName);
                        imgName = string.Format("file '{0}'", imgName);
                        sw.WriteLine(imgName);
                        string duration = string.Format("duration {0}", time);
                        sw.WriteLine(duration);
                        bg.Dispose();
                    }
                }
                sw.WriteLine(imgName);
                sw.Flush();
                sw.Close();
                fs.Close();
                
            } catch(Exception ex)
            {
                Console.WriteLine(ex.Message);

            }


        }
        public void CreateMP4(AnimeInfo animeinfo)
        {
            ProcessFFMPEG.MakeVideoFromTextFile(animeinfo, fileText);
        }
    }
}