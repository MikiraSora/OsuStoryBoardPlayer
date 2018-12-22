﻿using System;
using System.Collections.Generic;
using static System.Math;

namespace ReOsuStoryBoardPlayer
{
    public enum EasingTypes
    {
        None,
        Out,
        In,
        InQuad,
        OutQuad,
        InOutQuad,
        InCubic,
        OutCubic,
        InOutCubic,
        InQuart,
        OutQuart,
        InOutQuart,
        InQuint,
        OutQuint,
        InOutQuint,
        InSine,
        OutSine,
        InOutSine,
        InExpo,
        OutExpo,
        InOutExpo,
        InCirc,
        OutCirc,
        InOutCirc,
        InElastic,
        OutElastic,
        OutElasticHalf,
        OutElasticQuarter,
        InOutElastic,
        InBack,
        OutBack,
        InOutBack,
        InBounce,
        OutBounce,
        InOutBounce
    }


    /*
    From osu! source code. 
    */
    public static class OsuEasingInterpolator
    {
        /// <summary>
        /// 缓动函数
        /// </summary>
        /// <param name="easing">缓动类型</param>
        /// <param name="time">当前相对时间</param>
        /// <param name="initial">初始值</param>
        /// <param name="change">总变化值</param>
        /// <param name="duration">总时间</param>
        /// <returns>归一化值</returns>
        public static double ApplyEasing(EasingTypes easing, double time, double initial, double change, double duration)
        {
            if (change == 0 || time == 0 || duration == 0) return initial;
            if (time == duration) return initial + change;

            switch (easing)
            {
                default:
                    return change * (time / duration) + initial;

                case EasingTypes.In:
                case EasingTypes.InQuad:
                    return change * (time /= duration) * time + initial;

                case EasingTypes.Out:
                case EasingTypes.OutQuad:
                    return -change * (time /= duration) * (time - 2) + initial;

                case EasingTypes.InOutQuad:
                    if ((time /= duration / 2) < 1) return change / 2 * time * time + initial;
                    return -change / 2 * ((--time) * (time - 2) - 1) + initial;

                case EasingTypes.InCubic:
                    return change * (time /= duration) * time * time + initial;

                case EasingTypes.OutCubic:
                    return change * ((time = time / duration - 1) * time * time + 1) + initial;

                case EasingTypes.InOutCubic:
                    if ((time /= duration / 2) < 1) return change / 2 * time * time * time + initial;
                    return change / 2 * ((time -= 2) * time * time + 2) + initial;

                case EasingTypes.InQuart:
                    return change * (time /= duration) * time * time * time + initial;

                case EasingTypes.OutQuart:
                    return -change * ((time = time / duration - 1) * time * time * time - 1) + initial;

                case EasingTypes.InOutQuart:
                    if ((time /= duration / 2) < 1) return change / 2 * time * time * time * time + initial;
                    return -change / 2 * ((time -= 2) * time * time * time - 2) + initial;

                case EasingTypes.InQuint:
                    return change * (time /= duration) * time * time * time * time + initial;

                case EasingTypes.OutQuint:
                    return change * ((time = time / duration - 1) * time * time * time * time + 1) + initial;

                case EasingTypes.InOutQuint:
                    if ((time /= duration / 2) < 1) return change / 2 * time * time * time * time * time + initial;
                    return change / 2 * ((time -= 2) * time * time * time * time + 2) + initial;

                case EasingTypes.InSine:
                    return -change * Math.Cos(time / duration * (PI / 2)) + change + initial;

                case EasingTypes.OutSine:
                    return change * Math.Sin(time / duration * (PI / 2)) + initial;

                case EasingTypes.InOutSine:
                    return -change / 2 * (Math.Cos(PI * time / duration) - 1) + initial;

                case EasingTypes.InExpo:
                    return change * Math.Pow(2, 10 * (time / duration - 1)) + initial;

                case EasingTypes.OutExpo:
                    return (time == duration) ? initial + change : change * (-Math.Pow(2, -10 * time / duration) + 1) + initial;

                case EasingTypes.InOutExpo:
                    if ((time /= duration / 2) < 1) return change / 2 * Math.Pow(2, 10 * (time - 1)) + initial;
                    return change / 2 * (-Math.Pow(2, -10 * --time) + 2) + initial;

                case EasingTypes.InCirc:
                    return -change * (Math.Sqrt(1 - (time /= duration) * time) - 1) + initial;

                case EasingTypes.OutCirc:
                    return change * Math.Sqrt(1 - (time = time / duration - 1) * time) + initial;

                case EasingTypes.InOutCirc:
                    if ((time /= duration / 2) < 1) return -change / 2 * (Math.Sqrt(1 - time * time) - 1) + initial;
                    return change / 2 * (Math.Sqrt(1 - (time -= 2) * time) + 1) + initial;

                case EasingTypes.InElastic:
                    {
                        if ((time /= duration) == 1) return initial + change;

                        var p = duration * .3;
                        var a = change;
                        var s = 1.70158;
                        if (a < Math.Abs(change)) { a = change; s = p / 4; }
                        else s = p / (2 * Math.PI) * Math.Asin(change / a);
                        return -(a * Math.Pow(2, 10 * (time -= 1)) * Math.Sin((time * duration - s) * (2 * PI) / p)) + initial;
                    }
                case EasingTypes.OutElastic:
                    {
                        if ((time /= duration) == 1) return initial + change;

                        var p = duration * .3;
                        var a = change;
                        var s = 1.70158;
                        if (a < Math.Abs(change)) { a = change; s = p / 4; }
                        else s = p / (2 * PI) * Math.Asin(change / a);
                        return a * Math.Pow(2, -10 * time) * Math.Sin((time * duration - s) * (2 * PI) / p) + change + initial;
                    }
                case EasingTypes.OutElasticHalf:
                    {
                        if ((time /= duration) == 1) return initial + change;

                        var p = duration * .3;
                        var a = change;
                        var s = 1.70158;
                        if (a < Math.Abs(change)) { a = change; s = p / 4; }
                        else s = p / (2 * PI) * Math.Asin(change / a);
                        return a * Math.Pow(2, -10 * time) * Math.Sin((0.5f * time * duration - s) * (2 * PI) / p) + change + initial;
                    }
                case EasingTypes.OutElasticQuarter:
                    {
                        if ((time /= duration) == 1) return initial + change;

                        var p = duration * .3;
                        var a = change;
                        var s = 1.70158;
                        if (a < Math.Abs(change)) { a = change; s = p / 4; }
                        else s = p / (2 * PI) * Math.Asin(change / a);
                        return a * Math.Pow(2, -10 * time) * Math.Sin((0.25f * time * duration - s) * (2 * PI) / p) + change + initial;
                    }
                case EasingTypes.InOutElastic:
                    {
                        if ((time /= duration / 2) == 2) return initial + change;

                        var p = duration * (.3 * 1.5);
                        var a = change;
                        var s = 1.70158;
                        if (a < Math.Abs(change)) { a = change; s = p / 4; }
                        else s = p / (2 * PI) * Math.Asin(change / a);
                        if (time < 1) return -.5 * (a * Math.Pow(2, 10 * (time -= 1)) * Math.Sin((time * duration - s) * (2 * PI) / p)) + initial;
                        return a * Math.Pow(2, -10 * (time -= 1)) * Math.Sin((time * duration - s) * (2 * PI) / p) * .5 + change + initial;
                    }
                case EasingTypes.InBack:
                    {
                        var s = 1.70158;
                        return change * (time /= duration) * time * ((s + 1) * time - s) + initial;
                    }
                case EasingTypes.OutBack:
                    {
                        var s = 1.70158;
                        return change * ((time = time / duration - 1) * time * ((s + 1) * time + s) + 1) + initial;
                    }
                case EasingTypes.InOutBack:
                    {
                        var s = 1.70158;
                        if ((time /= duration / 2) < 1) return change / 2 * (time * time * (((s *= (1.525)) + 1) * time - s)) + initial;
                        return change / 2 * ((time -= 2) * time * (((s *= (1.525)) + 1) * time + s) + 2) + initial;
                    }
                case EasingTypes.InBounce:
                    return change - ApplyEasing(EasingTypes.OutBounce, duration - time, 0, change, duration) + initial;

                case EasingTypes.OutBounce:
                    if ((time /= duration) < (1 / 2.75))
                    {
                        return change * (7.5625 * time * time) + initial;
                    }
                    else if (time < (2 / 2.75))
                    {
                        return change * (7.5625 * (time -= (1.5 / 2.75)) * time + .75) + initial;
                    }
                    else if (time < (2.5 / 2.75))
                    {
                        return change * (7.5625 * (time -= (2.25 / 2.75)) * time + .9375) + initial;
                    }
                    else
                    {
                        return change * (7.5625 * (time -= (2.625 / 2.75)) * time + .984375) + initial;
                    }
                case EasingTypes.InOutBounce:
                    if (time < duration / 2) return ApplyEasing(EasingTypes.InBounce, time * 2, 0, change, duration) * .5 + initial;
                    return ApplyEasing(EasingTypes.OutBounce, time * 2 - duration, 0, change, duration) * .5 + change * .5 + initial;
            }
        }
    }
}