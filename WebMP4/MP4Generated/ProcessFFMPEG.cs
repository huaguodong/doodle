//// +----------------------------------------------------------+
//// | File: NPCommon.ProcessFFMPEG.cs
//// +----------------------------------------------------------+
//// | 执行ffmpeg进程 
//// +----------------------------------------------------------+
//// | Copyright (C) 2016 Tenda, Inc. All rights reserved.
//// +----------------------------------------------------------+
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using WebMP4.Models;

namespace MP4Generated
{
    public static class ProcessFFMPEG
    {
        private static String strOutput = "";//输出提示信息

        /// <summary>
        /// 传入参数命令行执行ffmpeg.exe
        /// </summary>
        /// <param name="strArguments">命令行参数</param>
        /// <returns>命令行信息</returns>
        private static string RunProcess(string strArguments, BackgroundWorker bGworker = null, double dStepProgressBegin = 0, double dStepProgressTotal = 0)
        {
            int nTimeCount = 0;

            Process p = new Process();
            p.StartInfo.FileName = @"D:\BaiduNetdiskDownload\WebMP4\WebMP4\MP4Generated\FFMPEG\bin\ffmpeg.exe";
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.Arguments = strArguments;//执行参数
            p.StartInfo.UseShellExecute = false;// Set UseShellExecute to false for redirection.
            p.StartInfo.RedirectStandardError = true;
            p.ErrorDataReceived += new DataReceivedEventHandler((sender, e) =>
            {
                if (!String.IsNullOrEmpty(e.Data))
                {
                    var output = e.Data;
                    if (bGworker != null && bGworker.CancellationPending == false && dStepProgressTotal > 0)
                    {
                        if (output.Contains("Duration:"))
                        {
                            Int32 indexOfDuration = output.IndexOf("Duration");
                            Int32 indexOfBegin = output.IndexOf(":", indexOfDuration);
                            Int32 indexOfEnd = output.IndexOf(",", indexOfBegin);
                            var duration = output.Substring(indexOfBegin + 1, indexOfEnd - indexOfBegin - 1);
                            nTimeCount = GetDurationSec(duration);
                        }
                        if (nTimeCount > 0 && output.Contains("time="))
                        {
                            Int32 indexOfTime = output.IndexOf("time=");
                            Int32 indexOfBegin = output.IndexOf("=", indexOfTime);
                            Int32 indexOfEnd = output.IndexOf("bitrate", indexOfBegin);
                            string timeStr = output.Substring(indexOfBegin + 1, indexOfEnd - indexOfBegin - 1);
                            var time = GetDurationSec(timeStr);
                            if (time > nTimeCount)
                                time = nTimeCount;
                            if (bGworker.IsBusy)
                                bGworker.ReportProgress(Convert.ToInt32(dStepProgressBegin + dStepProgressTotal * time / nTimeCount));
                        }
                    }
                    strOutput += output;
                }
            });

            using (p)
            {
                try
                {
                    p.Start();
                    strOutput = "";
                    p.BeginErrorReadLine();
                    while (true)
                    {
                        if (p.HasExited)
                        {
                            p.WaitForExit(50);
                            p.Close();
                            p.Dispose();
                            break;
                        }
                        else if (bGworker != null && bGworker.CancellationPending)
                        {
                            p.Kill();
                            p.WaitForExit(50);
                            p.Close();
                            p.Dispose();
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    strOutput = ex.Message;
                    p.Close();
                    p.Dispose();
                }
                return strOutput;
            }
        }
        /// <summary>
        /// 将时间字符串转换为秒数.
        /// </summary>
        private static int GetDurationSec(string input)
        {
            int totalSecond = 0;
            try
            {
                string[] split = input.Split(new char[] { ':', '.' });
                int hour = int.Parse(split[0]) * 3600;
                int min = int.Parse(split[1]) * 60;
                int second = int.Parse(split[2]);
                int millisecond = int.Parse(split[3]) / 1000;
                totalSecond = hour + min + second + millisecond;
            }
            catch (System.Exception ex)
            {
                Console.Write(ex.Message);

            }
            return totalSecond;
        }
        //        /// <summary>
        //        /// 获取字符串的Duration毫秒值.
        //        /// </summary>
        //        private static int GetDurationMsec(string input)
        //        {
        //            //Duration: 00:00:10.32, start: 0.021333, bitrate: 543 kb / s
        //            Regex regex = new Regex("Duration:\\s?(\\d{2}):(\\d{2}):(\\d{2}).(\\d{2})\\s?");
        //            Match m = regex.Match(input);
        //            int duration = 0, H = 0, M = 0, S = 0, dotS = 0;
        //            if (m.Success && m.Groups.Count == 5)
        //            {
        //                int.TryParse(m.Groups[1].Value, out H);
        //                int.TryParse(m.Groups[2].Value, out M);
        //                int.TryParse(m.Groups[3].Value, out S);
        //                int.TryParse(m.Groups[4].Value, out dotS);
        //                duration = (H * 3600 + M * 60 + S) * 1000 + dotS * 10;
        //            }
        //            return duration;
        //        }
        //        public static int GetDuration(String mediaFile)
        //        {
        //            if (!File.Exists(mediaFile))
        //                return 0;

        //            String aArguments = string.Format("-i \"{0}\"", mediaFile);
        //            string strOutput = RunProcess(aArguments);
        //            return GetDurationMsec(strOutput);
        //        }
        //        #region 视频
        /// <summary>
        /// 图片转视频
        /// </summary>
        /// <param name="animeInfo"></param>
        /// <param name="fileText"></param>
        /// <param name="fileOut"></param>
        public static void MakeVideoFromTextFile(AnimeInfo animeInfo, string fileText, double dStepProgressBegin = 0, double dStepProgressTotal = 0)
        {
            //ffmpeg -f concat -safe 0 -i inputp.txt -r 30 -y -vcodec libopenh264 outp.mp4
            string arguments = string.Format(@"-f concat -safe 0 -i ""{0}"" -r {1} -y -vcodec libopenh264 -b:v {2}k ""{3}"""
                                        , fileText, animeInfo.IntFPS, animeInfo.QualityV, animeInfo.FileOut);
             RunProcess(arguments);
        }
        //        /// <summary>
        //        /// 视频添加图片水印
        //        /// </summary>
        //        /// <param name="fileSource"></param>
        //        /// <param name="fileMark"></param>
        //        /// <param name="fileOut"></param>
        //        /// <param name="pos"></param>
        //        /// <param name="timeStart"></param>
        //        /// <param name="timeEnd"></param>
        //        public static void MakeWatermark(AnimeInfo animeInfo, string fileSource, string fileMark, string fileOut,Point pos, double timeStart, double timeEnd, double dStepProgressBegin = 0, double dStepProgressTotal = 0)
        //        {
        //            //ffmpeg -i "1.mp4" -vf "movie=p.png[inner]; [in][inner]overlay=10:10:enable='between(t,10,30)' [out]" -y "outp.mp4"
        //            fileMark = fileMark.Replace("\\", "\\\\");
        //            string arguments = string.Format(@"-i ""{0}"" -vf ""movie=\'{1}\'[inner];[in][inner]overlay={2}:{3}:enable='between(t,{4},{5})' [out]"" -y -vcodec libopenh264 -b:v {6}k ""{7}"""
        //                                        , fileSource, fileMark, pos.X,pos.Y, timeStart, timeEnd, animeInfo.QualityV, fileOut);
        //            RunProcess(arguments, animeInfo.BGworker, dStepProgressBegin, dStepProgressTotal);
        //        }
        //        public static void MakeWatermarkStepMouseRoute(AnimeInfo animeInfo, string fileSource, string fileMark, string fileOut,double duration, Rectangle rectS, Point posE, double dStepProgressBegin = 0, double dStepProgressTotal = 0)
        //        {
        //            //ffmpeg -i "test.mp4" -vf "movie=p.png,scale=30x30[inner]; [in][inner]overlay=x='if(gt(t,20),640,1280+(640-1280)*n/(20*29.97))':y='0.5625*x+360-0.5625*640' [out]" -y "outp.mp4
        //            fileMark = fileMark.Replace("\\", "\\\\");

        //            string arguments = "";
        //            if (posE.X - rectS.X != 0)
        //            {
        //                float a = (posE.Y- rectS.Y)*1f / (posE.X - rectS.X);
        //                arguments = string.Format(@"-i ""{0}"" -vf ""movie=\'{1}\',scale={8}x{9}[inner];[in][inner]overlay=x='if(gt(t,{2}),{6},{4}+({6}-{4})*n/{3})':y='{7}*(x-{4})+{5}' [out]"" -y -vcodec libopenh264 -b:v {10}k ""{11}"""
        //                                            , fileSource, fileMark, duration, duration*animeInfo.IntFPS, rectS.X, rectS.Y, posE.X, a, rectS.Width, rectS.Height, animeInfo.QualityV, fileOut);
        //            }
        //            else
        //            {
        //                arguments = string.Format(@"-i ""{0}"" -vf ""movie=\'{1}\',scale={7}x{8}[inner];[in][inner]overlay=x='{4}':y='if(gt(t,{2}),{6},{5}+({6}-{5})*n/{3})' [out]"" -y -vcodec libopenh264 -b:v {9}k ""{10}"""
        //                                            , fileSource, fileMark, duration, duration*animeInfo.IntFPS, rectS.X, rectS.Y,posE.Y, rectS.Width, rectS.Height, animeInfo.QualityV, fileOut);
        //            }
        //            RunProcess(arguments, animeInfo.BGworker, dStepProgressBegin, dStepProgressTotal);
        //        }
        //        /// <summary>
        //        /// 视频添加多个图片水印（鼠标轨迹）
        //        /// </summary>
        //        /// <param name="fileSource"></param>
        //        /// <param name="fileMarks"></param>
        //        /// <param name="fileOut"></param>
        //        /// <param name="overlays"></param>
        //        public static void MakeWatermarkMulti(AnimeInfo animeInfo, string fileSource, string fileMarks, string fileOut, string overlays, double dStepProgressBegin = 0, double dStepProgressTotal = 0)
        //        {
        //            //ffmpeg -i sv10.mp4 -i p.png -i p.png -filter_complex "[1]scale=30x30[t1];[0][t1]overlay=10:10:enable='between(t,0,3)' [tmp];[2]scale=30x30[t2];[tmp][t2] overlay=50:50:enable='between(t,3,6)'" -y -vcodec libopenh264 outc.mp4
        //            string arguments = string.Format(@"-i ""{0}"" {1} -filter_complex ""{2}"" -y -vcodec libopenh264 -b:v {3}k ""{4}"""
        //                                        , fileSource, fileMarks, overlays, animeInfo.QualityV, fileOut);
        //            RunProcess(arguments, animeInfo.BGworker, dStepProgressBegin, dStepProgressTotal);
        //        }
        //        /// <summary>
        //        /// 视频叠加
        //        /// </summary>
        //        /// <param name="fileSource"></param>
        //        /// <param name="fileMark"></param>
        //        /// <param name="fileOut"></param>
        //        /// <param name="pos"></param>
        //        /// <param name="timeStart"></param>
        //        public static void MakeMergeVideo(AnimeInfo animeInfo, string fileSource, string fileMark, string fileOut,Rectangle rect, double timeStart, double dStepProgressBegin = 0, double dStepProgressTotal = 0)
        //        {
        //            //ffmpeg -i test.mp4 -i 1-5.mp4 -filter_complex "[0:v]setpts=PTS-STARTPTS[v0];[1:v]setpts=PTS-STARTPTS+5/TB,scale=200x100[v1];[v0][v1]overlay=100:0:eof_action=pass[out1]" -map [out1] -y -vcodec libopenh264 outp.mp4
        //            string arguments = string.Format(@"-i ""{0}"" -i ""{1}"" -filter_complex ""[0:v]setpts=PTS-STARTPTS[v0];[1:v]setpts=PTS-STARTPTS+{2}/TB,scale={3}x{4}[v1];[v0][v1]overlay={5}:{6}:eof_action=pass[out1]"" -map [out1] -y -vcodec libopenh264 -b:v {7}k ""{8}"""
        //                                        , fileSource, fileMark, timeStart, rect.Width, rect.Height, rect.X, rect.Y, animeInfo.QualityV, fileOut);
        //            RunProcess(arguments, animeInfo.BGworker, dStepProgressBegin, dStepProgressTotal);
        //        }
        //        /// <summary>
        //        /// 视频添加水印图片淡入淡出效果
        //        /// </summary>
        //        /// <param name="fileSource"></param>
        //        /// <param name="fileFade"></param>
        //        /// <param name="fileOut"></param>
        //        /// <param name="pos"></param>
        //        /// <param name="timeStartFadeIn"></param>
        //        /// <param name="timeFadeIn"></param>
        //        /// <param name="timeStartFadeOut"></param>
        //        /// <param name="timeFadeOut"></param>
        //        public static void MakeFade(AnimeInfo animeInfo, string fileSource, string fileFade, string fileOut,Point pos, double timeStartFadeIn, double timeFadeIn, double timeStartFadeOut, double timeFadeOut, double dStepProgressBegin = 0, double dStepProgressTotal = 0)
        //        {
        //            //ffmpeg -i test.mp4 -loop 1 -i f.png -filter_complex "[1]format=yuva420p,fade=in:st=3:d=10:alpha=1,fade=out:st=20:d=5:alpha=1[i]; [0][i]overlay=0:0:shortest=1" -y -vcodec libopenh264 outp.mp4
        //            string arguments = string.Format(@"-i ""{0}"" -loop 1 -i ""{1}"" -filter_complex ""[1]format=yuva420p,fade=in:st={2}:d={3}:alpha=1,fade=out:st={4}:d={5}:alpha=1[v1]; [0][v1]overlay={6}:{7}:shortest=1"" -y -vcodec libopenh264 -b:v {8}k ""{9}"""
        //                                        , fileSource, fileFade, timeStartFadeIn, timeFadeIn, timeStartFadeOut, timeFadeOut,pos.X,pos.Y, animeInfo.QualityV, fileOut);
        //            RunProcess(arguments, animeInfo.BGworker, dStepProgressBegin, dStepProgressTotal);
        //        }
        //        /// <summary>
        //        /// 视频添加放大镜效果
        //        /// </summary>
        //        /// <param name="animeInfo"></param>
        //        /// <param name="fileSource"></param>
        //        /// <param name="fileOut"></param>
        //        /// <param name="widthDrawArea"></param>
        //        /// <param name="rectObjectBefore"></param>
        //        /// <param name="rectObject"></param>
        //        /// <param name="timeStart"></param>
        //        /// <param name="timeDuration"></param>
        //        /// <param name="keep"></param>
        //        public static void MakeZoompan(AnimeInfo animeInfo, string fileSource, string fileOut, int widthDrawArea,Rectangle rectObjectBefore, Rectangle rectObject, double timeStart, double timeDuration,double keep, double dStepProgressBegin = 0, double dStepProgressTotal = 0)
        //        {
        //            //ffmpeg - i SV22.mp4 - vf "zoompan=z='if(between(in,0,115),1440/(1440+(460-1440)*min(in/69,1)),1)':x='172*if(between(in,0,115),min(in/69,1))':y='185*if(between(in,0,115),min(in/69,1))':d=1:fps=23:s=1440x900" - y - vcodec libopenh264 SV23.mp4
        //            string arguments = string.Format(@"-i ""{0}"" -vf ""zoompan=z='if(between(in,{2},{3}),{1}/({5}+({6}-{5})*min((in-{2})/{4},1)),1)':x='{7}+({9}-{7})*if(between(in,{2},{3}),min((in-{2})/{4},1))':y='{8}+({10}-{8})*if(between(in,{2},{3}),min((in-{2})/{4},1))':d=1:fps={11}:s={12}x{13}"" -y -vcodec libopenh264 -b:v 2500k ""{14}"""
        //                                        , fileSource, widthDrawArea,timeStart * animeInfo.IntFPS, (timeStart + timeDuration+keep) * animeInfo.IntFPS, timeDuration * animeInfo.IntFPS, rectObjectBefore.Width, rectObject.Width, rectObjectBefore.X, rectObjectBefore.Y, rectObject.X, rectObject.Y, animeInfo.IntFPS
        //                                        , Convert.ToInt32(animeInfo.SizeBG.Width* animeInfo.ScaleRate), Convert.ToInt32(animeInfo.SizeBG.Height* animeInfo.ScaleRate), fileOut);
        //            RunProcess(arguments, animeInfo.BGworker, dStepProgressBegin, dStepProgressTotal);
        //        }
        //        /// <summary>
        //        /// 视频添加黑边
        //        /// </summary>
        //        /// <param name="fileIn"></param>
        //        /// <param name="fileOut"></param>
        //        /// <param name="scaled"></param>
        //        /// <param name="pad"></param>
        //        public static void ScalePadVideo(AnimeInfo animeInfo, string fileIn, string fileOut,Size scaled,Size pad, double dStepProgressBegin = 0, double dStepProgressTotal = 0)
        //        {
        //            //ffmpeg -i 2.mp4 -vf scale=854:480,pad=854:540:0:30:black -y -vcodec libopenh264 -r 23 -c:a aac -ar 44100 -ab 128k -ac 2 -b:v 2500k 2s.mp4
        //            string arguments="";
        //            if (dStepProgressTotal == 0)
        //                arguments = string.Format(@"-i ""{0}"" -vf scale={1}:{2},pad={3}:{4}:{5}:{6}:black -s {7}x{8} -y -vcodec libopenh264 ""{9}"""
        //                            , fileIn, scaled.Width, scaled.Height, scaled.Width + pad.Width, scaled.Height + pad.Height, pad.Width / 2, pad.Height / 2, animeInfo.SizeVideoContainer.Width, animeInfo.SizeVideoContainer.Height, fileOut);
        //            else
        //                arguments = string.Format(@"-i ""{0}"" -vf scale={1}:{2},pad={3}:{4}:{5}:{6}:black -s {7}x{8} -y -vcodec libopenh264 -r {9} -c:a aac -ar {10} -ab {11}k -ac {12} -b:v {13}k ""{14}"""
        //                            , fileIn, scaled.Width, scaled.Height, scaled.Width + pad.Width, scaled.Height + pad.Height, pad.Width / 2, pad.Height / 2, animeInfo.SizeVideoContainer.Width, animeInfo.SizeVideoContainer.Height, animeInfo.IntFPS, animeInfo.SamplingRate, animeInfo.BitRate, animeInfo.Channel, animeInfo.QualityV,fileOut);
        //            RunProcess(arguments, animeInfo.BGworker, dStepProgressBegin, dStepProgressTotal);
        //        }
        //        #endregion
        //        #region 音频
        //        ///// <summary>
        //        ///// 混合音频
        //        ///// </summary>
        //        ///// <param name="animeInfo"></param>
        //        ///// <param name="fileIn1"></param>
        //        ///// <param name="delay1">fileIn1的延时输入毫秒值</param>
        //        ///// <param name="fileIn2"></param>
        //        ///// <param name="delay2">fileIn2的延时输入毫秒值</param>
        //        ///// <param name="audioFileOut"></param>
        //        //public static void MixAudio(AnimeInfo animeInfo, string fileIn1, long delay1, string fileIn2, long delay2, string audioFileOut)
        //        //{
        //        //    string arguments = "";
        //        //    if (fileIn2!="")
        //        //    {
        //        //        #region 长媒体置于短媒体之前，临时解决FFmpeg bug（duration=longest无效）
        //        //        long duration1 = CalMediaDuration(fileIn1)+ delay1;
        //        //        long duration2 = CalMediaDuration(fileIn2)+ delay2;
        //        //        if (duration1 < duration2)
        //        //        {
        //        //            string t = fileIn1;
        //        //            fileIn1 = fileIn2;
        //        //            fileIn2 = t;

        //        //            long d = delay1;
        //        //            delay1 = delay2;
        //        //            delay2 = d;
        //        //        }
        //        //        #endregion
        //        //        //ffmpeg -i 1.mp4 -i test.mp4 -filter_complex "[0:a]adelay=50000|50000[a0];[1:a]adelay=10000|10000[a1];[a0][a1]amix=inputs=2:duration=first:dropout_transition=0,dynaudnorm[out]" -map "[out]" -ac 1 -ab 96k -ar 11025 -c:a aac -y outa.aac
        //        //        string strDelay1 = "",strDelay2 = "", strRedirect1 = @"[0:a]", strRedirect2 = @"[1:a]";
        //        //        if (delay1 > 0)
        //        //        {
        //        //            strRedirect1 = @"[a0]";
        //        //            strDelay1 = string.Format(@"[0:a]adelay={0}|{0}{1};", delay1, strRedirect1);
        //        //        }
        //        //        if (delay2>0)
        //        //        {
        //        //            strRedirect2 = @"[a1]";
        //        //            strDelay2 = string.Format(@"[1:a]adelay={0}|{0}{1};", delay2, strRedirect2);
        //        //        }

        //        //        arguments = string.Format(@"-i ""{0}"" -i ""{1}"" -filter_complex ""{2}{3}{4}{5}amix=inputs=2:duration=first:dropout_transition=0,dynaudnorm[out]"" -map ""[out]"" -ac {6} -ab {7}k -ar {8} -c:a aac -y ""{9}"""
        //        //                                    , fileIn1, fileIn2, strDelay1, strDelay2, strRedirect1, strRedirect2, animeInfo.Channel,animeInfo.BitRate, animeInfo.SamplingRate,audioFileOut);
        //        //    }
        //        //    else
        //        //    {
        //        //        //ffmpeg -i 1.mp4 -filter_complex "[0:a]adelay=50000|50000[out]" -map "[out]" -ac 2 -ab 96k -ar 11025 -c:a aac -y outa.aac
        //        //        string strDelay = "";
        //        //        if (delay1>0)
        //        //            strDelay= string.Format(@"-filter_complex ""[0:a]adelay ={0}|{0}[out]"" -map ""[out]""",delay1);

        //        //        arguments = string.Format(@"-i ""{0}"" {1} -ac {2} -ab {3}k -ar {4} -c:a aac -y ""{5}"""
        //        //                                    , fileIn1, strDelay, animeInfo.Channel,animeInfo.BitRate, animeInfo.SamplingRate,audioFileOut);
        //        //    }
        //        //    CommonFunc.RunProcess(fileFfmpeg, arguments);
        //        //}
        //        /// <summary>
        //        /// 制作空音频
        //        /// </summary>
        //        /// <param name="animeInfo"></param>
        //        /// <param name="audioFileOut"></param>
        //        /// <param name="duration"></param>
        //        public static void MakeAudioFromNull(AnimeInfo animeInfo, float duration, string audioFileOut)
        //        {
        //            //ffmpeg -f lavfi -i aevalsrc=0 -t 1 -ar 44100 -ab 128k -ac 2 -y 0.aac
        //            string arguments = string.Format(@"-f lavfi -i anullsrc=r={1} -t {0} -c:a aac -ab {2}k -ac {3} -q:a {4} -y ""{5}"""
        //                                        , duration, animeInfo.SamplingRate, animeInfo.BitRate, animeInfo.Channel, animeInfo.QualityA, audioFileOut);
        //            //string arguments = string.Format(@"-f lavfi -i aevalsrc=0 -t {0} -ar {1} -ab {2}k -ac {3} -q:a {4} -y ""{5}"""
        //            //                            , duration, animeInfo.SamplingRate, animeInfo.BitRate, animeInfo.Channel, animeInfo.QualityA, audioFileOut);
        //            RunProcess(arguments, animeInfo.BGworker);
        //        }
        //        /// <summary>
        //        /// 视频转音频
        //        /// </summary>
        //        /// <param name="animeInfo"></param>
        //        /// <param name="audioFileOut"></param>
        //        /// <param name="videoName"></param>
        //        //public static void MakeAudioFromVideo(AnimeInfo animeInfo, string videoName, string audioFileOut, float duration=-1)
        //        //{
        //        //    //ffmpeg -i 1.mp4 - vn - ar 44100 - ab 128k - ac 2 out.ts
        //        //    string arguments = string.Format(@"-i ""{0}"" -vn -c:a aac -ar {1} -ab {2}k -ac {3} -y ""{4}"""
        //        //                                , videoName, animeInfo.SamplingRate, animeInfo.BitRate, animeInfo.Channel, audioFileOut);
        //        //    string output = RunProcess(arguments, animeInfo.BGworker);

        //        //    if (output.Contains("Output file #0 does not contain any stream"))
        //        //    {
        //        //        if (duration < 2)
        //        //            duration = 2;
        //        //        MakeAudioFromNull(animeInfo, duration, audioFileOut);
        //        //    }
        //        //}
        //        public static void MakeAudioFromVideo(AnimeInfo animeInfo, string videoName, string audioFileOut)
        //        {
        //            //ffmpeg -i 1.mp4 - vn - ar 44100 - ab 128k - ac 2 out.ts
        //            string arguments = string.Format(@"-i ""{0}"" -vn -c:a aac -ar {1} -ab {2}k -ac {3} -y ""{4}"""
        //                                        , videoName, animeInfo.SamplingRate, animeInfo.BitRate, animeInfo.Channel, audioFileOut);
        //            RunProcess(arguments, animeInfo.BGworker);
        //        }
        //        /// <summary>
        //        /// 连接音频
        //        /// </summary>
        //        /// <param name="audioFileOut"></param>
        //        /// <param name="audioFile"></param>
        //        /// <param name="audioFile2"></param>
        //        public static void ConcatAudio(AnimeInfo animeInfo, string audioFile, string audioFile2, string audioFileOut)
        //        {
        //            //ffmpeg -i "concat:0.ts|1.ts" -c copy out.ts
        //            string arguments = string.Format(@"-i ""concat:{0}|{1}"" -c copy -y ""{2}""", audioFile, audioFile2, audioFileOut);
        //            RunProcess(arguments, animeInfo.BGworker);
        //        }
        //        /// <summary>
        //        /// 合并音频
        //        /// </summary>
        //        /// <param name="audioFileOut"></param>
        //        /// <param name="audioFile"></param>
        //        /// <param name="audioFile2"></param>
        //        public static void MergeAudio(AnimeInfo animeInfo, string audioFile, string audioFile2, string audioFileOut)
        //        {
        //            //ffmpeg -i 1.ts  -i 2.ts -filter_complex amix=inputs=2:duration=longest:dropout_transition=1 out.ts
        //            //#region 长媒体置于短媒体之前，临时解决FFmpeg bug（duration=longest无效）
        //            //long duration = CalMediaDuration(audioFile);
        //            //long duration2 = CalMediaDuration(audioFile2);
        //            //if (duration < duration2)
        //            //{
        //            //    string t = audioFile;
        //            //    audioFile = audioFile2;
        //            //    audioFile2 = t;
        //            //}
        //            //#endregion

        //            string arguments = string.Format(@"-i ""{0}"" -i ""{1}"" -filter_complex ""amix=inputs=2:duration=longest:dropout_transition=0,volume=10dB"" -y ""{2}"""
        //            //string arguments = string.Format(@"-i ""{0}"" -i ""{1}"" -filter_complex ""amix=inputs=2:duration=longest:dropout_transition=0,dynaudnorm,volume=5dB"" -y ""{2}"""
        //            //string arguments = string.Format(@"-i ""{0}"" -i ""{1}"" -filter_complex ""amix=inputs=2:duration=first:dropout_transition=0,dynaudnorm"" -y ""{2}"""
        //                                        , audioFile, audioFile2, audioFileOut);
        //            RunProcess(arguments, animeInfo.BGworker);
        //        }
        //        #endregion
        //        #region 合并连接
        //        /// <summary>
        //        /// 合并视频和音频
        //        /// </summary>
        //        /// <param name="videoFile"></param>
        //        /// <param name="audioFile"></param>
        //        /// <param name="audioFileOut"></param>
        //        public static void MergeVideoAudio(AnimeInfo animeInfo, string videoFile, string audioFile, string mediaFileOut)
        //        {
        //            //ffmpeg -i B.mp4 -i 1.aac -c:v copy -c:a aac -strict experimental  outav.mp4
        //            //string arguments = string.Format(@"-i ""{0}"" -i ""{1}"" -c:v copy -c:a aac -strict experimental -y ""{2}""", videoFile, audioFile, mediaFileOut);
        //            //string arguments = string.Format(@"-i ""{0}"" -i ""{1}"" -filter_complex ""[1:0]apad"" -shortest -vcodec libopenh264 -c:a aac -y -b:v {2}k ""{3}""", videoFile, audioFile, animeInfo.QualityV, mediaFileOut);
        //            string arguments = string.Format(@"-i ""{0}"" -i ""{1}"" -shortest -vcodec libopenh264 -c:a aac -y -b:v {2}k ""{3}""", videoFile, audioFile, animeInfo.QualityV, mediaFileOut);
        //            RunProcess(arguments, animeInfo.BGworker);
        //        }
        //        /// <summary>
        //        /// 多媒体连接
        //        /// </summary>
        //        /// <param name="fileText"></param>
        //        /// <param name="fileOut"></param>
        //        public static void ConcatMedia(AnimeInfo animeInfo, string fileText, string fileOut)
        //        {
        //            //ffmpeg -f concat -safe 0 -i avIn.txt -codec copy -y outp.mp4
        //            string arguments = string.Format(@"-f concat -safe 0 -i ""{0}"" -codec copy -y ""{1}""", fileText, fileOut);
        //            RunProcess(arguments, animeInfo.BGworker);
        //        }
        //        #endregion
        //        public static void WavConvertToMp3(string input,
        //                                           string output,
        //                                           string bitrate = null,
        //                                           string Samplingfrequency = null,
        //                                           string channel = null)
        //        {
        //            try
        //            {
        //                string arguments = "";
        //                if (bitrate == null && Samplingfrequency == null && channel == null)
        //                {
        //                    arguments = string.Format(@" -i ""{0}"" -qscale 0 -y -f mp3 ""{1}"""
        //                                                , input, output);
        //                }
        //                else
        //                {
        //                    arguments = string.Format(@" -i ""{0}"" -ab {1}k -ar {2} -ac {3} -qscale 50 -y -f mp3 ""{4}"""
        //                                                , input, bitrate, Samplingfrequency, channel, output);
        //                }
        //                string strOutPut = RunProcess(arguments);
        //            }
        //            catch (Exception ex)
        //            {
        //            }

        //        }
        //        private static void getStrOutput(string strArguments)
        //        {
        //            Process p = new Process();
        //            //p.StartInfo.FileName = @"D:\bin\ffmpeg.exe";
        //            p.StartInfo.FileName = ApplicationInfo.strDojoExecutablePath + @"\ffmpeg.exe";
        //            p.StartInfo.CreateNoWindow = true;
        //            p.StartInfo.Arguments = strArguments;//执行参数
        //            p.StartInfo.UseShellExecute = false;// Set UseShellExecute to false for redirection.
        //            p.StartInfo.RedirectStandardInput = true;
        //            p.StartInfo.RedirectStandardOutput = true;
        //            p.StartInfo.RedirectStandardError = true;
        //            using (p)
        //            {
        //                try
        //                {
        //                    p.Start();
        //                    strOutput = "";
        //                    strOutput = p.StandardError.ReadToEnd();
        //                }
        //                catch (Exception ex)
        //                {
        //                    strOutput = ex.Message;
        //                    p.Close();
        //                    p.Dispose();
        //                }
        //            }
        //        }
        //        public static int GetAudioTime(string path)
        //        {
        //            int audioTime = 0;
        //            string strArguments = string.Format(@" -i ""{0}"" 2>&1 | find ""Duration""", path);
        //            getStrOutput(strArguments);
        //            audioTime = GetDurationMsec(strOutput);
        //            //if (strOutput.Contains("Duration:"))
        //            //{
        //            //    Int32 indexOfDuration = strOutput.IndexOf("Duration");
        //            //    Int32 indexOfBegin = strOutput.IndexOf(":", indexOfDuration);
        //            //    Int32 indexOfEnd = strOutput.IndexOf(",", indexOfBegin);
        //            //    var duration = strOutput.Substring(indexOfBegin + 1, indexOfEnd - indexOfBegin - 1);

        //            //}
        //            return audioTime;
        //        }
    }
}
