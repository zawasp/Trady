﻿using System;
using System.Collections.Generic;
using System.Linq;
using Trady.Analysis.Indicator;
using Trady.Analysis.Infrastructure;
using Trady.Analysis.Pattern.State;
using Trady.Core;

namespace Trady.Analysis.Pattern.Indicator
{
    public partial class StochasticsOscillatorTrend
    {
		public class Full<TInput, TOutput> : IndicatorBase<TInput, TOutput>
		{
			public Full(
				IEnumerable<TInput> inputs,
				Func<TInput, (decimal High, decimal Low, decimal Close)> inputMapper,
				Func<TInput, Trend?, TOutput> outputMapper,
				int periodCount,
				int smaPeriodCountK,
                int smaPeriodCountD)
				: base(inputs, inputMapper, outputMapper, new Stochastics.FullByTuple(inputs.Select(inputMapper), periodCount, smaPeriodCountK, smaPeriodCountD))
			{
			}
		}

		public class FullByTuple : Full<(decimal High, decimal Low, decimal Close), Trend?>
		{
			public FullByTuple(IEnumerable<(decimal High, decimal Low, decimal Close)> inputs, int periodCount, int smaPeriodCountK, int smaPeriodCountD)
                : base(inputs, i => i, (i, otm) => otm, periodCount, smaPeriodCountK, smaPeriodCountD)
			{
			}
		}

		public class Full : Full<Candle, AnalyzableTick<Trend?>>
		{
			public Full(IEnumerable<Candle> inputs, int periodCount, int smaPeriodCountK, int smaPeriodCountD)
				: base(inputs, i => (i.High, i.Low, i.Close), (i, otm) => new AnalyzableTick<Trend?>(i.DateTime, otm), periodCount, smaPeriodCountK, smaPeriodCountD)
			{
			}
		}
    }
}