using System;
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
            _limaconDrawer = new Drawer(LimaconType.TypeA);
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

            int width, height;
            SDL.SDL_GetWindowSize(_window, out width, out height);

            SDL.SDL_Point[] points = _limaconDrawer.Draw(width, height);

            PaintLimacon(points, width, height);

            SDL.SDL_SetRenderDrawColor(_renderer, 255, 255, 255, 255);
            SDL.SDL_RenderDrawPoints(_renderer, points, points.Length);

            SDL.SDL_RenderPresent(_renderer);
        }

        private void PaintLimacon(SDL.SDL_Point[] points, int width, int height)
        {
            int centerX = width / 2;
            int centerY = height / 2;
            SDL.SDL_Point[] currentPoints;

            // I Quater
            currentPoints = points
                .Where(p => p.x >= centerX && p.y >= centerY)
                .ToArray();
            PaintIOrIVQuater(currentPoints, centerY, 255, 255, 0);

            // II Quater

            currentPoints = points
                .Where(p => p.x < centerX && p.y >= centerY)
                .ToArray();
            PaintIIOrIIIQuater(currentPoints, centerY, 255, 0, 0);

            // III Quater

            currentPoints = points
                .Where(p => p.x < centerX && p.y < centerY)
                .ToArray();
            PaintIIOrIIIQuater(currentPoints, centerY, 0, 255, 0);

            // IV Quater
            currentPoints = points
                .Where(p => p.x >= centerX && p.y < centerY)
                .ToArray();
            PaintIOrIVQuater(currentPoints, centerY, 0, 0, 255);
        }

        private void PaintIOrIVQuater(SDL.SDL_Point[] points, int centerY, byte r, byte g, byte b)
        {
            SDL.SDL_SetRenderDrawColor(_renderer, r, g, b, 255);
            foreach (SDL.SDL_Point point in points)
            {
                SDL.SDL_RenderDrawLine(_renderer, point.x, point.y, point.x, centerY);
            }
        }

        private void PaintIIOrIIIQuater(SDL.SDL_Point[] points, int centerY, byte r, byte g, byte b)
        {
            int[] xCoordinates = points
                .Select(p => p.x)
                .ToArray();

            SDL.SDL_SetRenderDrawColor(_renderer, r, g, b, 255);
            foreach (int xCoordinate in xCoordinates)
            {
                SDL.SDL_Point[] pointsWithSameX = points
                    .Where(p => p.x == xCoordinate)
                    .ToArray();

                int maxY = pointsWithSameX
                    .Select(p => p.y)
                    .Max();
                int minY = pointsWithSameX
                    .Select(p => p.y)
                    .Min();

                if (Math.Abs(maxY - minY) < 10)
                {
                    SDL.SDL_RenderDrawLine(_renderer, xCoordinate, maxY, xCoordinate, centerY);
                }
                else
                {
                    SDL.SDL_RenderDrawLine(_renderer, xCoordinate, maxY, xCoordinate, minY);
                }
            }
        }
    }
}
