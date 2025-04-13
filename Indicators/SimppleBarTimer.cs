#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

// This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
    public class SimpleBarTimer : Indicator
    {
        private System.Timers.Timer updateTimer;
        private DateTime candleStartTime;
        private TimeSpan candleDuration;
        private TimeSpan remainingTime;
        private bool hasAlerted = false; // Track if the alert has already played

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Displays a real-time countdown timer on the bottom-left side of the chart for multiple timeframes.";
                Name = "SimpleBarTimer";
                Calculate = Calculate.OnEachTick; // Update on each tick for real-time
                IsOverlay = true; // Overlay the timer on the chart
                DisplayInDataBox = false; // Don’t display in the data box
                PaintPriceMarkers = false; // Don’t paint price markers
                DrawHorizontalGridLines = false; // Optional: Disable grid lines
                DrawVerticalGridLines = false; // Optional: Disable grid lines
                ScaleJustification = NinjaTrader.Gui.Chart.ScaleJustification.Right;
                IsSuspendedWhileInactive = true;
            }
            else if (State == State.Historical)
            {
                ClearOutputWindow();
            }
            else if (State == State.Realtime)
            {
                // Initialize the system timer to update every second
                updateTimer = new System.Timers.Timer(1000); // Update every 1 second
                updateTimer.Elapsed += (sender, e) => UpdateTimer();
                updateTimer.AutoReset = true;
                updateTimer.Start();
            }
            else if (State == State.Terminated)
            {
                // Clean up the timer
                if (updateTimer != null)
                {
                    updateTimer.Stop();
                    updateTimer.Dispose();
                }
            }
        }

        protected override void OnBarUpdate()
        {
            try
            {
                // Ensure we're working with a supported timeframe (1m, 5m, 15m, 1h, 4h, 1d)
                if (BarsPeriod.BarsPeriodType != BarsPeriodType.Minute && BarsPeriod.BarsPeriodType != BarsPeriodType.Day)
                {
                    Draw.TextFixed(this, "ErrorMessage", "This indicator works only on 1m, 5m, 15m, 1h, 4h, or 1d charts.",
                        TextPosition.TopLeft, Brushes.Red, new SimpleFont("Arial", 12), Brushes.Transparent, Brushes.Transparent, 100);
                    return;
                }

                if (BarsPeriod.BarsPeriodType == BarsPeriodType.Minute)
                {
                    if (BarsPeriod.Value != 1 && BarsPeriod.Value != 5 && BarsPeriod.Value != 15 && BarsPeriod.Value != 60 && BarsPeriod.Value != 240)
                    {
                        Draw.TextFixed(this, "ErrorMessage", "This indicator works only on 1m, 5m, 15m, 1h, 4h, or 1d charts.",
                            TextPosition.TopLeft, Brushes.Red, new SimpleFont("Arial", 12), Brushes.Transparent, Brushes.Transparent, 100);
                        return;
                    }
                    candleDuration = TimeSpan.FromMinutes(BarsPeriod.Value);
                }
                else if (BarsPeriod.BarsPeriodType == BarsPeriodType.Day)
                {
                    if (BarsPeriod.Value != 1)
                    {
                        Draw.TextFixed(this, "ErrorMessage", "This indicator works only on 1m, 5m, 15m, 1h, 4h, or 1d charts.",
                            TextPosition.TopLeft, Brushes.Red, new SimpleFont("Arial", 12), Brushes.Transparent, Brushes.Transparent, 100);
                        return;
                    }
                    candleDuration = TimeSpan.FromDays(1);
                }

                // Update the timer
                UpdateTimer();
            }
            catch (Exception ex)
            {
                Print($"Error in OnBarUpdate: {ex.Message}");
            }
        }

        protected override void OnMarketData(MarketDataEventArgs marketDataUpdate)
        {
            try
            {
                // Update the timer on every tick in real-time
                if (State == State.Realtime)
                {
                    UpdateTimer();
                }
            }
            catch (Exception ex)
            {
                Print($"Error in OnMarketData: {ex.Message}");
            }
        }

        private void UpdateTimer()
        {
            try
            {
                // Use the system clock to determine the current time
                DateTime currentTime = DateTime.Now;

                // Align the candle start time with the system clock based on the timeframe
                if (BarsPeriod.BarsPeriodType == BarsPeriodType.Minute)
                {
                    int minutes = (currentTime.Minute / BarsPeriod.Value) * BarsPeriod.Value;
                    int hours = currentTime.Hour;
                    if (BarsPeriod.Value == 60) // 1h
                    {
                        hours = currentTime.Hour;
                        minutes = 0;
                    }
                    else if (BarsPeriod.Value == 240) // 4h
                    {
                        hours = (currentTime.Hour / 4) * 4;
                        minutes = 0;
                    }
                    candleStartTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day,
                        hours, minutes, 0);
                }
                else if (BarsPeriod.BarsPeriodType == BarsPeriodType.Day)
                {
                    candleStartTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, 0, 0, 0);
                }

                // Calculate the elapsed time since the candle started
                TimeSpan elapsedTime = currentTime - candleStartTime;
                remainingTime = candleDuration - elapsedTime;

                // If the remaining time is negative, the candle has ended
                if (remainingTime <= TimeSpan.Zero)
                {
                    // Reset the start time to the next candle
                    if (BarsPeriod.BarsPeriodType == BarsPeriodType.Minute)
                    {
                        candleStartTime = candleStartTime.AddMinutes(BarsPeriod.Value);
                    }
                    else if (BarsPeriod.BarsPeriodType == BarsPeriodType.Day)
                    {
                        candleStartTime = candleStartTime.AddDays(1);
                    }
                    elapsedTime = currentTime - candleStartTime;
                    remainingTime = candleDuration - elapsedTime;
                    hasAlerted = false; // Reset the alert for the new candle
                }

                // Format the remaining time based on the timeframe
                string timeDisplay;
                if (BarsPeriod.BarsPeriodType == BarsPeriodType.Day)
                {
                    timeDisplay = string.Format("{0:hh\\:mm\\:ss}", remainingTime);
                }
                else
                {
                    timeDisplay = string.Format("{0:mm\\:ss}", remainingTime);
                }

                // Determine the color based on remaining time (alert when less than 10 seconds)
                Brush timerColor = remainingTime.TotalSeconds <= 10 ? Brushes.Red : Brushes.White;

                // Play a sound alert when 10 seconds or less remain (only once per candle)
                if (remainingTime.TotalSeconds <= 10 && !hasAlerted)
                {
                    PlaySound(@"C:\Program Files (x86)\NinjaTrader 8\sounds\Alert1.wav");
                    hasAlerted = true;
                }

                // Draw the timer on the bottom-left side of the chart (fixed position)
                Draw.TextFixed(this, "Timer", timeDisplay, TextPosition.BottomLeft, timerColor,
                    new SimpleFont("Arial", 48), Brushes.Transparent, Brushes.Transparent, 100);

                // Force the chart to refresh
                if (State == State.Realtime && ChartControl != null)
                {
                    ChartControl.InvalidateVisual();
                }
            }
            catch (Exception ex)
            {
                Print($"Error in UpdateTimer: {ex.Message}");
            }
        }
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private SimpleBarTimer[] cacheSimpleBarTimer;
		public SimpleBarTimer SimpleBarTimer()
		{
			return SimpleBarTimer(Input);
		}

		public SimpleBarTimer SimpleBarTimer(ISeries<double> input)
		{
			if (cacheSimpleBarTimer != null)
				for (int idx = 0; idx < cacheSimpleBarTimer.Length; idx++)
					if (cacheSimpleBarTimer[idx] != null &&  cacheSimpleBarTimer[idx].EqualsInput(input))
						return cacheSimpleBarTimer[idx];
			return CacheIndicator<SimpleBarTimer>(new SimpleBarTimer(), input, ref cacheSimpleBarTimer);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.SimpleBarTimer SimpleBarTimer()
		{
			return indicator.SimpleBarTimer(Input);
		}

		public Indicators.SimpleBarTimer SimpleBarTimer(ISeries<double> input )
		{
			return indicator.SimpleBarTimer(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.SimpleBarTimer SimpleBarTimer()
		{
			return indicator.SimpleBarTimer(Input);
		}

		public Indicators.SimpleBarTimer SimpleBarTimer(ISeries<double> input )
		{
			return indicator.SimpleBarTimer(input);
		}
	}
}

#endregion
