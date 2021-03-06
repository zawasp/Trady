﻿using System;
using System.Collections.Generic;
using System.Linq;
using Trady.Analysis.Infrastructure;
using Trady.Core;

namespace Trady.Analysis.Indicator
{
    public class MinusDirectionalIndicator<TInput, TOutput> : AnalyzableBase<TInput, (decimal High, decimal Low, decimal Close), decimal?, TOutput>
    {
        PlusDirectionalMovementByTuple _pdm;
        MinusDirectionalMovementByTuple _mdm;
        readonly GenericExponentialMovingAverage _tmdmEma;
        readonly AverageTrueRangeByTuple _atr;

        public MinusDirectionalIndicator(IEnumerable<TInput> inputs, Func<TInput, (decimal High, decimal Low, decimal Close)> inputMapper, Func<TInput, decimal?, TOutput> outputMapper, int periodCount) : base(inputs, inputMapper, outputMapper)
        {
			_pdm = new PlusDirectionalMovementByTuple(inputs.Select(i => inputMapper(i).High));
			_mdm = new MinusDirectionalMovementByTuple(inputs.Select(i => inputMapper(i).Low));

			Func<int, decimal?> tmdm = i => _mdm[i] > 0 && _pdm[i] < _mdm[i] ? _mdm[i] : 0;

			_tmdmEma = new GenericExponentialMovingAverage(
				periodCount,
				i => Enumerable.Range(i - periodCount + 1, periodCount).Select(j => tmdm(j)).Average(),
				i => tmdm(i),
				i => 1.0m / periodCount,
				inputs.Count());

			_atr = new AverageTrueRangeByTuple(inputs.Select(inputMapper), periodCount);

			PeriodCount = periodCount;
        }

        public int PeriodCount { get; }

        protected override decimal? ComputeByIndexImpl(IEnumerable<(decimal High, decimal Low, decimal Close)> mappedInputs, int index)
			=> _tmdmEma[index] / _atr[index] * 100;
	}

    public class MinusDirectionalIndicatorByTuple : MinusDirectionalIndicator<(decimal High, decimal Low, decimal Close), decimal?>
    {
        public MinusDirectionalIndicatorByTuple(IEnumerable<(decimal High, decimal Low, decimal Close)> inputs, int periodCount) 
            : base(inputs, i => i, (i, otm) => otm, periodCount)
        {
        }
    }

    public class MinusDirectionalIndicator : MinusDirectionalIndicator<Candle, AnalyzableTick<decimal?>>
    {
        public MinusDirectionalIndicator(IEnumerable<Candle> inputs, int periodCount) 
            : base(inputs, i => (i.High, i.Low, i.Close), (i, otm) => new AnalyzableTick<decimal?>(i.DateTime, otm), periodCount)
        {
        }
    }
}