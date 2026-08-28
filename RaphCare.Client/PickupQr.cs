using QRCoder;

namespace RaphCare.Client;

/// <summary>Builds a pickup-code QR as a PNG data URI for admin print slips and the patient app.</summary>
public static class PickupQr
{
    public static byte[] ToPng(string pickupCode, int pixelsPerModule = 8)
    {
        var code = (pickupCode ?? string.Empty).Trim().ToUpperInvariant();
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(code, QRCodeGenerator.ECCLevel.M);
        return new PngByteQRCode(data).GetGraphic(pixelsPerModule);
    }

    public static string ToPngDataUri(string pickupCode) =>
        "data:image/png;base64," + Convert.ToBase64String(ToPng(pickupCode));
}
