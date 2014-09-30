using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using KSP;
using PartReplacement;

namespace TrackResource
{
	
	[KSPAddon (KSPAddon.Startup.Flight, false)]
	public class VesselStatsManager : MonoBehaviour
	{
		private static Dictionary<Vessel, ResourceStats> vesselDict = new Dictionary<Vessel, ResourceStats> ();
		static VesselStatsManager Instance;
		
		public VesselStatsManager ()
		{
			Instance = this;
		}
		
		public void Start ()
		{
			GameEvents.onVesselGoOffRails.Add (Add);
			GameEvents.onVesselWillDestroy.Add (Remove);
			GameEvents.onTimeWarpRateChanged.Add (Check);
		}
		
		public void onDestroy ()
		{
			GameEvents.onVesselGoOffRails.Remove (Add);
			GameEvents.onVesselWillDestroy.Remove (Remove);
			GameEvents.onTimeWarpRateChanged.Remove (Check);
		}
		
		public void Add (Vessel v)
		{
			if (!vesselDict.ContainsKey (v))
			{
				ResourceStats r = VesselStatsManager.Instance.gameObject.AddComponent<ResourceStats> ();
				vesselDict.Add (v, r);
				
				foreach (PartTapIn part in v.Parts)
				{
					part.OnRequestResource.Add(r.Sample);
				}
			}
		}
		
		public void Remove (Vessel v)
		{
			if (vesselDict.ContainsKey (v))
			{
				ResourceStats r = vesselDict [v];
				
				foreach (PartTapIn part in v.parts)
				{
					part.OnRequestResource.Remove(r.Sample);
				}
				
				vesselDict.Remove (v);
				
				
			}
		}
		
		
		private void Check ()
		{
			if (TimeWarp.CurrentRateIndex == 0)
			{
				foreach (KeyValuePair<Vessel, ResourceStats> pair in vesselDict)
				{
					if (!pair.Key.loaded)
					{
						Remove (pair.Key);
						Debug.Log ("Vessel No Longer In Range Upon Leaving Timewarp" + pair.Key.name);
					}
				}
			}
		}
		
		public ResourceStats Get (Vessel v)
		{
			ResourceStats result;
			if (!vesselDict.TryGetValue(v, out result))
			{
				Add (v);
				result = vesselDict [v];
			}
			return result;
		}
	}
	
	public class ResourceStats : MonoBehaviour
	{
		private Dictionary<string, double> consumption = new Dictionary<string, double> ();
		private Dictionary<string, double> generation = new Dictionary<string, double> ();
		private Dictionary<string, double> sumConsumption = new Dictionary<string, double> ();
		private Dictionary<string, double> sumGeneration = new Dictionary<string, double> ();
		
		public void Sample (string resourceName, double demand, ResourceFlowMode FlowMode, double accepted)
		{
			if (demand == 0)
			{
				return;
			}
			if (demand > 0)
			{
				if (!sumConsumption.ContainsKey (resourceName))
				{
					sumConsumption.Add(resourceName, demand);
					return;
				}
				sumConsumption [resourceName] += demand;
			} else {
				if (!sumGeneration.ContainsKey (resourceName))
				{
					sumGeneration.Add(resourceName, demand);
					return;
				}
				sumConsumption [resourceName] += demand;
			}
		}
		
		public double GetConsumption(string resourceName)
		{
			if (consumption.ContainsKey (resourceName))
			{
				return consumption [resourceName];
			} else {
				return 0;
			}
		}
		
		public double GetGeneration (string resourceName)
		{
			if (generation.ContainsKey (resourceName))
			{
				return generation [resourceName];
			} else {
				return 0;
			}
		}
		
		public void LateUpdate ()
		{
			consumption = sumConsumption;
			generation = sumGeneration;
			
			sumConsumption = new Dictionary<string, double> ();
			sumGeneration = new Dictionary<string, double> ();
		}
	}
}

