using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

if (args.Length != 1) throw new ArgumentException("Usage: IconBuilder <output.ico>");
string output = Path.GetFullPath(args[0]);
Directory.CreateDirectory(Path.GetDirectoryName(output)!);

int[] sizes = [16, 20, 24, 32, 40, 48, 64, 128, 256];
List<byte[]> frames = sizes.Select(RenderFrame).ToList();
using FileStream stream = File.Create(output);
using BinaryWriter writer = new(stream);
writer.Write((ushort)0);
writer.Write((ushort)1);
writer.Write((ushort)sizes.Length);
int offset = 6 + sizes.Length * 16;
for (int index = 0; index < sizes.Length; index++)
{
    int size = sizes[index];
    writer.Write((byte)(size == 256 ? 0 : size));
    writer.Write((byte)(size == 256 ? 0 : size));
    writer.Write((byte)0);
    writer.Write((byte)0);
    writer.Write((ushort)1);
    writer.Write((ushort)32);
    writer.Write(frames[index].Length);
    writer.Write(offset);
    offset += frames[index].Length;
}
foreach (byte[] frame in frames) writer.Write(frame);

static byte[] RenderFrame(int size)
{
    using Bitmap bitmap = new(size, size, PixelFormat.Format32bppArgb);
    using Graphics graphics = Graphics.FromImage(bitmap);
    graphics.SmoothingMode = SmoothingMode.AntiAlias;
    graphics.Clear(Color.Transparent);
    float inset = Math.Max(1, size * 0.04f);
    RectangleF background = new(inset, inset, size - inset * 2, size - inset * 2);
    using (GraphicsPath path = Rounded(background, size * 0.22f))
    using (SolidBrush brush = new(Color.FromArgb(243, 246, 251))) graphics.FillPath(brush, path);

    float tile = size * 0.28f;
    float gap = size * 0.085f;
    float start = (size - tile * 2 - gap) / 2;
    Color[] colors =
    [
        Color.FromArgb(15, 108, 189),
        Color.FromArgb(43, 136, 216),
        Color.FromArgb(43, 136, 216),
        Color.FromArgb(96, 165, 232)
    ];
    for (int row = 0; row < 2; row++)
    for (int column = 0; column < 2; column++)
    {
        RectangleF tileRect = new(start + column * (tile + gap), start + row * (tile + gap), tile, tile);
        using GraphicsPath path = Rounded(tileRect, size * 0.07f);
        using SolidBrush brush = new(colors[row * 2 + column]);
        graphics.FillPath(brush, path);
    }
    using MemoryStream png = new();
    bitmap.Save(png, ImageFormat.Png);
    return png.ToArray();
}

static GraphicsPath Rounded(RectangleF rectangle, float radius)
{
    float diameter = radius * 2;
    GraphicsPath path = new();
    path.AddArc(rectangle.Left, rectangle.Top, diameter, diameter, 180, 90);
    path.AddArc(rectangle.Right - diameter, rectangle.Top, diameter, diameter, 270, 90);
    path.AddArc(rectangle.Right - diameter, rectangle.Bottom - diameter, diameter, diameter, 0, 90);
    path.AddArc(rectangle.Left, rectangle.Bottom - diameter, diameter, diameter, 90, 90);
    path.CloseFigure();
    return path;
}
