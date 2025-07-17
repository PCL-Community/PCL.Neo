using PCL.Neo.Core.Models.Skin.Extensions;
using SkiaSharp;

namespace PCL.Neo.Core.Models.Skin;

public class SkinHelper
{
    public static SKBitmap Capture(SKBitmap skin, int scale = 8) {
        const int CANVAS_SIZE = 72;
        const int HEAD_BLOCK = 8;
        const int HEAD_SRC_X = 8;
        const int HEAD_SRC_Y = 8;
        const int HAT_SRC_X = 40;
        const int HAT_SRC_Y = 8;

        var canvas = new SKBitmap(CANVAS_SIZE, CANVAS_SIZE);
        skin.CopyBlock(canvas, HEAD_SRC_X, HEAD_SRC_Y, HEAD_BLOCK,
            HEAD_BLOCK, 4, 4, HEAD_BLOCK, HEAD_BLOCK);

        skin.CopyBlockBlend(canvas, HAT_SRC_X, HAT_SRC_Y, HEAD_BLOCK,
            HEAD_BLOCK, 0, 0, HEAD_BLOCK + 1, HEAD_BLOCK + 1);

        return canvas.ResizeNearest(scale);
    }
}
