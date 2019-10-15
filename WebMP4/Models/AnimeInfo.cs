using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebMP4.Models
{
    public class AnimeInfo
    {
        private string intFPS = "24";
        public string IntFPS
        {
            get
            {
                return intFPS;
            }
            set
            {
                intFPS = value;
            }
        }
        private string qualityV = "2500";
        /// <summary>
        /// 视频质量 -b:v 2500k
        /// </summary>
        public string QualityV
        {
            get
            {
                return qualityV;
            }

            set
            {
                qualityV = value;
            }
        }
        private string qualityA = "9";
        /// <summary>
        /// 音频质量 -q:a 9
        /// </summary>
        public string QualityA
        {
            get
            {
                return qualityA;
            }

            set
            {
                qualityA = value;
            }
        }
        private string keyframe = "30";
        public string Keyframe
        {
            get
            {
                return keyframe;
            }

            set
            {
                keyframe = value;
            }
        }
        private string channel = "1";
        public string Channel
        {
            get
            {
                return channel;
            }

            set
            {
                channel = value;
            }
        }
        private string samplingrate = "44100";
        public string SamplingRate
        {
            get
            {
                return samplingrate;
            }

            set
            {
                samplingrate = value;
            }
        }
        private string bitrate = "128";
        public string Bitrate
        {
            get
            {
                return bitrate;
            }

            set
            {
                bitrate = value;
            }
        }

        private string fileOut = "";
        public string FileOut
        {
            get
            {
                return fileOut;
            }

            set
            {
                fileOut = value;
            }
        }




    }
}