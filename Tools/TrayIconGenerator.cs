using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;

namespace PinyinSwitcher.Tools
{
    internal static class TrayIconGenerator
    {
        private static readonly int[] IconSizes = { 16, 20, 24, 32, 48, 64, 256 };
        private static readonly string[] FontNames = { "Microsoft YaHei UI", "Microsoft YaHei", "Segoe UI", "Arial" };

        public static void GenerateIcons()
        {
            DirectoryInfo projectDirectory = FindProjectDirectory();
            string resourcesDirectory = Path.Combine(projectDirectory.FullName, "Resources");
            Directory.CreateDirectory(resourcesDirectory);

            GenerateIcon("全", Path.Combine(resourcesDirectory, "full.ico"));
            GenerateIcon("双", Path.Combine(resourcesDirectory, "double.ico"));
        }

        private static void GenerateIcon(string text, string path)
        {
            byte[][] images = new byte[IconSizes.Length][];
            for (int index = 0; index < IconSizes.Length; index++)
            {
                images[index] = RenderPng(text, IconSizes[index]);
            }

            using (BinaryWriter writer = new BinaryWriter(File.Create(path)))
            {
                writer.Write((ushort)0);
                writer.Write((ushort)1);
                writer.Write((ushort)IconSizes.Length);

                int imageOffset = 6 + (16 * IconSizes.Length);
                for (int index = 0; index < IconSizes.Length; index++)
                {
                    int size = IconSizes[index];
                    writer.Write((byte)(size == 256 ? 0 : size));
                    writer.Write((byte)(size == 256 ? 0 : size));
                    writer.Write((byte)0);
                    writer.Write((byte)0);
                    writer.Write((ushort)1);
                    writer.Write((ushort)32);
                    writer.Write((uint)images[index].Length);
                    writer.Write((uint)imageOffset);
                    imageOffset += images[index].Length;
                }

                for (int index = 0; index < images.Length; index++)
                {
                    writer.Write(images[index]);
                }
            }

            ValidateIcon(path);
        }

        private static byte[] RenderPng(string text, int size)
        {
            using (Bitmap bitmap = new Bitmap(size, size, PixelFormat.Format32bppArgb))
            using (Graphics graphics = Graphics.FromImage(bitmap))
            using (FontFamily fontFamily = FindFontFamily())
            using (StringFormat format = (StringFormat)StringFormat.GenericTypographic.Clone())
            using (SolidBrush brush = new SolidBrush(Color.White))
            {
                graphics.Clear(Color.Transparent);
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                format.FormatFlags = StringFormatFlags.NoClip | StringFormatFlags.NoWrap;

                using (GraphicsPath path = new GraphicsPath())
                {
                    path.AddString(text, fontFamily, (int)FontStyle.Regular, 100f, PointF.Empty, format);
                    RectangleF bounds = path.GetBounds();
                    float padding = Math.Max(0.5f, size * 0.02f);
                    float scale = Math.Min(
                        (size - (padding * 2f)) / bounds.Width,
                        (size - (padding * 2f)) / bounds.Height);
                    float x = (size - (bounds.Width * scale)) / 2f - (bounds.X * scale);
                    float y = (size - (bounds.Height * scale)) / 2f - (bounds.Y * scale);
                    using (Matrix transform = new Matrix(scale, 0f, 0f, scale, x, y))
                    {
                        path.Transform(transform);
                    }

                    graphics.FillPath(brush, path);
                }

                using (MemoryStream stream = new MemoryStream())
                {
                    bitmap.Save(stream, ImageFormat.Png);
                    return stream.ToArray();
                }
            }
        }

        private static FontFamily FindFontFamily()
        {
            foreach (string fontName in FontNames)
            {
                try
                {
                    return new FontFamily(fontName);
                }
                catch (ArgumentException)
                {
                }
            }

            return FontFamily.GenericSansSerif;
        }

        private static DirectoryInfo FindProjectDirectory()
        {
            DirectoryInfo directory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (directory != null && !File.Exists(Path.Combine(directory.FullName, "PinyinSwitcher.csproj")))
            {
                directory = directory.Parent;
            }

            if (directory == null)
            {
                throw new DirectoryNotFoundException("未找到 PinyinSwitcher.csproj，无法确定 Resources 输出目录。");
            }

            return directory;
        }

        private static void ValidateIcon(string path)
        {
            using (BinaryReader reader = new BinaryReader(File.OpenRead(path)))
            {
                if (reader.ReadUInt16() != 0 || reader.ReadUInt16() != 1 || reader.ReadUInt16() != IconSizes.Length)
                {
                    throw new InvalidDataException("生成的 ICO 文件头无效。");
                }

                foreach (int size in IconSizes)
                {
                    int width = reader.ReadByte();
                    int height = reader.ReadByte();
                    reader.BaseStream.Position += 14;
                    if ((width == 0 ? 256 : width) != size || (height == 0 ? 256 : height) != size)
                    {
                        throw new InvalidDataException("生成的 ICO 缺少 " + size + "x" + size + " 图像。");
                    }
                }
            }

            using (Icon icon = new Icon(path))
            {
            }
        }
    }
}
