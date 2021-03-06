﻿using System;
using System.Collections.Generic;
using System.Linq;
using Trady.Analysis.Infrastructure;
using Trady.Core;

namespace Trady.Analysis.Indicator
{
    public class AverageDirectionalIndex<TInput, TOutput> : AnalyzableBase<TInput, (decimal High, decimal Low, decimal Close), decimal?, TOutput>
    {
        DirectionalMovementIndexByTuple _dx;
        readonly GenericExponentialMovingAverage _adx;

        public AverageDirectionalIndex(IEnumerable<TInput> inputs, Func<TInput, (decimal High, decimal Low, decimal Close)> inputMapper, Func<TInput, decimal?, TOutput> outputMapper, int periodCount) : base(inputs, inputMapper, outputMapper)
        {
			_dx = new DirectionalMovementIndexByTuple(inputs.Select(inputMapper), periodCount);

            _adx = new GenericExponentialMovingAverage(
                periodCount,
                i => Enumerable.Range(i - periodCount + 1, periodCount).Select(j => _dx[j]).Average(),
                i => _dx[i],
                i => 1.0m / periodCount,
                inputs.Count());

			PeriodCount = periodCount;
        }

        public int PeriodCount { get; }

        protected override decimal? ComputeByIndexImpl(IEnumerable<(decimal High, decimal Low, decimal Close)> mappedInputs, int index) => _adx[index];
    }

    public class AverageDirectionalIndexByTuple : AverageDirectionalIndex<(decimal High, decimal Low, decimal Close), decimal?>
    {
        public AverageDirectionalIndexByTuple(IEnumerable<(decimal High, decimal Low, decimal Close)> inputs, int periodCount) 
            : base(inputs, i => i, (i, otm) => otm, periodCount)
        {
        }
    }

    public class AverageDirectionalIndex : AverageDirectionalIndex<Candle, AnalyzableTick<decimal?>>
    {
        public AverageDirectionalIndex(IEnumerable<Candle> inputs, int periodCount) 
            : base(inputs, i => (i.High, i.Low, i.Close), (i, otm) => new AnalyzableTick<decimal?>(i.DateTime, otm), periodCount)
        {
        }
    }
}