﻿using System;
using System.Collections.Generic;
using System.Linq;
using Trady.Analysis.Helper;
using Trady.Analysis.Infrastructure;
using Trady.Analysis.Pattern.State;
using Trady.Core;

namespace Trady.Analysis.Pattern.Candlestick
{
    /// <summary>
    /// Reference: http://stockcharts.com/school/doku.php?id=chart_school:chart_analysis:candlestick_pattern_dictionary
    /// </summary>
    public class BullishShortDay<TInput, TOutput> : AnalyzableBase<TInput, (decimal Open, decimal Close), bool, TOutput>
    {
        BullishByTuple _bullish;
        ShortDayByTuple _shortDay;

        public BullishShortDay(IEnumerable<TInput> inputs, Func<TInput, (decimal Open, decimal Close)> inputMapper, Func<TInput, bool, TOutput> outputMapper, int periodCount = 20, decimal threshold = 0.25m) : base(inputs, inputMapper, outputMapper)
        {
            var mappedInputs = inputs.Select(inputMapper);
			_bullish = new BullishByTuple(mappedInputs);
			_shortDay = new ShortDayByTuple(mappedInputs);

			PeriodCount = periodCount;
			Threshold = threshold;
        }

        public int PeriodCount { get; }

        public decimal Threshold { get; }

        protected override bool ComputeByIndexImpl(IEnumerable<(decimal Open, decimal Close)> mappedInputs, int index)
			=> _bullish[index] && _shortDay[index];
	}

    public class BullishShortDayByTuple : BullishShortDay<(decimal Open, decimal Close), bool>
    {
        public BullishShortDayByTuple(IEnumerable<(decimal Open, decimal Close)> inputs, int periodCount = 20, decimal threshold = 0.25M) 
            : base(inputs, i => i, (i, otm) => otm, periodCount, threshold)
        {
        }
    }

    public class BullishShortDay : BullishShortDay<Candle, AnalyzableTick<bool>>
    {
        public BullishShortDay(IEnumerable<Candle> inputs, int periodCount = 20, decimal threshold = 0.25M) 
            : base(inputs, i => (i.Open, i.Close), (i, otm) => new AnalyzableTick<bool>(i.DateTime, otm), periodCount, threshold)
        {
        }
    }
}