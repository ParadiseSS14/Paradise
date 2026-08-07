namespace Content.Client.UserInterface.Tweens.Easers;

    public static class PresetEasingDelegateFactory
    {
        const float C1 = 1.70158f;
        const float C2 = C1 * 1.525f;
        const float C3 = C1 + 1;
        const float C4 = (2 * (float)Math.PI) / 3;
        const float C5 = (2 * (float)Math.PI) / 4.5f;

        const float N1 = 7.5625f;
        const float D1 = 2.75f;

        public static EasingDelegate GetEaseDelegate(Easers.Easing easing)
        {
            EasingDelegate result;

            switch (easing)
            {
                default:
                case Easers.Easing.Linear: result = Linear; break;
                case Easers.Easing.InSine: result = InSine; break;
                case Easers.Easing.OutSine: result = OutSine; break;
                case Easers.Easing.InOutSine: result = InOutSine; break;
                case Easers.Easing.InQuad: result = InQuad; break;
                case Easers.Easing.OutQuad: result = OutQuad; break;
                case Easers.Easing.InOutQuad: result = InOutQuad; break;
                case Easers.Easing.InCubic: result = InCubic; break;
                case Easers.Easing.OutCubic: result = OutCubic; break;
                case Easers.Easing.InOutCubic: result = InOutCubic; break;
                case Easers.Easing.InQuart: result = InQuart; break;
                case Easers.Easing.OutQuart: result = OutQuart; break;
                case Easers.Easing.InOutQuart: result = InOutQuart; break;
                case Easers.Easing.InQuint: result = InQuint; break;
                case Easers.Easing.OutQuint: result = OutQuint; break;
                case Easers.Easing.InOutQuint: result = InOutQuint; break;
                case Easers.Easing.InExpo: result = InExpo; break;
                case Easers.Easing.OutExpo: result = OutExpo; break;
                case Easers.Easing.InOutExpo: result = InOutExpo; break;
                case Easers.Easing.InCirc: result = InCirc; break;
                case Easers.Easing.OutCirc: result = OutCirc; break;
                case Easers.Easing.InOutCirc: result = InOutCirc; break;
                case Easers.Easing.InBack: result = InBack; break;
                case Easers.Easing.OutBack: result = OutBack; break;
                case Easers.Easing.InOutBack: result = InOutBack; break;
                case Easers.Easing.InElastic: result = InElastic; break;
                case Easers.Easing.OutElastic: result = OutElastic; break;
                case Easers.Easing.InOutElastic: result = InOutElastic; break;
                case Easers.Easing.InBounce: result = InBounce; break;
                case Easers.Easing.OutBounce: result = OutBounce; break;
                case Easers.Easing.InOutBounce: result = InOutBounce; break;
            }

            return result;
        }

        static float Linear(float a, float b, float t)
        {
            return Lerp(a, b, t);
        }

        static float InSine(float a, float b, float t)
        {
            return Lerp(a, b, 1 - (float)Math.Cos((t * (float)Math.PI) / 2f));
        }

        static float OutSine(float a, float b, float t)
        {
            return Lerp(a, b, (float)Math.Sin((t * (float)Math.PI) / 2f));
        }

        static float InOutSine(float a, float b, float t)
        {
            return Lerp(a, b, -((float)Math.Cos(t * (float)Math.PI) - 1) / 2f);
        }

        static float InQuad(float a, float b, float t)
        {
            return Lerp(a, b, t * t);
        }

        static float OutQuad(float a, float b, float t)
        {
            return Lerp(a, b, 1 - (1 - t) * (1 - t));
        }

        static float InOutQuad(float a, float b, float t)
        {
            return Lerp(a, b, t < 0.5f ? 2 * t * t : 1 - (float)Math.Pow(-2 * t + 2, 2) / 2);
        }

        static float InCubic(float a, float b, float t)
        {
            return Lerp(a, b, t * t * t);
        }

        static float OutCubic(float a, float b, float t)
        {
            return Lerp(a, b, 1 - (float)Math.Pow(1 - t, 3));
        }

        static float InOutCubic(float a, float b, float t)
        {
            return Lerp(a, b, t < 0.5f ? 4 * t * t * t : 1 -(float) Math.Pow(-2 * t + 2, 3) / 2);
        }

        static float InQuart(float a, float b, float t)
        {
            return Lerp(a, b, t * t * t * t);
        }

        static float OutQuart(float a, float b, float t)
        {
            return Lerp(a, b, 1 - (float)Math.Pow(1 - t, 4));
        }

        static float InOutQuart(float a, float b, float t)
        {
            return Lerp(a, b, t < 0.5f ? 8 * t * t * t * t : 1 - (float)Math.Pow(-2 * t + 2, 4) / 2);
        }

        static float InQuint(float a, float b, float t)
        {
            return Lerp(a, b, t * t * t * t * t);
        }

        static float OutQuint(float a, float b, float t)
        {
            return Lerp(a, b, 1 - (float)Math.Pow(1 - t, 5));
        }

        static float InOutQuint(float a, float b, float t)
        {
            return Lerp(a, b, t < 0.5f ? 16 * t * t * t * t * t : 1 - (float)Math.Pow(-2 * t + 2, 5) / 2);
        }

        static float InExpo(float a, float b, float t)
        {
            return Lerp(a, b, t == 0 ? 0 : (float)Math.Pow(2, 10 * t - 10));
        }

        static float OutExpo(float a, float b, float t)
        {
            return Lerp(a, b, t == 1 ? 1 : 1 - (float)Math.Pow(2, -10 * t));
        }

        static float InOutExpo(float a, float b, float t)
        {
            return Lerp(a, b, t == 0 ? 0 : t == 1 ? 1 : t < 0.5f ? (float)Math.Pow(2, 20 * t - 10) / 2 : (2 - (float)Math.Pow(2, -20 * t + 10)) / 2);
        }

        static float InCirc(float a, float b, float t)
        {
            return Lerp(a, b, 1 - (float)Math.Sqrt(1 - (float)Math.Pow(t, 2)));
        }

        static float OutCirc(float a, float b, float t)
        {
            return Lerp(a, b, (float)Math.Sqrt(1 - (float)Math.Pow(t - 1, 2)));
        }

        static float InOutCirc(float a, float b, float t)
        {
            return Lerp(a, b, t < 0.5f ? (1 - (float)Math.Sqrt(1 - (float)Math.Pow(2 * t, 2))) / 2 : (float)(Math.Sqrt(1 - Math.Pow(-2 * t + 2, 2)) + 1) / 2);
        }

        static float InBack(float a, float b, float t)
        {
            return Lerp(a, b, C3 * t * t * t - C1 * t * t);
        }

        static float OutBack(float a, float b, float t)
        {
            return Lerp(a, b, 1 + C3 * (float)Math.Pow(t - 1, 3) + C1 * (float)Math.Pow(t - 1, 2));
        }

        static float InOutBack(float a, float b, float t)
        {
            return Lerp(a, b, t < 0.5f ? ((float)Math.Pow(2 * t, 2) * ((C2 + 1) * 2 * t - C2)) / 2 : ((float)Math.Pow(2 * t - 2, 2) * ((C2 + 1) * (t * 2 - 2) + C2) + 2) / 2);
        }

        static float InElastic(float a, float b, float t)
        {
            return Lerp(a, b, t == 0 ? 0 : t == 1 ? 1 : -(float)Math.Pow(2, 10 * t - 10) * (float)Math.Sin((t * 10 - 10.75f) * C4));
        }

        static float OutElastic(float a, float b, float t)
        {
            return Lerp(a, b, t == 0 ? 0 : t == 1 ? 1 : (float)Math.Pow(2, -10 * t) * (float)Math.Sin((t * 10 - 0.75f) * C4) + 1);
        }

        static float InOutElastic(float a, float b, float t)
        {
            return Lerp(a, b, t == 0 ? 0 : t == 1 ? 1 : t < 0.5f ? -((float)Math.Pow(2, 20 * t - 10) * (float)Math.Sin((20 * t - 11.125f) * C5)) / 2 : ((float)Math.Pow(2, -20 * t + 10) * (float)Math.Sin((20 * t - 11.125f) * C5)) / +1);
        }

        static float InBounce(float a, float b, float t)
        {
            return Lerp(a, b, 1 - RawOutBounce(1 - t));
        }

        static float OutBounce(float a, float b, float t)
        {
            return Lerp(a, b, RawOutBounce(t));
        }

        static float InOutBounce(float a, float b, float t)
        {
            return Lerp(a, b, t < 0.5f ? (1 - RawOutBounce(1 - 2 * t)) / 2 : (1 + RawOutBounce(2 * t - 1)) / 2);
        }

        static float RawOutBounce(float t)
        {
            if (t < 1 / D1)
            {
                return N1 * t * t;
            }

            if (t < 2 / D1)
            {
                return N1 * (t -= 1.5f / D1) * t + 0.75f;
            }

            if (t < 2.5f / D1)
            {
                return N1 * (t -= 2.25f / D1) * t + 0.9375f;
            }

            return N1 * (t -= 2.625f / D1) * t + 0.984375f;
        }

        static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * t;
        }
    }

    public delegate float EasingDelegate(float a, float b, float t);
