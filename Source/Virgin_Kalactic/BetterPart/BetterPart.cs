using System;
using UnityEngine;
using KSP;
using System.Linq;
using System.Collections.Generic;

namespace BetterPart
{
	
	public class BetterPart : Part
	{
		public override float RequestResource (int resourceID, float demand)
		{
			return (float)RequestResource (resourceID, (double)demand);
		}

		public override float RequestResource (string resourceName, float demand)
		{
			return (float)RequestResource (resourceName, (double)demand);
		}

		public override double RequestResource (int resourceID, double demand)
		{
			return RequestResource (PartResourceLibrary.Instance.GetDefinition (resourceID).name, demand);
		}
		
		public override double RequestResource (string resourceName, double demand)
		{
			TrackResource tr = DictionaryManager.GetTrackResourceForVessel (vessel);
			double accepted = base.RequestResource (resourceName, demand);
			tr.Sample (resourceName, demand, accepted);
			return accepted;
		}
	}
	
	[KSPAddon (KSPAddon.Startup.Flight, false) ]
	public class DictionaryManager : UnityEngine.MonoBehaviour
	{
		private static Dictionary<Vessel, TrackResource> vesselResourceDict = new Dictionary<Vessel, TrackResource> ();
		static DictionaryManager Instance;
		
		public void Start ()
		{
			GameEvents.onVesselGoOffRails.Add (CTRFV);
			GameEvents.onVesselWillDestroy.Add (RVFD);
			GameEvents.onTimeWarpRateChanged.Add (checkLoaded);
			GameEvents.onGameSceneLoadRequested.Add (emptyDictionary);
			Instance = this;
		}
		
		public void onDestroy ()
		{
			GameEvents.onVesselGoOffRails.Remove (CTRFV);
			GameEvents.onVesselWillDestroy.Remove (RVFD);
			GameEvents.onTimeWarpRateChanged.Remove (checkLoaded);
			GameEvents.onGameSceneLoadRequested.Remove (emptyDictionary);
		}
		
		private void CTRFV (Vessel v)
		{
			CreateTrackResForVessel (v);
		}
		
		private static void CreateTrackResForVessel (Vessel v)
		{
			if (!vesselResourceDict.ContainsKey (v))
			{
				TrackResource tr = DictionaryManager.Instance.gameObject.AddComponent<TrackResource> ();
				vesselResourceDict.Add (v, tr);
			}
		}
		
		private void RVFD (Vessel v)
		{
			RemoveVesselFromDict (v);
		}
		
		private static void RemoveVesselFromDict (Vessel v)
		{
			vesselResourceDict.Remove (v);
		}
		
		private void emptyDictionary (GameScenes scene)
		{
			if (scene != GameScenes.FLIGHT)
			{
				vesselResourceDict.Clear();
			}
		}
		
		private void checkLoaded ()
		{
			if (TimeWarp.CurrentRateIndex == 0)
			{
				foreach (KeyValuePair<Vessel, TrackResource> pair in vesselResourceDict)
				{
					if (!pair.Key.loaded)
					{
						RemoveVesselFromDict (pair.Key);
						Debug.Log ("Vessel No Longer In Range Upon Leaving TimeWarp: " + pair.Key.name);
					}
				}
			}
		}
		
		public static TrackResource GetTrackResourceForVessel (Vessel v)
		{
			TrackResource tr;
			if (!vesselResourceDict.TryGetValue (v, out tr))
			{
				CreateTrackResForVessel (v);
				tr = vesselResourceDict [v];
			}
			return tr;
		}
	}
	
	public class TrackResource : MonoBehaviour
	{
		private Dictionary<string, double> consumption = new Dictionary<string, double> ();
		private Dictionary<string, double> generation = new Dictionary<string, double> ();
		private Dictionary<string, double> sumConsumption = new Dictionary<string, double> ();
		private Dictionary<string, double> sumGeneration = new Dictionary<string, double> ();
		
		public void Sample (string resourceName, double demand, double accepted)
		{
			if (demand == 0)
			{
				return;
			}
			if (demand > 0)
			{
				if (!sumConsumption.ContainsKey (resourceName))
				{
					sumConsumption.Add (resourceName, 0);
				}
				sumConsumption [resourceName] += demand;
				
			} else {
				if (!sumGeneration.ContainsKey (resourceName))
				{
					sumGeneration.Add (resourceName, 0);
				}
				sumGeneration [resourceName] += demand * -1;
				
			}
		}
		
		public double GetConsumption (string resourceName)
		{
			if (consumption.ContainsKey (resourceName)) {
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


//	public class NodeToggle : PartModule
//	{
//		public override void OnInitialize ()
//		{
//			if (HighLogic.LoadedSceneIsEditor)
//			{
//				part.attachNodesMaster = attachNode;
//				
//				foreach (AttachNode node in attachNodeMaster)
//				{
//					part.attachNodesStates.add(node.id, true);
//				}
//			}
//		}
//			
//		[KSPEvent(guiActive = true, guiName = "Toggle Node")]
//		public void toggleThing()
//		{
//			
//		}
//	}