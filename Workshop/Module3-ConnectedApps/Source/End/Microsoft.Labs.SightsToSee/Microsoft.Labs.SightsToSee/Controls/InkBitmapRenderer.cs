using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.UI.Input.Inking;
using Microsoft.Graphics.Canvas;

namespace Microsoft.Labs.SightsToSee.Controls
{
    public class InkBitmapRenderer
    {
        private CanvasDevice _canvasDevice;

        public InkBitmapRenderer() : this(false)
        {
        }

        public InkBitmapRenderer(bool forceSoftwareRenderer)
        {
            _canvasDevice = CanvasDevice.GetSharedDevice(forceSoftwareRenderer);
            _canvasDevice.DeviceLost += HandleDeviceLost;
        }

        public async Task<SoftwareBitmap> RenderAsync(IEnumerable<InkStroke> inkStrokes, double width, double height)
        {
            var dpi = DisplayInformation.GetForCurrentView().LogicalDpi;
            try
            {
                var renderTarget = new CanvasRenderTarget(_canvasDevice, (float) width, (float) height, dpi);
                using (renderTarget)
                {
                    using (var drawingSession = renderTarget.CreateDrawingSession())
                    {
                        drawingSession.DrawInk(inkStrokes);
                    }

                    return await SoftwareBitmap.CreateCopyFromSurfaceAsync(renderTarget);
                }
            }
            catch (Exception e) when (_canvasDevice.IsDeviceLost(e.HResult))
            {
                _canvasDevice.RaiseDeviceLost();
            }

            return null;
        }

        private void HandleDeviceLost(CanvasDevice sender, object args)
        {
            if (sender == _canvasDevice)
            {
                RecreateDevice();
            }
        }

        private void RecreateDevice()
        {
            _canvasDevice.DeviceLost -= HandleDeviceLost;

            _canvasDevice = CanvasDevice.GetSharedDevice(_canvasDevice.ForceSoftwareRenderer);
            _canvasDevice.DeviceLost += HandleDeviceLost;
        }
    }
}