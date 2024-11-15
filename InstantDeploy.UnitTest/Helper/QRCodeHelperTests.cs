namespace InstantDeploy.UnitTest.Helper;

[TestSubject(typeof(QRCodeHelper))]
public class QRCodeHelperTests
{
    private static string BaseDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");

    [Fact(DisplayName = "FilePath - 二维码识别")]
    public void ParsingQRCodeContent_FilePath()
    {
        string qrCode = Path.Combine(BaseDirectory, "qr_code.jpg");

        // Arrange
        // Act
        var contents = QRCodeHelper.ParsingQRCodeContent(qrCode);
        // Assert
        Assert.Equal(contents, new string[] { "016066232848" });
    }

    [Fact(DisplayName = "Stream - 二维码识别")]
    public void ParsingQRCodeContent_Stream()
    {
        string qrCode = Path.Combine(BaseDirectory, "qr_code.jpg");
        // Arrange
        // Act
        var contents = QRCodeHelper.ParsingQRCodeContent(qrCode.ToMemoryStream());
        // Assert
        Assert.Equal(contents, new string[] { "016066232848" });
    }

    [Fact(DisplayName = "File - 二维码生成")]
    public void GenerateQRCode()
    {
        string content = "qr code test";
        string output_qr_code = Path.Combine(BaseDirectory, "output_qr_code.jpg");
        string logo_file_path = Path.Combine(BaseDirectory, "logo.png");
        QRCodeHelper.GenerateQRCode(ref output_qr_code, content, logo_file_path);
        Assert.True(File.Exists(output_qr_code));
    }
}