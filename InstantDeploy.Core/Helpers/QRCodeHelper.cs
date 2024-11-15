namespace InstantDeploy.Helpers;

/// <summary> 二维码帮助类 </summary>
public class QRCodeHelper
{
    /// <summary> 工作目录 </summary>
    private static readonly string BaseDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "QRCodeModels");

    private static readonly string _detector_prototxt_path = Path.Combine(BaseDirectory, "detect.prototxt");
    private static readonly string _detector_caffe_model_path = Path.Combine(BaseDirectory, "detect.caffemodel");
    private static readonly string _prototxt_path = Path.Combine(BaseDirectory, "sr.prototxt");
    private static readonly string _caffe_model_path = Path.Combine(BaseDirectory, "sr.caffemodel");

    static QRCodeHelper()
    {
        // opencv wechat_qrcode
        // 项目开源地址：https://github.com/opencv/opencv_contrib/tree/master/modules/wechat_qrcode
        // 模型下载地址：https://github.com/WeChatCV/opencv_3rdparty

        BaseDirectory.TryCreateDirectory();

        // 嵌入资源保存在本地
        Properties.Resources.detect_prototxt.TrySaveFile(_detector_prototxt_path);
        Properties.Resources.detect_caffemodel.TrySaveFile(_detector_caffe_model_path);
        Properties.Resources.sr_prototxt.TrySaveFile(_prototxt_path);
        Properties.Resources.sr_caffemodel.TrySaveFile(_caffe_model_path);
    }

    /// <summary> 解析二维码内容 </summary>
    /// <param name="memoryStream"> 二维码 MemoryStream </param>
    /// <returns> string[] </returns>
    public static string[] ParsingQRCodeContent(MemoryStream memoryStream)
    {
        using OpenCvSharp.WeChatQRCode weChatQRCode = OpenCvSharp.WeChatQRCode.Create(_detector_prototxt_path
            , _detector_caffe_model_path
            , _prototxt_path
            , _caffe_model_path);

        using Mat mat = Cv2.ImDecode(memoryStream.ToArray(), ImreadModes.Color);
        weChatQRCode.DetectAndDecode(mat, out var rects, out var contents);
        return contents;
    }

    /// <summary> 解析二维码内容 </summary>
    /// <param name="qrCodeFile"> 二维码文件路径 </param>
    /// <returns> string[] </returns>
    public static string[] ParsingQRCodeContent(string qrCodeFile)
    {
        using OpenCvSharp.WeChatQRCode weChatQRCode = OpenCvSharp.WeChatQRCode.Create(_detector_prototxt_path
            , _detector_caffe_model_path
            , _prototxt_path
            , _caffe_model_path);

        using Mat mat = Cv2.ImRead(qrCodeFile, ImreadModes.Color);
        weChatQRCode.DetectAndDecode(mat, out var rects, out var contents);
        return contents;
    }

    /// <summary> 生成二维码 </summary>
    /// <param name="filePath"> 二维码保存路径 </param>
    /// <param name="content"> 二维码内容 </param>
    /// <param name="logoPath"> 二维码 icon </param>
    /// <exception cref="FileNotFoundException"> </exception>
    public static void GenerateQRCode(ref string filePath, string content, string logoPath = "")
    {
        using var stream = GenerateQRCode(content, logoPath);
        stream.TrySaveFile(filePath);
    }

    /// <summary> 生成二维码 </summary>
    /// <param name="content"> </param>
    /// <param name="logoPath"> </param>
    /// <returns> </returns>
    public static Stream? GenerateQRCode(string content, string logoPath = "")
    {
        //【Gdip 问题解决】此开关仅在 .NET 6 中可用，在 .NET 7 中已删除：https://learn.microsoft.com/zh-cn/dotnet/core/compatibility/core-libraries/6.0/system-drawing-common-windows-only
        // 采用：SkiaSharp

        if (!string.IsNullOrWhiteSpace(logoPath) && !File.Exists(logoPath))
            throw new FileNotFoundException("File does not exist.");

        using QRCodeGenerator qrGenerator = new QRCodeGenerator();
        using QRCodeData qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q, true);
        using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
        {
            byte[] qrCodeBytes = qrCode.GetGraphic(pixelsPerModule: 20
                , darkColor: System.Drawing.Color.Black
                , lightColor: System.Drawing.Color.White);

            if (!string.IsNullOrWhiteSpace(logoPath) && File.Exists(logoPath))
            {
                using (SKBitmap qrBitmap = SKBitmap.Decode(qrCodeBytes))
                using (SKBitmap iconBitmap = SKBitmap.Decode(logoPath))
                {
                    // 图标大小为二维码的1/5
                    int iconSize = qrBitmap.Width / 5;

                    // 创建一个空白画布用于合并二维码和图标
                    using (SKBitmap combinedBitmap = new SKBitmap(qrBitmap.Width, qrBitmap.Height))
                    {
                        using (SKCanvas canvas = new SKCanvas(combinedBitmap))
                        {
                            // 在画布上绘制二维码
                            canvas.DrawBitmap(qrBitmap, new SKPoint(0, 0));

                            // 计算图标的位置
                            float iconX = (qrBitmap.Width - iconSize) / 2;
                            float iconY = (qrBitmap.Height - iconSize) / 2;

                            // 在画布上绘制图标
                            SKRect iconRect = new SKRect(iconX, iconY, iconX + iconSize, iconY + iconSize);
                            canvas.DrawBitmap(iconBitmap, iconRect);

                            // 裁剪图像去除白色边框
                            int size = 50;
                            SKRectI cropRect = new SKRectI(size, size, combinedBitmap.Width - size, combinedBitmap.Height - size);
                            // 调整裁剪矩形的大小以去除边框
                            using (SKBitmap croppedBitmap = new SKBitmap(cropRect.Width, cropRect.Height))
                            using (SKCanvas skcanvas = new SKCanvas(croppedBitmap))
                            {
                                skcanvas.DrawBitmap(combinedBitmap, cropRect, new SKRect(0, 0, cropRect.Width, cropRect.Height));

                                // 保存最终的带图标的二维码
                                using (SKImage image = SKImage.FromBitmap(croppedBitmap))
                                using (SKData data = image.Encode(SKEncodedImageFormat.Png, 100))
                                {
                                    Stream stream = new MemoryStream();
                                    data.SaveTo(stream);
                                    return stream;
                                }
                            }
                        }
                    }
                }
            }
        }

        return null;
    }
}