using System;
using UnityEngine;
using KSP;
using System.Linq;
using System.Collections.Generic;

namespace Virgin_Kalactic
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
			GameEvents.onVesselGoOnRails.Add (RVFD);
			GameEvents.onVesselWillDestroy.Add (RVFD);
			Instance = this;
		}
		
		public void onDestroy ()
		{
			GameEvents.onVesselGoOffRails.Remove (CTRFV);
			GameEvents.onVesselGoOnRails.Remove (RVFD);
			GameEvents.onVesselWillDestroy.Remove (RVFD);
		}
		
		private void CTRFV (Vessel v)
		{
			CreateTrackResForVessel (v);
		}
		
		private static void CreateTrackResForVessel (Vessel v)
		{
			if (!vesselResourceDict.ContainsKey (v)) {
				TrackResource tr = DictionaryManager.Instance.gameObject.AddComponent<TrackResource> ();     
				vesselResourceDict.Add (v, tr);
			} else {
				Debug.Log ("CreateTrackResForVessel: TrackRes Already Exists for Vessel" + v.GetName ());
				//Debug.Log (System.Environment.StackTrace);
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
		
		public static TrackResource GetTrackResourceForVessel (Vessel v)
		{
			TrackResource tr;
			if (!vesselResourceDict.TryGetValue (v, out tr)) {
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
			if (demand == 0) {
				return;
			}
			if (demand > 0) {
				if (!sumConsumption.ContainsKey (resourceName)) {
					sumConsumption.Add (resourceName, 0);
				}
				sumConsumption [resourceName] += demand;
				
			} else {
				if (!sumGeneration.ContainsKey (resourceName)) {
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
			if (generation.ContainsKey (resourceName)) {
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
	
	public class AdvGenerator: PartModule
	{
		//[KSPField(guiActive = true, guiName = "Responding to")]

		public List<Resource> inputs = new List<Resource> ();
		public List<Resource> outputs = new List<Resource> ();
		public ConfigNode node = null;
		
		[KSPField]
		public int numSamples = 20;
		private int curSample = 0;
		
		[KSPField(guiActive = true, guiName = "Demand")]
		public double demand;
		private double curIn;
		private double curOut;
		[KSPField(guiActive = true, guiName = "Throttle")]
		public double throttle = 0; // instantiation may not be necessary, setting to 0 until made use of
		[KSPField]
		public double minThrottle = 0.001;
		
		[KSPField(guiActive = true, guiName = "Status")]
		public string status = "Inactive";
		[KSPField(isPersistant = true)]
		public bool activen = false; //Todo: name this something less stupid
		
		private TrackResource tr;
		public Resource primary;
		
		
		
		// loading
		public class Resource
		{
			private PartResourceDefinition _resource = new PartResourceDefinition ();

			public PartResourceDefinition resource {
				get { return this._resource; }
			}
			
			private double _maxRate = 1;

			public double maxRate {
				get { return this._maxRate; }
			}
			
			private FloatCurve _rateCurve = new FloatCurve ();
			
			public FloatCurve rateCurve {
				get { return this._rateCurve; }
			}
			
			private FloatCurve _revRateCurve = new FloatCurve ();
			
			public FloatCurve revRateCurve {
				get { return this._revRateCurve; }
			}
			
			public double[] samples;
			public double minThrottle;
			public string type;
			
			public Resource (ConfigNode node)
			{
				//Debug.Log ("VKLoading Resource");
				if (node.HasValue ("resourceName") && PartResourceLibrary.Instance.resourceDefinitions.Any (d => d.name == node.GetValue ("resourceName"))) {
					//Debug.Log ("VKResource Found");
					this._resource = PartResourceLibrary.Instance.GetDefinition (node.GetValue ("resourceName"));
					if (node.HasValue ("maxRate"))
					{
						double.TryParse (node.GetValue ("maxRate"), out _maxRate);
					}
					if (node.HasNode ("rateCurve"))
					{
						_rateCurve.Load (node.GetNode ("rateCurve"));
					}
					if (node.HasNode ("revRateCurve"))
					{
						_revRateCurve.Load (node.GetNode ("revRateCurve"));
					}
					if (node.HasValue ("type")) {
						type = node.GetValue ("type");
					}
				}
			}
		}
		
		private void LoadResources ()
		{
			if (node.HasNode ("INPUT") && node.HasNode ("OUTPUT")) {
				Debug.Log ("Loading Input and Output");
				inputs.AddRange (this.node.GetNodes ("INPUT").Select (n => new Resource (n)));
				outputs.AddRange (this.node.GetNodes ("OUTPUT").Select (n => new Resource (n)));
				if (inputs.Count > 0 && outputs.Count > 0)
				{
					//Debug.Log ("Input and Output loaded successfully");
					return;
				}
			}
			Debug.Log ("Invalid resources");
			isEnabled = false;
			activen = false;
			
		}
		
		
		// events
		public override void OnStart (PartModule.StartState state)
		{
			//Debug.Log ("VKStart");
			LoadResources ();
			foreach (Resource item in outputs) {
				item.samples = new double[numSamples];
			}
		}
		
		public override void OnLoad (ConfigNode node)
		{
			//Debug.Log ("VKLoad");
			if (this.node == null) {
				this.node = node;
			}
			//LoadResources ();
		}
		
		public void FixedUpdate ()
		{
			if (HighLogic.LoadedSceneIsFlight) {
				if (!tr) {
					tr = DictionaryManager.GetTrackResourceForVessel (vessel);
				}
				
				if (activen) {
					double consumed;
					//Debug.Log ("VKUpdate---");
					
					// Determine Output Need
					foreach (Resource item in outputs)
					{
						item.samples [curSample] = tr.GetConsumption (item.resource.name);
						curSample = (curSample + 1) % numSamples;
						if (item.samples.Average() > tr.GetConsumption (item.resource.name))
						{
							demand = item.samples.Average () * 1.10;
						} else {
							demand = tr.GetConsumption (item.resource.name) * 1.10;
						}
					}
					
					// Select Primary Throttle Driver and Determine throttle level necessary for Output
					primary = outputs.Find (x => x.type.Contains ("PRIMARY"));
					
					
					throttle = (double)primary.rateCurve.Evaluate((float)((1 / Time.deltaTime) * demand / primary.maxRate)) + 0.002;
					//Debug.Log ("Demand: " + demand + " maxRate: " + primary.maxRate + " DeltaTime: " + Time.deltaTime);
					
					// Determine if Generator should Idle
					if (throttle < minThrottle)
					{
						throttle = minThrottle;
						status = "Idling";
					}
					//Debug.Log("Throttle Calculated: " + throttle);
					
					// Determine and consume input resources, watching for inadequate supply
					foreach (Resource item in inputs)
					{
						curIn = (double)(item.maxRate * item.rateCurve.Evaluate ((float)throttle) * TimeWarp.fixedDeltaTime);
						//Debug.Log ("Requesting: " + curIn);
						consumed = part.RequestResource (item.resource.name, curIn);
						//Debug.Log ("Recieved: " + consumed);
						
						if (curIn*0.8 > consumed) {
							status = "FlameOut";
							//Debug.Log("Flameout, disabling Generator");
							Deactivate();
							break;
						} else {
							status = "Running";
						}
						//Debug.Log("Input: " + item.resource.name + " Consumed: " + curIn);
					}
					
					// Generate outputs
					if (status != "FlameOut")
					{
						foreach (Resource item in outputs)
						{
							curOut = (double)(item.maxRate * item.revRateCurve.Evaluate ((float)throttle) * TimeWarp.fixedDeltaTime);
							part.RequestResource (item.resource.name, (float)-curOut);
							//Debug.Log ("Output: " + item.resource.name + " generated: " + curOut);
						}
					}
					
					
					
					//Debug.Log("Outputs Generated: " + part.RequestResource (primary.resource.name, -demand));
				}
			}
		}
		
		[KSPEvent(guiActive = true, guiName = "Activate")]
		public void Activate ()
		{
			activen = true;
			status = "Running";
			Events ["Activate"].active = false;
			Events ["Deactivate"].active = true;
		}
		
		[KSPEvent(guiActive = true, guiName = "Deactivate", active = false)]
		public void Deactivate ()
		{
			activen = false;
			Events ["Activate"].active = true;
			Events ["Deactivate"].active = false;
		}
	}
}
	 
//	public class BalloonAnimator : PartModule
//	{
//		
//		public override void OnStart(PartModule.StartState state)
//		{
//			//Set Part Animation State to match current contents
//		}
//		public override void OnUpdate()
//		{
//this.part.Resources
//Update Part Animation State to match current contents
//		}
//	}


//	public class ModuleDustClouds : PartModule
//	{
//               
//		WheelCollider Wub;
//               
//		[KSPField]
//		public float minSpeed;
//               
//		public override void OnStart(PartModule.StartState state)
//		{
//			if (HighLogic.LoadedSceneIsFlight)
//			{
//				Wub = this.part.Modules.OfType<ModuleWheel>().SelectMany(m => m.wheels).Select(w => w.whCollider).First();
//			}
//		}
//               
//		public override void OnUpdate()
//		{
//                       
//			float Torque = Math.Abs(Wub.motorTorque);
//			float Speed = this.vessel.GetSrfVelocity().magnitude;
//                       
//			if (Wub.isGrounded && Speed > this.minSpeed)
//			{
//				Debug.Log("speed " + Speed + " ||| Torque " + Torque);
// spawn dust
//			}
//		}
//
//		
//	}
//}

