using StbImageSharp;
using System.Drawing;
using System.Drawing.Imaging;
using Veldrid;
using Vulkan;

namespace VeldridAPPvk.Engine
{
    internal class TexturesBuffer
    {
        public List<Texture> Textures { get ; private set; }
        public List<TextureView> TexturesView { get; private set; }
        public Sampler SamplerConf { get; private set; }
        public List<System.Resources.ResourceSet> resources {  get; private set; }
        public Veldrid.ResourceSet textureSet { get; private set; }
        private GraphicsDevice Device;

        public TexturesBuffer(GraphicsDevice device)
        {
            Textures = new List<Texture>();
            TexturesView = new List<TextureView>();
            resources = new List<System.Resources.ResourceSet>();

            SamplerConf = device.ResourceFactory.CreateSampler(SamplerDescription.Linear);
            this.Device = device;
           
        }

        private unsafe Texture LoadTexture(string pathload = null)
        {

            var stream = File.OpenRead(pathload);
            ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

            TextureDescription textureDescription = new TextureDescription(
                (uint)image.Width,
                (uint)image.Height,
                1,
                1,
                1,
                Veldrid.PixelFormat.R8_G8_B8_A8_UNorm,
                TextureUsage.Sampled,
                TextureType.Texture2D);

            Texture texture = Device.ResourceFactory.CreateTexture(ref textureDescription);
            fixed (byte* dataPtr = image.Data)
            {
                nint dataIntPtr = (nint)dataPtr;
                Device.UpdateTexture(
                    texture,
                    dataIntPtr,
                    (uint)(image.Width * image.Height * 4),
                    0,
                    0,
                    0,
                    (uint)image.Width,
                    (uint)image.Height,
                    1,
                    0,
                    0);
            };
            return texture;
        }

        private unsafe Texture LoadTextireBitmap(Bitmap texture)
        {
            BitmapData bitmapData = texture.LockBits(
                new System.Drawing.Rectangle(0, 0, texture.Width, texture.Height),
                ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            TextureDescription textureDescription = new TextureDescription(
                (uint)texture.Width,
                (uint)texture.Height,
                1,
                1,
                1,
                Veldrid.PixelFormat.R8_G8_B8_A8_UNorm,
                TextureUsage.Sampled,
                TextureType.Texture2D);

            Texture textureR = Device.ResourceFactory.CreateTexture(ref textureDescription);

                Device.UpdateTexture(
                textureR,
                (nint)bitmapData.Scan0,
                (uint)(textureR.Width * textureR.Height * 4),
                    0,
                0,
                0,
                (uint)textureR.Width,
                    (uint)textureR.Height,
                    1,
                    0,
                    0);
            texture.UnlockBits(bitmapData);
            return textureR;
        }
        public void AddTexture(Bitmap texture = null, string pathload = "")
        {
            if (texture == null) Textures.Add(LoadTexture(pathload));  
            else Textures.Add(LoadTextireBitmap(texture));

            TexturesView.Add(Device.ResourceFactory.CreateTextureView(Textures[Textures.Count - 1]));

        }



    }
}
