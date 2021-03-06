﻿using System;
using System.Collections.Generic;
using System.Linq;
using Trady.Analysis.Infrastructure;
using Trady.Core;

namespace Trady.Analysis.Indicator
{
    public class OnBalanceVolume<TInput, TOutput> : CumulativeAnalyzableBase<TInput, (decimal Close, decimal Volume), decimal?, TOutput>
    {
        public OnBalanceVolume(IEnumerable<TInput> inputs, Func<TInput, (decimal Close, decimal Volume)> inputMapper, Func<TInput, decimal?, TOutput> outputMapper) : base(inputs, inputMapper, outputMapper)
        {
        }

        protected override decimal? ComputeInitialValue(IEnumerable<(decimal Close, decimal Volume)> mappedInputs, int index) => mappedInputs.ElementAt(index).Volume;

        protected override decimal? ComputeCumulativeValue(IEnumerable<(decimal Close, decimal Volume)> mappedInputs, int index, decimal? prevOutputToMap)
        {
            var input = mappedInputs.ElementAt(index);
            var prevInput = mappedInputs.ElementAt(index - 1);
			decimal increment = input.Volume * (input.Close > prevInput.Close ? 1 : (input.Close == prevInput.Close ? 0 : -1));
			return prevOutputToMap + increment;
        }
    }

    public class OnBalanceVolumeByTuple : OnBalanceVolume<(decimal Close, decimal Volume), decimal?>
    {
        public OnBalanceVolumeByTuple(IEnumerable<(decimal Close, decimal Volume)> inputs) 
            : base(inputs, i => i, (i, otm) => otm)
        {
        }
    }

    public class OnBalanceVolume : OnBalanceVolume<Candle, AnalyzableTick<decimal?>>
    {
        public OnBalanceVolume(IEnumerable<Candle> inputs) 
            : base(inputs, i => (i.Close, i.Volume), (i, otm) => new AnalyzableTick<decimal?>(i.DateTime, otm))
        {
        }
    }
}