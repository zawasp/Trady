﻿using System;
using System.Collections.Generic;
using System.Linq;
using Trady.Analysis.Infrastructure;
using Trady.Analysis.Pattern.State;
using Trady.Core;

namespace Trady.Analysis.Pattern.Candlestick
{
    /// <summary>
    /// Reference: http://stockcharts.com/school/doku.php?id=chart_school:chart_analysis:candlestick_bearish_reversal_patterns#bearish_abandoned_baby
    /// </summary>
    public class BearishAbandonedBaby<TInput, TOutput> : AnalyzableBase<TInput, (decimal Open, decimal High, decimal Low, decimal Close), bool?, TOutput>
    {
        readonly UpTrendByTuple _upTrend;
        readonly BullishLongDayByTuple _bullishLongDay;
        readonly BearishLongDayByTuple _bearishLongDay;
        readonly DojiByTuple _doji;

        public BearishAbandonedBaby(IEnumerable<TInput> inputs, Func<TInput, (decimal Open, decimal High, decimal Low, decimal Close)> inputMapper, Func<TInput, bool?, TOutput> outputMapper, int upTrendPeriodCount = 3, int longPeriodCount = 20, decimal longThreshold = 0.75m, decimal dojiThreshold = 0.1m) : base(inputs, inputMapper, outputMapper)
        {
            var mappedInputs = inputs.Select(inputMapper);
            var ocs = mappedInputs.Select(i => (i.Open, i.Close));

			_upTrend = new UpTrendByTuple(mappedInputs.Select(i => (i.High, i.Low)), upTrendPeriodCount);
			_bullishLongDay = new BullishLongDayByTuple(ocs, longPeriodCount, longThreshold);
			_doji = new DojiByTuple(mappedInputs, dojiThreshold);
			_bearishLongDay = new BearishLongDayByTuple(ocs, longPeriodCount, longThreshold);

			UpTrendPeriodCount = upTrendPeriodCount;
			LongPeriodCount = longPeriodCount;
			LongThreshold = longThreshold;
			DojiThreshold = dojiThreshold;
        }

        public int UpTrendPeriodCount { get; }

        public int LongPeriodCount { get; }

        public decimal LongThreshold { get; }

        public decimal DojiThreshold { get; }

        protected override bool? ComputeByIndexImpl(IEnumerable<(decimal Open, decimal High, decimal Low, decimal Close)> mappedInputs, int index)
        {
			if (index < 2) return null;
			if (!_doji[index - 1]) return false;
            bool isGapped = mappedInputs.ElementAt(index - 1).Low > mappedInputs.ElementAt(index - 2).High && mappedInputs.ElementAt(index - 1).Low > mappedInputs.ElementAt(index).High;
			return (_upTrend[index - 1] ?? false) && _bullishLongDay[index - 2] && isGapped && _bearishLongDay[index];
        }
    }

    public class BearishAbandonedBabyByTuple : BearishAbandonedBaby<(decimal Open, decimal High, decimal Low, decimal Close), bool?>
    {
        public BearishAbandonedBabyByTuple(IEnumerable<(decimal Open, decimal High, decimal Low, decimal Close)> inputs, int upTrendPeriodCount = 3, int longPeriodCount = 20, decimal longThreshold = 0.75M, decimal dojiThreshold = 0.1M) 
            : base(inputs, i => i, (i, otm) => otm, upTrendPeriodCount, longPeriodCount, longThreshold, dojiThreshold)
        {
        }
    }

    public class BearishAbandonedBaby : BearishAbandonedBaby<Candle, AnalyzableTick<bool?>>
    {
        public BearishAbandonedBaby(IEnumerable<Candle> inputs, int upTrendPeriodCount = 3, int longPeriodCount = 20, decimal longThreshold = 0.75M, decimal dojiThreshold = 0.1M) 
            : base(inputs, i => (i.Open, i.High, i.Low, i.Close), (i, otm) => new AnalyzableTick<bool?>(i.DateTime, otm), upTrendPeriodCount, longPeriodCount, longThreshold, dojiThreshold)
        {
        }
    }
}