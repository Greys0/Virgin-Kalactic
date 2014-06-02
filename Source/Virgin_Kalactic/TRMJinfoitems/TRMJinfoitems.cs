using System;
using BetterPart;
using MuMech;

namespace TRMJinfoitems
{
	public class TRInfo
	{
		
		private TrackResource tr;
		
		public enum polarity { Generation, Consumption};
		
		[ValueInfoItem("Electric Charge Generation", InfoItem.Category.Misc)]
		public double ElectricChargeGen()
		{
			if (!HighLogic.LoadedSceneIsFlight) return 0;
			
			return 0;
		}
		
		[ValueInfoItem("Electric Charge Consumption", InfoItem.Category.Misc)]
		public double ElectricChargeCon()
		{
			if (!HighLogic.LoadedSceneIsFlight) return 0;
			
			return 0;
		}
		
		public double fetch(string reqType, polarity reqGet)
		{
			
			tr = DictionaryManager.GetTrackResourceForVessel(FlightGlobals.ActiveVessel);
			switch (reqGet)
			{
				case polarity.Generation: return tr.GetGeneration(reqType);
				case polarity.Consumption: return tr.GetConsumption(reqType);
				default: return 0;
			}
		}
	}
}

