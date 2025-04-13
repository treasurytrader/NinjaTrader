
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

//indicator base on "MACD BB Lines" indicator for NT7
//programed for NT8 by DRE Jackrpo 20190101 as retribution
	//added level lines +-.3 
	//added zero line level
//Enjoy it!!!

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class _MACD_BB_Lines : Indicator
	{

		#region Variables
		private int PrevBar = 0;
		#endregion

		/* #region Display Name // 이름이 표시되지 않기 때문에 주석 처리
		public override string DisplayName
			{
			get { return Name ;}
			}
		#endregion */

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Macd BB lines, where the BB are calculated with EMA method, based on NT7 MACD BB LINES";
				Name										= "_MACD_BB_Lines";
				Calculate									= Calculate.OnPriceChange;

				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= false;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				// MaximumBarsLookBack 						= MaximumBarsLookBack.Infinite;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				
				Fast										= 10;
				Slow										= 26;
				Smoothing									= 5;

				StDev										= 1;
				Period										= 10;

				BandFillColor								= Brushes.DarkSlateGray;
				BandFillOpacity								= 50;

				UpColor										= Brushes.DodgerBlue;
				DwColor										= Brushes.Red;

				ShowCross									= true;
				CrossAlert									= Brushes.DarkSlateGray;
				CrossAlert2									= Brushes.DarkSlateGray;
				
				AddLine(Brushes.Gold,   0.3, "ReferenceLine1");
				AddLine(Brushes.Gold,  -0.3, "ReferenceLine2");
				AddLine(Brushes.Maroon,   0, "Zeroline");

				AddPlot(Brushes.Silver, "BBUpper");
				AddPlot(Brushes.Silver, "BBLower");

				AddPlot(new Stroke(Brushes.Green, 2), PlotStyle.Dot, "Macd");
			}
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
		{
			//Add your custom indicator logic here.

			if(CurrentBar < 1) return;

			Draw.Region(this, "Fill_BB", CurrentBar, 0, BBLower, BBUpper, null, BandFillColor, BandFillOpacity,0);

			Macd[0] 			= MACD(Input, Fast, Slow, Smoothing)[0];
			// double Avg 			= EMA(Macd, Period)[0];
			double Avg 			= SMA(Macd, Period)[0];
			double SDev 		= StdDev(Macd, Period)[0];	
			double upperBand	= Avg+(StDev * SDev);
			double lowerBand	= Avg-(StDev * SDev);
			BBUpper[0] 			= upperBand;
			BBLower[0] 			= lowerBand;

			if(IsRising(Macd))
			{
				PlotBrushes[2][0] = UpColor;
			}
			else if(IsFalling(Macd))
			{
				PlotBrushes[2][0] = DwColor;
			}

			if (ShowCross && CurrentBar != PrevBar) 
			{
				if(CrossAbove(MACD(Input, Fast, Slow, Smoothing), 0, 1))
				{
					BackBrush = CrossAlert;
				}
				else if(CrossBelow(MACD(Input, Fast, Slow, Smoothing), 0, 1))
				{
					BackBrush = CrossAlert2;
				}
				PrevBar = CurrentBar;
			}

			// Draw.Line(this, "_macd_bb_lines", false, 0, Macd[0], -20, Macd[0], PlotBrushes[2][0], DashStyleHelper.Solid, 1);
			// Draw.VerticalLine(this, "_macd_bb_lines", -7, PlotBrushes[2][0], DashStyleHelper.Solid, 5, false);
		}

		#region Properties

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Fast", Description="Number of bars for fast EMA", Order=1, GroupName="1. MACD")]
		public int Fast
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Slow", Description="Number of bars for slow EMA", Order=2, GroupName="1. MACD")]
		public int Slow
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Smoothing", Description="Number of bars for smoothing", Order=3, GroupName="1. MACD")]
		public int Smoothing
		{ get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Rising Colour", Description="Default Colour for Rising MACD", Order=8, GroupName="1. MACD")]
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
		[Display(Name="Falling Colour", Description="Default Colour for Falling MACD", Order=9, GroupName="1. MACD")]
		public Brush DwColor
		{ get; set; }

		[Browsable(false)]
		public string DwColorSerializable
		{
			get { return Serialize.BrushToString(DwColor); }
			set { DwColor = Serialize.StringToBrush(value); }
		}

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="StDev Factor", Description="Standard Deviation Factor", Order=1, GroupName="2. BB")]
		public int StDev
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="StDev Period", Description="Period for StDev", Order=2, GroupName="2. BB")]
		public int Period
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Fill Opacity", Description="Fill Color Opacity.", Order=4, GroupName="2. BB")]
		public int BandFillOpacity
		{ get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Band Fill Color", Description="Default Colour to Fill Bollinger Bands", Order=3, GroupName="2. BB")]
		public Brush BandFillColor
		{ get; set; }

		[Browsable(false)]
		public string BandFillColorSerializable
		{
			get { return Serialize.BrushToString(BandFillColor); }
			set { BandFillColor = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[Display(Name="ShowCross", Description="Show Zero Line crosses", Order=1, GroupName="3. Cross Alert")]
		public bool ShowCross
		{ get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Cross Alert Up", Description="Cross 0 line alert Colour Up", Order=2, GroupName="3. Cross Alert")]
		public Brush CrossAlert
		{ get; set; }

		[Browsable(false)]
		public string CrossAlertSerializable
		{
			get { return Serialize.BrushToString(CrossAlert); }
			set { CrossAlert = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Cross Alert Down", Description="Cross 0 line alert Colour Down", Order=3, GroupName="3. Cross Alert")]
		public Brush CrossAlert2
		{ get; set; }

		[Browsable(false)]
		public string CrossAlert2Serializable
		{
			get { return Serialize.BrushToString(CrossAlert2); }
			set { CrossAlert2 = Serialize.StringToBrush(value); }
		}	

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Macd
		{
			get { return Values[2]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> BBUpper
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> BBLower
		{
			get { return Values[0]; }
		}

		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private _MACD_BB_Lines[] cache_MACD_BB_Lines;
		public _MACD_BB_Lines _MACD_BB_Lines(int fast, int slow, int smoothing, Brush upColor, Brush dwColor, int stDev, int period, int bandFillOpacity, Brush bandFillColor, bool showCross, Brush crossAlert, Brush crossAlert2)
		{
			return _MACD_BB_Lines(Input, fast, slow, smoothing, upColor, dwColor, stDev, period, bandFillOpacity, bandFillColor, showCross, crossAlert, crossAlert2);
		}

		public _MACD_BB_Lines _MACD_BB_Lines(ISeries<double> input, int fast, int slow, int smoothing, Brush upColor, Brush dwColor, int stDev, int period, int bandFillOpacity, Brush bandFillColor, bool showCross, Brush crossAlert, Brush crossAlert2)
		{
			if (cache_MACD_BB_Lines != null)
				for (int idx = 0; idx < cache_MACD_BB_Lines.Length; idx++)
					if (cache_MACD_BB_Lines[idx] != null && cache_MACD_BB_Lines[idx].Fast == fast && cache_MACD_BB_Lines[idx].Slow == slow && cache_MACD_BB_Lines[idx].Smoothing == smoothing && cache_MACD_BB_Lines[idx].UpColor == upColor && cache_MACD_BB_Lines[idx].DwColor == dwColor && cache_MACD_BB_Lines[idx].StDev == stDev && cache_MACD_BB_Lines[idx].Period == period && cache_MACD_BB_Lines[idx].BandFillOpacity == bandFillOpacity && cache_MACD_BB_Lines[idx].BandFillColor == bandFillColor && cache_MACD_BB_Lines[idx].ShowCross == showCross && cache_MACD_BB_Lines[idx].CrossAlert == crossAlert && cache_MACD_BB_Lines[idx].CrossAlert2 == crossAlert2 && cache_MACD_BB_Lines[idx].EqualsInput(input))
						return cache_MACD_BB_Lines[idx];
			return CacheIndicator<_MACD_BB_Lines>(new _MACD_BB_Lines(){ Fast = fast, Slow = slow, Smoothing = smoothing, UpColor = upColor, DwColor = dwColor, StDev = stDev, Period = period, BandFillOpacity = bandFillOpacity, BandFillColor = bandFillColor, ShowCross = showCross, CrossAlert = crossAlert, CrossAlert2 = crossAlert2 }, input, ref cache_MACD_BB_Lines);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators._MACD_BB_Lines _MACD_BB_Lines(int fast, int slow, int smoothing, Brush upColor, Brush dwColor, int stDev, int period, int bandFillOpacity, Brush bandFillColor, bool showCross, Brush crossAlert, Brush crossAlert2)
		{
			return indicator._MACD_BB_Lines(Input, fast, slow, smoothing, upColor, dwColor, stDev, period, bandFillOpacity, bandFillColor, showCross, crossAlert, crossAlert2);
		}

		public Indicators._MACD_BB_Lines _MACD_BB_Lines(ISeries<double> input , int fast, int slow, int smoothing, Brush upColor, Brush dwColor, int stDev, int period, int bandFillOpacity, Brush bandFillColor, bool showCross, Brush crossAlert, Brush crossAlert2)
		{
			return indicator._MACD_BB_Lines(input, fast, slow, smoothing, upColor, dwColor, stDev, period, bandFillOpacity, bandFillColor, showCross, crossAlert, crossAlert2);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators._MACD_BB_Lines _MACD_BB_Lines(int fast, int slow, int smoothing, Brush upColor, Brush dwColor, int stDev, int period, int bandFillOpacity, Brush bandFillColor, bool showCross, Brush crossAlert, Brush crossAlert2)
		{
			return indicator._MACD_BB_Lines(Input, fast, slow, smoothing, upColor, dwColor, stDev, period, bandFillOpacity, bandFillColor, showCross, crossAlert, crossAlert2);
		}

		public Indicators._MACD_BB_Lines _MACD_BB_Lines(ISeries<double> input , int fast, int slow, int smoothing, Brush upColor, Brush dwColor, int stDev, int period, int bandFillOpacity, Brush bandFillColor, bool showCross, Brush crossAlert, Brush crossAlert2)
		{
			return indicator._MACD_BB_Lines(input, fast, slow, smoothing, upColor, dwColor, stDev, period, bandFillOpacity, bandFillColor, showCross, crossAlert, crossAlert2);
		}
	}
}

#endregion
