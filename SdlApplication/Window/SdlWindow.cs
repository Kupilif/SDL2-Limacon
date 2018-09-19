using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SdlApplication.Limacon;
using SDL2;

namespace SdlApplication.Window
{
    public class SdlWindow
    {
        private readonly int _renderLoopTimeoutMs = 10;

        private readonly int _screenWidth;
        private readonly int _screenHeight;
        private readonly string _title;
        private readonly Drawer _limaconDrawer;

        private IntPtr _renderer;
        private IntPtr _window;

        public SdlWindow(string title, int screenWidth, int screenHeight)
        {
            _title = title;
            _screenHeight = screenHeight;
            _screenWidth = screenWidth;
            _limaconDrawer = new Drawer(LimaconType.TypeC);
        }

        public void Open()
        {
            var thred = new Thread(() =>
            {
                SDL.SDL_Init(SDL.SDL_INIT_EVERYTHING);
                _window = SDL.SDL_CreateWindow(_title, SDL.SDL_WINDOWPOS_CENTERED, SDL.SDL_WINDOWPOS_CENTERED,
                    _screenWidth, _screenHeight, SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE);
                _renderer = SDL.SDL_CreateRenderer(_window, -1, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED);

                WindowProcedure();

                SDL.SDL_DestroyRenderer(_renderer);
                SDL.SDL_DestroyWindow(_window);
                SDL.SDL_Quit();
            });
            thred.Start();
            thred.Join();
        }

        private void WindowProcedure()
        {
            bool exit = false;
            while (!exit)
            {
                SDL.SDL_Event sdlEvent;
                SDL.SDL_PollEvent(out sdlEvent);
                switch (sdlEvent.type)
                {
                    case SDL.SDL_EventType.SDL_QUIT:
                    {
                        exit = true;
                        break;
                    }
                    case SDL.SDL_EventType.SDL_KEYDOWN:
                    {
                        var key = sdlEvent.key;
                        switch (key.keysym.sym)
                        {
                            case SDL.SDL_Keycode.SDLK_SPACE:
                                _limaconDrawer.SwitchType();
                                break;
                        }
                        break;
                    }
                }
                DrawLimacon();
                Thread.Sleep(_renderLoopTimeoutMs);
            }
        }

        // Формат цвета в HEX коде:
        //     0xRRGGBB00
        //  где R: от 00 до FF
        //      G: от 00 до FF
        //      B: от 00 до FF 
        private void DrawLimacon()
        {
            SDL.SDL_SetRenderDrawColor(_renderer, 0, 0, 0, 255);
            SDL.SDL_RenderClear(_renderer);
            SDL.SDL_SetRenderDrawColor(_renderer, 255, 255, 255, 255);

            int width, height;
            SDL.SDL_GetWindowSize(_window, out width, out height);

            SDL.SDL_Point[] points = _limaconDrawer.Draw(width, height);

            PaintLimacon(points, height / 2, 255, 255, 0);

            SDL.SDL_SetRenderDrawColor(_renderer, 255, 255, 255, 255);
            SDL.SDL_RenderDrawPoints(_renderer, points, points.Length);

            SDL.SDL_RenderPresent(_renderer);
        }

        private void PaintLimacon(SDL.SDL_Point[] points, int centerY, byte r, byte g, byte b)
        {
            var zeroPoints = points.Where(p => Math.Abs(p.y - centerY) == 0).ToArray();
            zeroPoints = zeroPoints.OrderBy(p => p.x).ToArray();
            var minX = zeroPoints[0];
            var maxX = zeroPoints[zeroPoints.Length - 2];

            var newPoints = points.Where(p => (p.x >= minX.x && p.x <= maxX.x));

            SDL.SDL_SetRenderDrawColor(_renderer, r, g, b, 255);

            foreach (var point in newPoints)
            {
                
                var curPoints = points.Where(p => p.x == point.x).OrderBy(p => p.y).ToArray();
                var temp = new List<SDL.SDL_Point>();

                foreach (var p in curPoints)
                {
                    temp.Add(new SDL.SDL_Point() { x = p.x, y = p.y - centerY });
                }

                var minusPoints = temp.Where(p => p.y <= 0).OrderBy(p => p.y).ToArray();
                var positivPoints = temp.Where(p => p.y > 0).OrderBy(p => p.y).ToArray();

                SDL.SDL_RenderDrawLine(_renderer, minusPoints[minusPoints.Length - 1].x, minusPoints[minusPoints.Length - 1].y + centerY, positivPoints[0].x, positivPoints[0].y + centerY);
            }
        }
    }
}
