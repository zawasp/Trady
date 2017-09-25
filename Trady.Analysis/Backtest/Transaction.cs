using System;
using System.Collections.Generic;
using Trady.Core;

namespace Trady.Analysis.Backtest
{
    public class Transaction : IEquatable<Transaction>
    {
        public Transaction(IEnumerable<Candle> candles, int index, DateTime dateTime, decimal price, TransactionType type, decimal quantity, decimal absCashFlow)
        {
            Candles = candles;
            Index = index;
            DateTime = dateTime;
            Price = price;
            Type = type;
            Quantity = quantity;
            AbsoluteCashFlow = absCashFlow;
        }

        public IEnumerable<Candle> Candles { get; }

        public DateTime DateTime { get; }

        public int Index { get; }

        public TransactionType Type { get; }

        public decimal Quantity { get; }

        public decimal AbsoluteCashFlow { get; }

        public decimal Price { get; }

        public bool Equals(Transaction other)
            => Candles.Equals(other.Candles) && Index == other.Index && Type == other.Type && Quantity == other.Quantity && AbsoluteCashFlow == other.AbsoluteCashFlow && Price == other.Price;
    }
}