using System;
using System.Collections.Generic;
using System.Linq;
using SDL2;

namespace SdlApplication.Limacon
{
    public class Drawer
    {
        private readonly double _step = 0.001;
        private readonly Dictionary<LimaconType, double> _aToLCoefficients = new Dictionary<LimaconType, double>()
        {
            { LimaconType.TypeA, 3 },
            { LimaconType.TypeB, 1.5 },
            { LimaconType.TypeC, 0.5 }
        };
        private readonly double _aInBigRadiusCoefficient = 0.5;
        private readonly double _lInBigRadiusCoefficient = 1;
        private readonly int _typesCount;

        private LimaconType _currentType;
        

        public Drawer(LimaconType initialType)
        {
            _currentType = initialType;
            _typesCount = Enum.GetNames(typeof(LimaconType)).Length;
        }

        public void SwitchType()
        {
            var currentTypeShort = (short) _currentType;
            _currentType = (LimaconType) (++currentTypeShort % _typesCount);
        }

        // x = a * cos^2(t) + l * cos(t)
        // y = a * cos(t) * sin(t) + l * sin(t)
        public SDL.SDL_Point[] Draw(int width, int height)
        {
            double bigRadius = Math.Min(width, height) / 2D;
            double aToLCoefficient = _aToLCoefficients[_currentType];
            double a = bigRadius / (_aInBigRadiusCoefficient + _lInBigRadiusCoefficient * aToLCoefficient);
            double l = aToLCoefficient * a;
            
            int centerX = width / 2;
            int centerY = height / 2;
            var points = new List<SDL.SDL_Point>();

            for (double t = 0; t < 2 * Math.PI; t += _step)
            {
                var point = new SDL.SDL_Point
                {
                    x = (int)Math.Round(a * Math.Pow(Math.Cos(t), 2) + l * Math.Cos(t) - a / 2) + centerX,
                    y = (int)Math.Round(a * Math.Cos(t) * Math.Sin(t) + l * Math.Sin(t)) + centerY
                };
                points.Add(point);
            }

            return points
                .Distinct()
                .ToArray();
        }
    }
}
