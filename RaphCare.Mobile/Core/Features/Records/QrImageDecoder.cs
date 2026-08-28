using SkiaSharp;
using ZXing;
using ZXing.Common;

namespace RaphCare.Mobile.Core.Features.Records;

internal static class QrImageDecoder
{
    public static string? Decode(Stream stream)
    {
        using var bitmap = SKBitmap.Decode(stream);
        if (bitmap is null || bitmap.Width <= 0 || bitmap.Height <= 0)
            return null;

        var pixels = bitmap.Bytes;
        if (pixels is null || pixels.Length == 0)
            return null;

        var source = new RGBLuminanceSource(
            pixels,
            bitmap.Width,
            bitmap.Height,
            RGBLuminanceSource.BitmapFormat.BGRA32);
        var reader = new BarcodeReaderGeneric
        {
            AutoRotate = true,
            Options = new DecodingOptions
            {
                PossibleFormats = [BarcodeFormat.QR_CODE],
                TryHarder = true
            }
        };
        return reader.Decode(source)?.Text;
    }
}
