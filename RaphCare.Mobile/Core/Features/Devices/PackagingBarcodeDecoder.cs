using SkiaSharp;
using ZXing;
using ZXing.Common;

namespace RaphCare.Mobile.Core.Features.Devices;

/// <summary>Reads packaging barcodes and QR codes from a photo for watch claim.</summary>
internal static class PackagingBarcodeDecoder
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
                PossibleFormats =
                [
                    BarcodeFormat.QR_CODE,
                    BarcodeFormat.CODE_128,
                    BarcodeFormat.CODE_39,
                    BarcodeFormat.CODE_93,
                    BarcodeFormat.EAN_13,
                    BarcodeFormat.EAN_8,
                    BarcodeFormat.UPC_A,
                    BarcodeFormat.UPC_E,
                    BarcodeFormat.ITF,
                    BarcodeFormat.DATA_MATRIX,
                ],
                TryHarder = true
            }
        };
        return reader.Decode(source)?.Text;
    }
}
