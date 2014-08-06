// The MIT License (MIT)
// 
// Copyright (c) 2014 Rodrigo 'r2d2rigo' Diaz
// Portions of this code Copyright (c) 2010-2013 Alexandre Mutel
//
// See LICENSE for full license.

using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.IO;
using SharpDX.SimpleInitializer;
using System.IO;
using System.Windows;
using Windows.ApplicationModel;
using SharpDX;
using System;
#if SILVERLIGHT
using System.Windows.Media.Imaging;
#else
using Windows.UI.Xaml.Media.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Core;
using Windows.Graphics.Imaging;
#endif

namespace SampleInitializer.Samples.Common
{
    public class SceneRenderer : IDisposable
    {
        private SharpDXContext parentContext;
        private Texture2D texture;
        private VertexShader vertexShader;
        private PixelShader pixelShader;
        private InputLayout vertexLayout;
        private VertexBufferBinding vertexBufferBinding;
        private SharpDX.Direct3D11.Buffer constantBuffer;
        private ShaderResourceView textureView;
        private SamplerState sampler;

        public SceneRenderer(SharpDXContext context)
        {
            this.parentContext = context;
            this.parentContext.DeviceReset += context_DeviceReset;
        }

        private async void context_DeviceReset(object sender, DeviceResetEventArgs e)
        {
            this.ReleaseResources();

            string assetsPath = Package.Current.InstalledLocation.Path + "/Assets/Render/";

            byte[] vertexShaderByteCode = NativeFile.ReadAllBytes(assetsPath + "MiniCubeTexture_VS.fxo");
            this.vertexShader = new VertexShader(this.parentContext.D3DDevice, vertexShaderByteCode);

            byte[] pixelShaderByteCode = NativeFile.ReadAllBytes(assetsPath + "MiniCubeTexture_PS.fxo");
            this.pixelShader = new PixelShader(this.parentContext.D3DDevice, pixelShaderByteCode);

            this.vertexLayout = new InputLayout(this.parentContext.D3DDevice, vertexShaderByteCode, new[]
            {
                new InputElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32A32_Float, 0, 0),
                new InputElement("TEXCOORD", 0, SharpDX.DXGI.Format.R32G32_Float, 16, 0)
            });

            SharpDX.Direct3D11.Buffer vertices = SharpDX.Direct3D11.Buffer.Create(this.parentContext.D3DDevice, BindFlags.VertexBuffer, new[]
            {
                -0.5f, -0.5f, -0.5f, 0.5f,     0.0f, 1.0f,
                -0.5f,  0.5f, -0.5f, 0.5f,     0.0f, 0.0f,
                0.5f,  0.5f, -0.5f, 0.5f,     1.0f, 0.0f,
                -0.5f, -0.5f, -0.5f, 0.5f,     0.0f, 1.0f,
                0.5f,  0.5f, -0.5f, 0.5f,     1.0f, 0.0f,
                0.5f, -0.5f, -0.5f, 0.5f,     1.0f, 1.0f,
            });

            this.vertexBufferBinding = new VertexBufferBinding(vertices, sizeof(float) * 6, 0);

            this.constantBuffer = new SharpDX.Direct3D11.Buffer(this.parentContext.D3DDevice, Utilities.SizeOf<Matrix>(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);

            this.sampler = new SamplerState(this.parentContext.D3DDevice, new SamplerStateDescription()
            {
                Filter = Filter.MinMagMipLinear,
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Wrap,
                BorderColor = Color.Black,
                ComparisonFunction = Comparison.Never,
                MaximumAnisotropy = 16,
                MipLodBias = 0,
                MinimumLod = -float.MaxValue,
                MaximumLod = float.MaxValue
            });

#if SILVERLIGHT
            Deployment.Current.Dispatcher.BeginInvoke(() =>
#else
            await CoreWindow.GetForCurrentThread().Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
#endif

            {
                using (MemoryStream sourceStream = new MemoryStream(NativeFile.ReadAllBytes(assetsPath + "sharpdx.png")))
                {
#if SILVERLIGHT
                    BitmapImage image = new BitmapImage();
                    image.CreateOptions = BitmapCreateOptions.None;
                    image.SetSource(sourceStream);

                    WriteableBitmap bitmap = new WriteableBitmap(image);

                    using (DataStream dataStream = new DataStream(bitmap.Pixels.Length * 4, true, true))
                    {
                        dataStream.WriteRange<int>(bitmap.Pixels);
#else
                    BitmapDecoder decoder = await BitmapDecoder.CreateAsync(sourceStream.AsRandomAccessStream());
                    BitmapFrame bitmap = await decoder.GetFrameAsync(0);
                    PixelDataProvider dataProvider = await bitmap.GetPixelDataAsync(BitmapPixelFormat.Bgra8,
                        BitmapAlphaMode.Premultiplied,
                        new BitmapTransform(),
                        ExifOrientationMode.IgnoreExifOrientation,
                        ColorManagementMode.DoNotColorManage);
                    byte[] pixelData = dataProvider.DetachPixelData();

                    using (DataStream dataStream = new DataStream(pixelData.Length, true, true))
                    {
                        dataStream.WriteRange<byte>(pixelData);
#endif

                        dataStream.Seek(0, SeekOrigin.Begin);

                        DataRectangle dataRectangle = new DataRectangle(dataStream.DataPointer, (int)(bitmap.PixelWidth * 4));

                        this.texture = new Texture2D(this.parentContext.D3DDevice, new Texture2DDescription()
                        {
                            Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                            Width = (int)bitmap.PixelWidth,
                            Height = (int)bitmap.PixelHeight,
                            ArraySize = 1,
                            MipLevels = 1,
                            BindFlags = BindFlags.ShaderResource,
                            Usage = ResourceUsage.Default,
                            CpuAccessFlags = CpuAccessFlags.None,
                            OptionFlags = ResourceOptionFlags.None,
                            SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0)
                        }, dataRectangle);

                        this.textureView = new ShaderResourceView(this.parentContext.D3DDevice, this.texture);
                    }

#if SILVERLIGHT
                    bitmap = null;
                    image = null;
#else
                    pixelData = null;
                    dataProvider = null;
                    bitmap = null;
                    decoder = null;
#endif
                }
            });
        }

        public void Render()
        {
            int width = (int)this.parentContext.BackBufferSize.Width;
            int height = (int)this.parentContext.BackBufferSize.Height;

            var view = SharpDX.Matrix.LookAtLH(new Vector3(0, 0, -1), new Vector3(0, 0, 0), Vector3.UnitY);
            var proj = SharpDX.Matrix.OrthoLH(width * 2, height * 2, -10, 10);
            var viewProj = SharpDX.Matrix.Multiply(view, proj);

            this.parentContext.D3DContext.OutputMerger.SetTargets(this.parentContext.DepthStencilView, this.parentContext.BackBufferView);
            this.parentContext.D3DContext.ClearDepthStencilView(this.parentContext.DepthStencilView, DepthStencilClearFlags.Depth, 1.0f, 0);
            this.parentContext.D3DContext.ClearRenderTargetView(this.parentContext.BackBufferView, SharpDX.Color.CornflowerBlue.ToColor4());

            var worldViewProj = SharpDX.Matrix.Scaling(400, 400, 1) *
                SharpDX.Matrix.Translation(Vector3.Zero) *
                viewProj;
            worldViewProj.Transpose();

            this.parentContext.D3DContext.InputAssembler.SetVertexBuffers(0, vertexBufferBinding);
            this.parentContext.D3DContext.InputAssembler.InputLayout = vertexLayout;
            this.parentContext.D3DContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            this.parentContext.D3DContext.VertexShader.SetConstantBuffer(0, constantBuffer);
            this.parentContext.D3DContext.VertexShader.Set(vertexShader);
            this.parentContext.D3DContext.PixelShader.SetShaderResource(0, textureView);
            this.parentContext.D3DContext.PixelShader.SetSampler(0, sampler);
            this.parentContext.D3DContext.PixelShader.Set(pixelShader);

            this.parentContext.D3DContext.UpdateSubresource(ref worldViewProj, constantBuffer, 0);
            this.parentContext.D3DContext.Draw(6, 0);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void ReleaseResources()
        {
            Utilities.Dispose(ref this.pixelShader);
            Utilities.Dispose(ref this.vertexShader);
            Utilities.Dispose(ref this.vertexLayout);
            Utilities.Dispose(ref this.constantBuffer);
            Utilities.Dispose(ref this.sampler);
            Utilities.Dispose(ref this.texture);
            Utilities.Dispose(ref this.textureView);

            if (this.vertexBufferBinding.Buffer != null)
            {
                this.vertexBufferBinding.Buffer.Dispose();
                this.vertexBufferBinding.Buffer = null;
            }
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.ReleaseResources();

                this.parentContext = null;
            }
        }
    }
}