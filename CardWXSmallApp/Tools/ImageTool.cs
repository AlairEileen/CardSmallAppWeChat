using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CardWXSmallApp.Tools
{
    public class ImageTool
    {
        private static readonly double sizePerPx = 0.18;
        /// <summary>
        /// 对图片进行压缩优化（限制宽高），始终保持原宽高比
        /// </summary>
        /// <param name="destPath">目标保存路径</param>
        /// <param name="srcPath">源文件路径</param>
        /// <param name="max_Width">压缩后的图片宽度不大于这值，如果为0，表示不限制宽度</param>
        /// <param name="max_Height">压缩后的图片高度不大于这值，如果为0，表示不限制高度</param>
        /// <param name="quality">1~100整数,无效值则取默认值95</param>
        /// <param name="mimeType">如 image/jpeg</param>
        public bool GetCompressImage(string srcPath, string destPath, int maxWidth, int maxHeight, int quality, out string error, string mimeType = "image/jpeg")
        {
            bool retVal = false;
            error = string.Empty;
            //宽高不能小于0
            if (maxWidth < 0 || maxHeight < 0)
            {
                error = "目标宽高不能小于0";
                return retVal;
            }
            Image srcImage = null;
            Image destImage = null;
            Graphics graphics = null;
            try
            {
                //获取源图像
                srcImage = Image.FromStream(System.IO.File.OpenRead(srcPath), false);
                FileInfo fileInfo = new FileInfo(srcPath);
                //目标宽度
                var destWidth = srcImage.Width;
                //目标高度
                var destHeight = srcImage.Height;
                //如果输入的最大宽为0，则不限制宽度
                //如果不为0，且原图宽度大于该值，则附值为最大宽度
                if (maxWidth != 0 && destWidth > maxWidth)
                {
                    destWidth = maxWidth;
                }
                //如果输入的最大宽为0，则不限制宽度
                //如果不为0，且原图高度大于该值，则附值为最大高度
                if (maxHeight != 0 && destHeight > maxHeight)
                {
                    destHeight = maxHeight;
                }
                float srcD = (float)srcImage.Height / srcImage.Width;
                float destD = (float)destHeight / destWidth;
                //目的高宽比 大于 原高宽比 即目的高偏大,因此按照原比例计算目的高度  
                if (destD > srcD)
                {
                    destHeight = Convert.ToInt32(destWidth * srcD);
                }
                else if (destD < srcD) //目的高宽比 小于 原高宽比 即目的宽偏大,因此按照原比例计算目的宽度  
                {
                    destWidth = Convert.ToInt32(destHeight / srcD);
                }
                //如果维持原宽高，则判断是否需要优化
                if (destWidth == srcImage.Width && destHeight == srcImage.Height && fileInfo.Length < destWidth * destHeight * sizePerPx)
                {
                    error = "图片不需要压缩优化";
                    return retVal;
                }
                //定义画布
                destImage = new Bitmap(destWidth, destHeight);
                //获取高清Graphics
                graphics = GetGraphics(destImage);
                //将源图像画到画布上，注意最后一个参数GraphicsUnit.Pixel
                graphics.DrawImage(srcImage, new Rectangle(0, 0, destWidth, destHeight), new Rectangle(0, 0, srcImage.Width, srcImage.Height), GraphicsUnit.Pixel);
                //如果是覆盖则先释放源资源
                if (destPath == srcPath)
                {
                    srcImage.Dispose();
                }
                //保存到文件，同时进一步控制质量
                SaveImage2File(destPath, destImage, quality, mimeType);
                retVal = true;

            }
            catch (Exception ex)
            {
                error = ex.Message;
            }
            finally
            {
                if (srcImage != null)
                    srcImage.Dispose();
                if (destImage != null)
                    destImage.Dispose();
                if (graphics != null)
                    graphics.Dispose();
            }
            return retVal;
        }

        /// <summary>
        /// 获取高清的Graphics
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        private Graphics GetGraphics(Image img)
        {
            var g = Graphics.FromImage(img);
            //设置质量
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;
            //InterpolationMode不能使用High或者HighQualityBicubic,如果是灰色或者部分浅色的图像是会在边缘处出一白色透明的线
            //用HighQualityBilinear却会使图片比其他两种模式模糊（需要肉眼仔细对比才可以看出）
            g.InterpolationMode = InterpolationMode.Default;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            return g;
        }
        /// <summary>
        /// 将Image实例保存到文件,注意此方法不执行 img.Dispose()
        /// 图片保存时本可以直接使用destImage.Save(path, ImageFormat.Jpeg)，但是这种方法无法进行进一步控制图片质量
        /// </summary>
        /// <param name="path"></param>
        /// <param name="img"></param>
        /// <param name="quality">1~100整数,无效值，则取默认值95</param>
        /// <param name="mimeType"></param>
        private void SaveImage2File(string path, Image destImage, int quality, string mimeType = "image/jpeg")
        {
            if (quality <= 0 || quality > 100) quality = 95;
            //创建保存的文件夹
            FileInfo fileInfo = new FileInfo(path);
            if (!Directory.Exists(fileInfo.DirectoryName))
            {
                Directory.CreateDirectory(fileInfo.DirectoryName);
            }
            //设置保存参数，保存参数里进一步控制质量
            EncoderParameters encoderParams = new EncoderParameters();
            long[] qua = new long[] { quality };
            EncoderParameter encoderParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
            encoderParams.Param[0] = encoderParam;
            //获取指定mimeType的mimeType的ImageCodecInfo
            var codecInfo = ImageCodecInfo.GetImageEncoders().FirstOrDefault(ici => ici.MimeType == mimeType);
            destImage.Save(path, codecInfo, encoderParams);
        }

    }
}
