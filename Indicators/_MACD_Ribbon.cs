
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

namespace NinjaTrader.NinjaScript.Indicators
{
	public class _MACD_Ribbon : Indicator
	{
		private	Series<double>	Macd;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Calculate					= Calculate.OnPriceChange;

				Fast						= 10;
				Slow						= 26;
				Smoothing					= 5;

				UpColor						= Brushes.DodgerBlue;
				DnColor						= Brushes.Red;

				AddPlot(new Stroke(Brushes.Green, 5), PlotStyle.Bar, "MACDRibbon");
			}
			else if (State == State.DataLoaded)
			{
				Macd = new Series<double>(this, MaximumBarsLookBack.Infinite);
			}
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
		{
			Value[0]	= 1;
			Macd[0]		= MACD(Input, Fast, Slow, Smoothing)[0];

			if (IsRising(Macd))
			{
				PlotBrushes[0][0] = UpColor;
			}
			else if (IsFalling(Macd))
			{
				PlotBrushes[0][0] = DnColor;
			}
		}

		#region Properties

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Fast", Order=1, GroupName="MACD")]
		public int Fast
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Slow", Order=2, GroupName="MACD")]
		public int Slow
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Smoothing", Order=3, GroupName="MACD")]
		public int Smoothing
		{ get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Rising Colour", Order=4, GroupName="Colour")]
		public Brush UpColor
		{ get; set; }

		[Browsable(false)]
		public string UpColorSerializable
		{
			get { return Serialize.BrushToString(UpColor); }
			set { UpColor = Serialize.StringToBrush(value); }
		}	

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Falling Colour", Order=5, GroupName="Colour")]
		public Brush DnColor
		{ get; set; }

		[Browsable(false)]
		public string DnColorSerializable
		{
			get { return Serialize.BrushToString(DnColor); }
			set { DnColor = Serialize.StringToBrush(value); }
		}

		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private _MACD_Ribbon[] cache_MACD_Ribbon;
		public _MACD_Ribbon _MACD_Ribbon(int fast, int slow, int smoothing, Brush upColor, Brush dnColor)
		{
			return _MACD_Ribbon(Input, fast, slow, smoothing, upColor, dnColor);
		}

		public _MACD_Ribbon _MACD_Ribbon(ISeries<double> input, int fast, int slow, int smoothing, Brush upColor, Brush dnColor)
		{
			if (cache_MACD_Ribbon != null)
				for (int idx = 0; idx < cache_MACD_Ribbon.Length; idx++)
					if (cache_MACD_Ribbon[idx] != null && cache_MACD_Ribbon[idx].Fast == fast && cache_MACD_Ribbon[idx].Slow == slow && cache_MACD_Ribbon[idx].Smoothing == smoothing && cache_MACD_Ribbon[idx].UpColor == upColor && cache_MACD_Ribbon[idx].DnColor == dnColor && cache_MACD_Ribbon[idx].EqualsInput(input))
						return cache_MACD_Ribbon[idx];
			return CacheIndicator<_MACD_Ribbon>(new _MACD_Ribbon(){ Fast = fast, Slow = slow, Smoothing = smoothing, UpColor = upColor, DnColor = dnColor }, input, ref cache_MACD_Ribbon);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators._MACD_Ribbon _MACD_Ribbon(int fast, int slow, int smoothing, Brush upColor, Brush dnColor)
		{
			return indicator._MACD_Ribbon(Input, fast, slow, smoothing, upColor, dnColor);
		}

		public Indicators._MACD_Ribbon _MACD_Ribbon(ISeries<double> input , int fast, int slow, int smoothing, Brush upColor, Brush dnColor)
		{
			return indicator._MACD_Ribbon(input, fast, slow, smoothing, upColor, dnColor);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators._MACD_Ribbon _MACD_Ribbon(int fast, int slow, int smoothing, Brush upColor, Brush dnColor)
		{
			return indicator._MACD_Ribbon(Input, fast, slow, smoothing, upColor, dnColor);
		}

		public Indicators._MACD_Ribbon _MACD_Ribbon(ISeries<double> input , int fast, int slow, int smoothing, Brush upColor, Brush dnColor)
		{
			return indicator._MACD_Ribbon(input, fast, slow, smoothing, upColor, dnColor);
		}
	}
}

#endregion
