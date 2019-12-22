using System;
using Sokol.AppKit;
using static SDL2.SDL;
using static Sokol.sokol_gfx;

namespace Sokol.Samples
{
    public abstract class App : IDisposable
    {
        private readonly SgDevice _device;
        private readonly Renderer _renderer;
        private bool _isExiting;

        protected App(SgDeviceDescription? deviceDescription = null)
        {
            SDL_Init(SDL_INIT_VIDEO);

            var description = deviceDescription ?? new SgDeviceDescription();

            WindowHandle = CreateWindow();
            _renderer = CreateRenderer(ref description, WindowHandle);

            _device = new SgDevice(ref description);
            GraphicsBackend = _device.GraphicsBackend;
        }

        public IntPtr WindowHandle { get; }

        public GraphicsBackend GraphicsBackend { get; }

        public void Dispose()
        {
            ReleaseResources();
            GC.SuppressFinalize(this);
        }

        private IntPtr CreateWindow()
        {
            var windowFlags = SDL_WindowFlags.SDL_WINDOW_HIDDEN |
                              SDL_WindowFlags.SDL_WINDOW_ALLOW_HIGHDPI |
                              SDL_WindowFlags.SDL_WINDOW_RESIZABLE;

            if (GraphicsBackend == GraphicsBackend.OpenGL)
            {
                windowFlags |= SDL_WindowFlags.SDL_WINDOW_OPENGL;
            }

            var windowHandle = SDL_CreateWindow("",
                SDL_WINDOWPOS_CENTERED, SDL_WINDOWPOS_CENTERED,
                800, 600, windowFlags);

            if (windowHandle == IntPtr.Zero)
            {
                throw new ApplicationException("Failed to create a window with SDL2.");
            }

            return windowHandle;
        }

        private static Renderer CreateRenderer(ref SgDeviceDescription deviceDescription, IntPtr windowHandle)
        {
            var backend = deviceDescription.GraphicsBackend;
            if (backend == GraphicsBackend.OpenGL)
            {
                return new RendererOpenGL(windowHandle);
            }

            var sysWmInfo = new SDL_SysWMinfo();
            SDL_GetWindowWMInfo(windowHandle, ref sysWmInfo);

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (sysWmInfo.subsystem)
            {
                case SDL_SYSWM_TYPE.SDL_SYSWM_COCOA:
                    var nsWindow = new NSWindow(sysWmInfo.info.cocoa.window);
                    return new RendererMetal(ref deviceDescription, windowHandle, nsWindow);
                case SDL_SYSWM_TYPE.SDL_SYSWM_UIKIT:
                // TODO: iOS Metal
                case SDL_SYSWM_TYPE.SDL_SYSWM_WINDOWS:
                // TODO: Windows DirectX11
                case SDL_SYSWM_TYPE.SDL_SYSWM_WINRT:
                // TODO: Windows DirectX11
                case SDL_SYSWM_TYPE.SDL_SYSWM_ANDROID:
                // TODO: Android OpenGL ES
                default:
                    throw new PlatformNotSupportedException("Cannot create a SwapchainSource for " +
                                                            sysWmInfo.subsystem + ".");
            }
        }

        public void Run()
        {
            SDL_ShowWindow(WindowHandle);

            while (!_isExiting)
            {
                Tick();
            }

            Exit();
        }

        private void Tick()
        {
            while (SDL_PollEvent(out var e) != 0)
            {
                switch (e.type)
                {
                    case SDL_EventType.SDL_QUIT:
                        _isExiting = true;
                        break;
                }
            }

            var (width, height) = _renderer.GetDrawableSize();
            Draw(width, height);
            sg_commit();
            _renderer.Present();
        }

        protected abstract void Draw(int drawableWidth, int drawableHeight);

        public void Exit()
        {
            Dispose();
        }

        private void ReleaseResources()
        {
            _device.Dispose();
            _renderer.Dispose();

            if (WindowHandle != IntPtr.Zero)
            {
                SDL_DestroyWindow(WindowHandle);
            }

            SDL_Quit();
        }

        ~App()
        {
            ReleaseResources();
        }
    }
}