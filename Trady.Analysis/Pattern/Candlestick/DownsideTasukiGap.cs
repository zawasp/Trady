﻿using System;
using System.Collections.Generic;
using System.Linq;
using Trady.Analysis.Infrastructure;
using Trady.Analysis.Pattern.State;
using Trady.Core;

namespace Trady.Analysis.Pattern.Candlestick
{
    /// <summary>
    /// Reference: http://www.investopedia.com/terms/d/downside-tasuki-gap.asp
    /// </summary>
    public class DownsideTasukiGap<TInput, TOutput> : AnalyzableBase<TInput, (decimal Open, decimal High, decimal Low, decimal Close), bool?, TOutput>
    {
        DownTrendByTuple _downTrend;
        BearishByTuple _bearish;
        BullishByTuple _bullish;

        public DownsideTasukiGap(IEnumerable<TInput> inputs, Func<TInput, (decimal Open, decimal High, decimal Low, decimal Close)> inputMapper, Func<TInput, bool?, TOutput> outputMapper, int downTrendPeriodCount = 3, decimal sizeThreshold = 0.1m) : base(inputs, inputMapper, outputMapper)
        {
            var mappedInputs = inputs.Select(inputMapper);

            var ocs = mappedInputs.Select(i => (i.Open, i.Close));
            _downTrend = new DownTrendByTuple(mappedInputs.Select(i => (i.High, i.Low)), downTrendPeriodCount);
            _bearish = new BearishByTuple(ocs);
            _bullish = new BullishByTuple(ocs);

            DownTrendPeriodCount = downTrendPeriodCount;
            SizeThreshold = sizeThreshold;
        }

        public int DownTrendPeriodCount { get; }

        public decimal SizeThreshold { get; }

        protected override bool? ComputeByIndexImpl(IEnumerable<(decimal Open, decimal High, decimal Low, decimal Close)> mappedInputs, int index)
        {
			if (index < 2) return null;
            bool isWhiteCandleWithinGap = mappedInputs.ElementAt(index).Close < mappedInputs.ElementAt(index - 2).Low && mappedInputs.ElementAt(index).Close > mappedInputs.ElementAt(index - 1).High;
			return (_downTrend[index - 1] ?? false) &&
				_bearish[index - 2] &&
                mappedInputs.ElementAt(index - 2).Low > mappedInputs.ElementAt(index - 1).High &&
				_bearish[index - 1] &&
				_bullish[index] &&
				isWhiteCandleWithinGap;        
        }
    }

    public class DownsideTasukiGapByTuple : DownsideTasukiGap<(decimal Open, decimal High, decimal Low, decimal Close), bool?>
    {
        public DownsideTasukiGapByTuple(IEnumerable<(decimal Open, decimal High, decimal Low, decimal Close)> inputs, int downTrendPeriodCount = 3, decimal sizeThreshold = 0.1M) 
            : base(inputs, i => i, (i, otm) => otm, downTrendPeriodCount, sizeThreshold)
        {
        }
    }

    public class DownsideTasukiGap : DownsideTasukiGap<Candle, AnalyzableTick<bool?>>
    {
        public DownsideTasukiGap(IEnumerable<Candle> inputs, int downTrendPeriodCount = 3, decimal sizeThreshold = 0.1M) 
            : base(inputs, i => (i.Open, i.High, i.Low, i.Close), (i, otm) => new AnalyzableTick<bool?>(i.DateTime, otm), downTrendPeriodCount, sizeThreshold)
        {
        }
    }
}