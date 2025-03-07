﻿namespace Electron2D.Misc
{
    //https://easings.net/

    /// <summary>
    /// A class for evaluating preset ease modes.
    /// </summary>
    public static class Easing
    {
        #region Ease In
        public static float EaseInSine(float _time)
        {
            return 1 - MathF.Cos(_time * MathF.PI / 2f);
        }

        public static float EaseInQuad(float _time)
        {
            return _time * _time;
        }

        public static float EaseInCubic(float _time)
        {
            return _time * _time * _time;
        }

        public static float EaseInQuart(float _time)
        {
            return _time * _time * _time * _time;
        }

        public static float EaseInQuint(float _time)
        {
            return _time * _time * _time * _time * _time;
        }

        public static float EaseInCirc(float _time)
        {
            return 1 - MathF.Sqrt(1 - MathF.Pow(_time, 2));
        }

        public static float EaseInExpo(float _time)
        {
            return _time == 0 ? 0 : MathF.Pow(2, 10 * _time - 10);
        }

        public static float EaseInBack(float _time)
        {
            float c1 = 1.70158f;
            float c3 = c1 + 1;

            return c3 * _time * _time * _time - c1 * _time * _time;
        }

        public static float EaseInBounce(float _time)
        {
            return 1 - EaseOutBounce(1 - _time);
        }

        public static float EaseInElastic(float _time)
        {
            float c4 = (2 * MathF.PI) / 3;

            return _time == 0
              ? 0
              : _time == 1
              ? 1
              : -MathF.Pow(2, 10 * _time - 10) * MathF.Sin((_time * 10 - 10.75f) * c4);
        }
        #endregion

        #region Ease Out
        public static float EaseOutSine(float _time)
        {
            return MathF.Sin((_time * MathF.PI) / 2);
        }

        public static float EaseOutQuad(float _time)
        {
            return 1 - ((1 - _time) * (1 - _time));
        }

        public static float EaseOutCubic(float _time)
        {
            return 1 - MathF.Pow(1 - _time, 3);
        }

        public static float EaseOutQuart(float _time)
        {
            return 1 - MathF.Pow(1 - _time, 4);
        }

        public static float EaseOutQuint(float _time)
        {
            return 1 - MathF.Pow(1 - _time, 5);
        }

        public static float EaseOutCirc(float _time)
        {
            return MathF.Sqrt(1 - MathF.Pow(_time - 1, 2));
        }

        public static float EaseOutExpo(float _time)
        {
            return _time == 1 ? 1 : 1 - MathF.Pow(2, -10 * _time);
        }

        public static float EaseOutBack(float _time)
        {
            float c1 = 1.70158f;
            float c3 = c1 + 1;

            return 1 + c3 * MathF.Pow(_time - 1, 3) + c1 * MathF.Pow(_time - 1, 2);
        }

        public static float EaseOutBounce(float _time)
        {
            float n1 = 7.5625f;
            float d1 = 2.75f;

            if (_time < 1 / d1)
            {
                return n1 * _time * _time;
            }
            else if (_time < 2 / d1)
            {
                return n1 * (_time -= 1.5f / d1) * _time + 0.75f;
            }
            else if (_time < 2.5f / d1)
            {
                return n1 * (_time -= 2.25f / d1) * _time + 0.9375f;
            }
            else
            {
                return n1 * (_time -= 2.625f / d1) * _time + 0.984375f;
            }
        }

        public static float EaseOutElastic(float _time)
        {
            float c4 = (2 * MathF.PI) / 3;

            return _time == 0
              ? 0
              : _time == 1
              ? 1
              : MathF.Pow(2, -10 * _time) * MathF.Sin((_time * 10 - 0.75f) * c4) + 1;
        }
        #endregion

        #region Ease In Out
        public static float EaseInOutSine(float _time)
        {
            return -(MathF.Cos(MathF.PI * _time) - 1) / 2;
        }

        public static float EaseInOutQuad(float _time)
        {
            return _time < 0.5 ? 2 * _time * _time : 1 - MathF.Pow(-2 * _time + 2, 2) / 2;
        }

        public static float EaseInOutCubic(float _time)
        {
            return _time < 0.5 ? 4 * _time * _time * _time : 1 - MathF.Pow(-2 * _time + 2, 3) / 2;
        }

        public static float EaseInOutQuart(float _time)
        {
            return _time < 0.5 ? 8 * _time * _time * _time * _time : 1 - MathF.Pow(-2 * _time + 2, 4) / 2;
        }

        public static float EaseInOutQuint(float _time)
        {
            return _time < 0.5 ? 16 * _time * _time * _time * _time * _time : 1 - MathF.Pow(-2 * _time + 2, 5) / 2;
        }

        public static float EaseInOutExpo(float _time)
        {
            return _time == 0
                ? 0
                : _time == 1
                ? 1
                : _time < 0.5 ? MathF.Pow(2, 20 * _time - 10) / 2
                : (2 - MathF.Pow(2, -20 * _time + 10)) / 2;
        }

        public static float EaseInOutCirc(float _time)
        {
            return _time < 0.5
                ? (1 - MathF.Sqrt(1 - MathF.Pow(2 * _time, 2))) / 2
                : (MathF.Sqrt(1 - MathF.Pow(-2 * _time + 2, 2)) + 1) / 2;
        }

        public static float EaseInOutBack(float _time)
        {
            float c1 = 1.70158f;
            float c2 = c1 * 1.525f;

            return _time < 0.5
              ? (MathF.Pow(2 * _time, 2) * ((c2 + 1) * 2 * _time - c2)) / 2
              : (MathF.Pow(2 * _time - 2, 2) * ((c2 + 1) * (_time * 2 - 2) + c2) + 2) / 2;
        }

        public static float EaseInOutElastic(float _time)
        {
            float c5 = (2 * MathF.PI) / 4.5f;

            return _time == 0
                ? 0
                : _time == 1
                ? 1
                : _time < 0.5
                ? -(MathF.Pow(2, 20 * _time - 10) * MathF.Sin((20 * _time - 11.125f) * c5)) / 2
                : (MathF.Pow(2, -20 * _time + 10) * MathF.Sin((20 * _time - 11.125f) * c5)) / 2 + 1;
        }

        public static float EaseInOutBounce(float _time)
        {
            return _time < 0.5
                ? (1 - EaseOutBounce(1 - 2 * _time)) / 2
                : (1 + EaseOutBounce(2 * _time - 1)) / 2;
        }
        #endregion
    }
}