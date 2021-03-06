﻿using ReOsuStoryboardPlayer.Core.Commands;
using ReOsuStoryboardPlayer.Core.PrimitiveValue;
using System.Collections.Generic;
using System.Linq;

namespace ReOsuStoryboardPlayer.Core.Parser.CommandParser.ValueCommandParser
{
    public class SplitableScaleCommandParser : FloatCommandParser<ScaleCommand>
    {
        public override IEnumerable<Command> Parse(IEnumerable<string> data_arr)
        {
            return base.Parse(data_arr).OfType<ScaleCommand>().Select(p => SplitCommand(p)).SelectMany(l => l);
        }

        private IEnumerable<Command> SplitCommand(ScaleCommand scale)
        {
            var w = _get<VectorScaleCommand>(scale);
            w.StartValue=new Vector(scale.StartValue, scale.StartValue);
            w.EndValue=new Vector(scale.EndValue, scale.EndValue);
            yield return w;

            T _get<T>(ValueCommand c) where T : ValueCommand, new()
            {
                return new T()
                {
                    StartTime=c.StartTime,
                    EndTime=c.EndTime,
                    Easing=c.Easing,
                };
            }
        }
    }
}