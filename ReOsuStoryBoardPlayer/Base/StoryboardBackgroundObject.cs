﻿using ReOsuStoryBoardPlayer.Commands;

namespace ReOsuStoryBoardPlayer.Base
{
    internal class StoryboardBackgroundObject : StoryBoardObject
    {
        public StoryboardBackgroundObject()
        {
            AddCommand(new FadeCommand()
            {
                Easing = EasingConverter.GetEasingInterpolator(Easing.Linear),
                StartTime = -2857,
                EndTime = -2857,
                StartValue = 1,
                EndValue = 1
            });

            AddCommand(new FadeCommand()
            {
                Easing = EasingConverter.GetEasingInterpolator(Easing.Linear),
                StartTime = int.MaxValue - 2857,
                EndTime = int.MaxValue - 2857,
                StartValue = 1,
                EndValue = 1
            });

            /*todo
            AddCommand(new ScaleCommand()
            {
                StartTime = int.MinValue,
                EndTime = int.MinValue,
                StartValue = 1,
                EndValue = 1
            });
            */
        }
    }
}