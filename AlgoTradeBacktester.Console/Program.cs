using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AlgoTradeBacktester.Core.Interfaces;
using AlgoTradeBacktester.Core.Constants;
using AlgoTradeBacktester.Services;
using AlgoTradeBacktester.Core.Models;
using AlgoTradeBacktester.Services.Strategies;

class Program
{
    static async Task Main(string[] args)
    {
        // Setup DI
        var services = new ServiceCollection();
        ConfigureServices(services);

        var serviceProvider = services.BuildServiceProvider();
        var exchangeService = serviceProvider.GetRequiredService<IExchangeService>();
        var strategyService = serviceProvider.GetRequiredService<IStrategyService>();

        try
        {
            // Backtest config
            var symbol = TradingConstants.BTCUSDT;
            var interval = "4h";
            var endTime = DateTime.UtcNow;
            var startTime = endTime.AddDays(-240);

            // Get candle data
            Console.WriteLine($"Fetching historical data for {symbol}...");
            var candles = await exchangeService.GetHistoricalDataAsync(
                symbol,
                interval,
                startTime,
                endTime);

            if (!candles.Any())
            {
                Console.WriteLine("No data received. Exiting...");
                return;
            }

            // Process and print results
            PrintBacktestInfo(candles);
            var backtestResult = strategyService.Backtest(candles);
            PrintBacktestResults(backtestResult);
            PrintTrades(backtestResult.Positions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging(configure => configure.AddConsole());
        services.AddTransient<IExchangeService, ExchangeService>();
        services.AddTransient<IRiskManager, RiskManager>();
        services.AddTransient<IStrategyService, SupertrendStrategy>();
    }

    private static void PrintBacktestInfo(List<Candle> candles)
    {
        Console.WriteLine($"Running backtest on {candles.Count} candles...");
        Console.WriteLine("First candle open time: " + candles.First().OpenTime);
        Console.WriteLine("Last candle open time: " + candles.Last().OpenTime);
    }

    private static void PrintBacktestResults(BacktestResult result)
    {
        Console.WriteLine("\n=== Backtest Results ===");
        Console.WriteLine($"Initial Balance: {result.InitialBalance:F2} USDT");
        Console.WriteLine($"Final Balance: {result.FinalBalance:F2} USDT");
        Console.WriteLine($"Total PnL: {result.TotalPnL:F2} USDT ({result.TotalPnL / result.InitialBalance:P2})");
        Console.WriteLine($"Total Fees: {result.TotalFees:F2} USDT");
        Console.WriteLine($"Total Profit: {result.TotalProfit:F2} USDT");

        Console.WriteLine($"\nTotal Trades: {result.Positions.Count}");
        Console.WriteLine($"Winning Trades: {result.WinningTrades}");
        Console.WriteLine($"Losing Trades: {result.LosingTrades}");
        Console.WriteLine($"Win Rate: {result.WinRate:P2}");
    }

    private static void PrintTrades(List<Position> positions)
    {
        Console.WriteLine("\n=== Detailed Trade History ===");
        Console.WriteLine("╔══════════════════╦══════════════════╦═══════╦══════════════╦══════════════╦══════════════╦══════════════╦══════════════╦═══════════╦═══════╗");
        Console.WriteLine("║    Entry Time    ║    Exit Time     ║ Type  ║ Entry Price  ║  Exit Price  ║Position Size ║ Init Balance ║Final Balance ║    PnL    ║ Stop  ║");
        Console.WriteLine("╠══════════════════╬══════════════════╬═══════╬══════════════╬══════════════╬══════════════╬══════════════╬══════════════╬═══════════╬═══════╣");

        foreach (var p in positions)
        {
            Console.WriteLine(
                $"║ {p.EntryTime:yyyy-MM-dd HH:mm} ║ {p.ExitTime:yyyy-MM-dd HH:mm} ║" +
                $" {(p.IsLong ? "LONG " : "SHORT")} ║" +
                $" {p.EntryPrice,12:F2} ║ {p.ExitPrice,12:F2} ║" +
                $" {p.Size,12:F2} ║" +
                $" {p.InitialBalance,12:F2} ║" +
                $" {p.FinalBalance,12:F2} ║" +
                $" {p.PnL.GetValueOrDefault() * p.Size,9:F2} ║" +
                $"   {(p.IsStopLoss ? "X" : " ")}   ║");
        }

        Console.WriteLine("╚══════════════════╩══════════════════╩═══════╩══════════════╩══════════════╩══════════════╩══════════════╩══════════════╩═══════════╩═══════╝");
    }
}
