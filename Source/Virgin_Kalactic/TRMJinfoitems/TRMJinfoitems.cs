using System;
using BetterPart;
using MuMech;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace TRMJinfoitems
{
	public class TRInfo : ComputerModule
	{
		
		public TRInfo(MechJebCore core) : base(core) { }
		
		private TrackResource tr;
		
		public enum polarity { Generation, Consumption};
		
		public Dictionary<string, int> frameTrack = new Dictionary<string, int> ();
		
		public bool canUpdate(string resID)
		{
			int output;
			
			if (!frameTrack.TryGetValue(resID, out output))
			{
				frameTrack.Add(resID, 1);
				return true;
			} else {
				frameTrack[resID] = (int)(output + 1 % (1/TimeWarp.fixedDeltaTime));
				if (output == 0) { return true; } else { return false; }
			}
		}
		
		[ValueInfoItem("Electric Charge", InfoItem.Category.Misc)]
		public void ElectricCharge()
		{
			
			if (!HighLogic.LoadedSceneIsFlight) 
			{
				GUILayout.Label("ElectricCharge: N/A");
				return;
			}
			
			if (!canUpdate("ElectricCharge"))
			{
				return;
			}
			
			GUILayout.BeginVertical();
			GUILayout.Label("Electric Charge:");
			GUILayout.Label("Generation : " + fetch ("ElectricCharge", polarity.Generation));
			GUILayout.Label("Consumption: " + fetch ("ElectricCharge", polarity.Consumption));
			GUILayout.EndVertical();
			
			//return fetch ("ElectricCharge", polarity.Generation);
		}
		
		/*[ValueInfoItem("Electric Charge Con", InfoItem.Category.Misc)]
		public double ElectricChargeCon()
		{
			if (!HighLogic.LoadedSceneIsFlight) return 0;
			
			return fetch ("ElectricCharge", polarity.Consumption) * TimeWarp.fixedDeltaTime;
		}
		
		[ValueInfoItem("LiquidFuel Gen", InfoItem.Category.Misc)]
		public double LiquidFuelGen()
		{
			if (!HighLogic.LoadedSceneIsFlight) return 0;
			
			return fetch ("LiquidFuel", polarity.Generation);
		}
		
		[ValueInfoItem("LiquidFuel Con", InfoItem.Category.Misc)]
		public double LiquidFuelCon()
		{
			if (!HighLogic.LoadedSceneIsFlight) return 0;
			
			return fetch ("LiquidFuel", polarity.Consumption);
		}*/
		
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

