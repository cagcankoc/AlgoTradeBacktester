using AlgoTradeBacktester.Core.Interfaces;
using AlgoTradeBacktester.Core.Models;
using AlgoTradeBacktester.Core.Constants;
using Microsoft.Extensions.Logging;

namespace AlgoTradeBacktester.Services.Strategies
{
    public class SupertrendStrategy : IStrategyService
    {
        private readonly IRiskManager _riskManager;
        private readonly ILogger<SupertrendStrategy> _logger;
        private decimal _currentBalance;
        private const int ATR_PERIOD = 10;
        private const decimal ATR_MULTIPLIER = 3.0m;

        public SupertrendStrategy(IRiskManager riskManager, ILogger<SupertrendStrategy> logger)
        {
            _riskManager = riskManager;
            _logger = logger;
        }

        public BacktestResult Backtest(List<Candle> candles)
        {
            var result = new BacktestResult { InitialBalance = TradingConstants.INITIAL_BALANCE };
            Position? currentPosition = null;
            _currentBalance = TradingConstants.INITIAL_BALANCE;

            if (candles.Count < ATR_PERIOD + 1) return result;

            var supertrendValues = CalculateSupertrend(candles);
            bool? prevTrend = null; // true -> uptrend, false -> downtrend

            for (int i = ATR_PERIOD; i < candles.Count; i++)
            {
                var candle = candles[i];
                var (supertrendUp, supertrendDown) = supertrendValues[i];

                // Determine trend direction
                bool? currentTrend = null;
                if (candle.Close > supertrendUp) currentTrend = true;
                else if (candle.Close < supertrendDown) currentTrend = false;
                else currentTrend = prevTrend;

                if (currentPosition != null)
                {
                    // Exit long position - only when trend turns down
                    if (currentPosition.IsLong && currentTrend == false)
                    {
                        ClosePosition(currentPosition, candle.OpenTime, candle.Close, false, result);
                        currentPosition = null;
                    }
                    // Exit short position - only when trend turns up
                    else if (!currentPosition.IsLong && currentTrend == true)
                    {
                        ClosePosition(currentPosition, candle.OpenTime, candle.Close, false, result);
                        currentPosition = null;
                    }
                    // Stop loss check
                    else if (_riskManager.ShouldClosePosition(currentPosition, candle, out decimal exitPrice))
                    {
                        ClosePosition(currentPosition, candle.OpenTime, exitPrice, true, result);
                        currentPosition = null;
                    }
                }
                else if (prevTrend != currentTrend && currentTrend.HasValue) // Trend change and no position
                {
                    // Long entry signal - when trend turns up
                    if (currentTrend.Value)
                    {
                        var stopLoss = Math.Min(candle.Low, supertrendDown);
                        var size = _riskManager.GetPositionSize(_currentBalance);

                        currentPosition = new Position
                        {
                            EntryTime = candle.OpenTime,
                            EntryPrice = candle.Close,
                            IsLong = true,
                            Size = size,
                            StopLossPrice = stopLoss,
                            InitialBalance = _currentBalance
                        };

                        result.Positions.Add(currentPosition);
                        //_logger.LogInformation($"Opened LONG position at {candle.Close:F2}");
                    }
                    // Short entry signal - when trend turns down
                    else
                    {
                        var stopLoss = Math.Max(candle.High, supertrendUp);
                        var size = _riskManager.GetPositionSize(_currentBalance);

                        currentPosition = new Position
                        {
                            EntryTime = candle.OpenTime,
                            EntryPrice = candle.Close,
                            IsLong = false,
                            Size = size,
                            StopLossPrice = stopLoss,
                            InitialBalance = _currentBalance
                        };

                        result.Positions.Add(currentPosition);
                        //_logger.LogInformation($"Opened SHORT position at {candle.Close:F2}");
                    }
                }

                prevTrend = currentTrend;
            }

            // Close last position
            if (currentPosition != null)
            {
                var lastCandle = candles[^1];
                ClosePosition(currentPosition, lastCandle.OpenTime, lastCandle.Close, false, result);
            }

            return result;
        }

        private void ClosePosition(Position position, DateTime exitTime, decimal exitPrice, bool isStopLoss, BacktestResult result)
        {
            position.ExitTime = exitTime;
            position.ExitPrice = exitPrice;
            position.IsStopLoss = isStopLoss;

            // Calculate trading fees
            decimal entryFee = position.Size * TradingConstants.TRADING_FEE;
            decimal exitFee = position.Size * TradingConstants.TRADING_FEE;
            decimal totalFees = entryFee + exitFee;
            position.Fee = totalFees;

            // Calculate PnL
            decimal tradePnL;
            if (position.IsLong)
            {
                tradePnL = position.Size * ((exitPrice - position.EntryPrice) / position.EntryPrice);
            }
            else
            {
                tradePnL = position.Size * ((position.EntryPrice - exitPrice) / position.EntryPrice);
            }

            // Net PnL (after fees)
            decimal netPnL = tradePnL - totalFees;

            // PnL percentage
            position.PnL = netPnL / position.Size;

            // Update results
            result.TotalPnL += tradePnL;
            result.TotalFees += totalFees;
            _currentBalance += netPnL;
            position.FinalBalance = _currentBalance;

            if (netPnL > 0)
                result.WinningTrades++;
            else
                result.LosingTrades++;

            //_logger.LogInformation(
            //    $"Closed {(position.IsLong ? "LONG" : "SHORT")} position: Entry={position.EntryPrice:F2}, " +
            //    $"Exit={exitPrice:F2}, PnL={netPnL:F2} ({position.PnL:P2}), Stop={isStopLoss}");
        }

        private List<(decimal up, decimal down)> CalculateSupertrend(List<Candle> candles)
        {
            var result = new List<(decimal up, decimal down)>();
            var atr = CalculateATR(candles);

            decimal prevFinalUpperBand = 0;
            decimal prevFinalLowerBand = 0;

            // Dummy values for first ATR_PERIOD candles
            for (int i = 0; i < ATR_PERIOD; i++)
            {
                result.Add((candles[i].High, candles[i].Low));
            }

            for (int i = ATR_PERIOD; i < candles.Count; i++)
            {
                var candle = candles[i];
                var basicUpperBand = (candle.High + candle.Low) / 2 + ATR_MULTIPLIER * atr[i];
                var basicLowerBand = (candle.High + candle.Low) / 2 - ATR_MULTIPLIER * atr[i];

                decimal finalUpperBand = basicUpperBand;
                decimal finalLowerBand = basicLowerBand;

                if (basicUpperBand < prevFinalUpperBand || candles[i - 1].Close > prevFinalUpperBand)
                    finalUpperBand = basicUpperBand;
                else
                    finalUpperBand = prevFinalUpperBand;

                if (basicLowerBand > prevFinalLowerBand || candles[i - 1].Close < prevFinalLowerBand)
                    finalLowerBand = basicLowerBand;
                else
                    finalLowerBand = prevFinalLowerBand;

                result.Add((finalUpperBand, finalLowerBand));
                prevFinalUpperBand = finalUpperBand;
                prevFinalLowerBand = finalLowerBand;
            }

            return result;
        }

        private List<decimal> CalculateATR(List<Candle> candles)
        {
            var trueRanges = new List<decimal>();
            var atrValues = new List<decimal>();

            // Calculate TR for first candle
            trueRanges.Add(candles[0].High - candles[0].Low);
            atrValues.Add(trueRanges[0]);

            // Calculate TR for other candles
            for (int i = 1; i < candles.Count; i++)
            {
                var tr = Math.Max(
                    Math.Max(
                        candles[i].High - candles[i].Low,
                        Math.Abs(candles[i].High - candles[i - 1].Close)
                    ),
                    Math.Abs(candles[i].Low - candles[i - 1].Close)
                );
                trueRanges.Add(tr);

                // Calculate ATR
                if (i < ATR_PERIOD)
                {
                    // Simple average for first ATR_PERIOD candles
                    atrValues.Add(trueRanges.Take(i + 1).Average());
                }
                else
                {
                    // Wilder's Smoothing for subsequent candles
                    var atr = ((ATR_PERIOD - 1) * atrValues[i - 1] + trueRanges[i]) / ATR_PERIOD;
                    atrValues.Add(atr);
                }
            }

            return atrValues;
        }
    }
}
