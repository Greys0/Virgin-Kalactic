using System;
using UnityEngine;
using KSP;
using System.Linq;
using System.Collections.Generic;
using BetterPart;

namespace Generators
{
	public class AdvGenerator: PartModule
	{

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
		
		//[KSPField(guiActive = true, guiName = "ThrottleRaw")]
		public double throttle = 0; // instantiation may not be necessary, setting to 0 until made use of
		[KSPField(guiActive = true, guiName = "Throttle")]
		public string throttleCleaned;
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

			public PartResourceDefinition resource
			{
				get { return this._resource; }
			}
			
			private double _maxRate = 1;

			public double maxRate
			{
				get { return this._maxRate; }
			}
			
			private FloatCurve _rateCurve = new FloatCurve ();
			
			public FloatCurve rateCurve
			{
				get { return this._rateCurve; }
			}
			
			private FloatCurve _revRateCurve = new FloatCurve ();
			
			public FloatCurve revRateCurve
			{
				get { return this._revRateCurve; }
			}
			
			public double[] samples;
			public double minThrottle;
			public string type;
			
			public Resource (ConfigNode node)
			{
				//Debug.Log ("VKLoading Resource");
				if (node.HasValue ("resourceName") && PartResourceLibrary.Instance.resourceDefinitions.Any (d => d.name == node.GetValue ("resourceName")))
				{
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
			if (node.HasNode ("INPUT") && node.HasNode ("OUTPUT"))
			{
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
			foreach (Resource item in outputs)
			{
				item.samples = new double[numSamples];
			}
		}
		
		public override void OnLoad (ConfigNode node)
		{
			//Debug.Log ("VKLoad");
			if (this.node == null) {
				this.node = node;
			}
		}
		
		public void FixedUpdate ()
		{
			if (HighLogic.LoadedSceneIsFlight)
			{
				
				TRHandler();
				
				if (activen)
				{
					
					//Debug.Log ("VKUpdate---");
					
					// there's no good reason for doing this, but I'm doing it anyways.
					UpdateDemand();
					
					// Select Primary Throttle Driver and Determine throttle level necessary for Output
					primary = outputs.Find (x => x.type.Contains ("PRIMARY"));
					
					
					//throttle = (double)primary.rateCurve.Evaluate((float)((1 / TimeWarp.fixedDeltaTime) * demand / primary.maxRate)) + 0.002;
					throttle = demand/(primary.maxRate * TimeWarp.fixedDeltaTime);
					throttle = Math.Max (minThrottle, Math.Min (1, throttle)); // Not integrated into the above line for debugging purposes
					//Debug.Log ("Demand: " + demand + " maxRate: " + primary.maxRate + " DeltaTime: " + Time.deltaTime);
					//Debug.Log("Throttle Calculated: " + throttle);
					
					// Determine and consume input resources
					ConsumeFuels();
					
					// Generate outputs
					if (status != "FlameOut") { GenerateProducts(); }
					
					throttleCleaned = (Math.Round(throttle, 3) * 100)+"%";
					//Debug.Log("Outputs Generated: " + part.RequestResource (primary.resource.name, -demand));
				}
			}
		}
		
		private void TRHandler() // separated to method expecting reuse, may not be necessary
		{
			if (!tr)
			{
				tr = DictionaryManager.GetTrackResourceForVessel (vessel);
			}
		}
		
		private void UpdateDemand()
		{
			foreach (Resource item in outputs)
			{
				item.samples [curSample] = tr.GetConsumption (item.resource.name);
				curSample = (curSample + 1) % numSamples;
				if (item.samples.Average() > tr.GetConsumption (item.resource.name))
				{
					demand = item.samples.Average ();
				} else {
					demand = tr.GetConsumption (item.resource.name);
				}
				//demand = demand * 1.01 + 0.005;
			}
		}
		
		private void ConsumeFuels()
		{
			
			double consumed;
			 
			//double[] accuracy = new double[inputs.Count];
			//int index = 0;
			
			foreach (Resource item in inputs)
			{
				curIn = (double)((item.maxRate * TimeWarp.fixedDeltaTime) * item.rateCurve.Evaluate ((float)throttle));
				//Debug.Log ("Requesting: " + curIn);
				consumed = part.RequestResource (item.resource.name, curIn);
				//Debug.Log ("Recieved: " + consumed);
				//Debug.Log ("ReqAcc: " + (curIn - consumed));
				
				if (status != "FlameOut")
				{
					if (curIn*0.8 > consumed)
					{
						status = "FlameOut";
						//Debug.Log("Flameout, disabling Generator");
						Deactivate();
					} else {
						status = "Running";
					}
				}

				//accuracy[index] = consumed / curIn;
				//index = (index + 1) % inputs.Count;
				//Debug.Log("Input: " + item.resource.name + " Consumed: " + curIn);
			}
			//return accuracy.Average();
		}
		
		private void GenerateProducts()
		{
			double generated;
			foreach (Resource item in outputs)
			{
				//curOut = (double)(item.maxRate * item.revRateCurve.Evaluate ((float)throttle) * TimeWarp.fixedDeltaTime);
				curOut = (double)item.maxRate*throttle;
				generated = part.RequestResource (item.resource.name, -curOut);
				//Debug.Log ("Accuracy of Output: " + (generated/demand));
				//Debug.Log ("Output: " + item.resource.name + " generated: " + curOut);
			}
		}
		
		[KSPEvent(guiActive = true, guiName = "Activate")]
		public void Activate()
		{
			activen = true;
			status = "Running";
			Events ["Activate"].active = false;
			Events ["Deactivate"].active = true;
		}
		
		[KSPEvent(guiActive = true, guiName = "Deactivate", active = false)]
		public void Deactivate()
		{
			activen = false;
			Events ["Activate"].active = true;
			Events ["Deactivate"].active = false;
		}
		
		[KSPAction("Activate Generator")]
		public void ActivateAction(KSPActionParam param)
		{
			Activate();
		}
		
		[KSPAction("Deactivate Generator")]
		public void DeactivateAction(KSPActionParam param)
		{
			Deactivate();
		}
		
		[KSPAction("Toggle Generator")]
		public void ToggleAction(KSPActionParam param)
		{
			if (activen) { Deactivate (); } else { Activate (); }
		}
	}
	
	public class FuelCell : PartModule
	{
		
		public enum state {
			Disabled,
			Charging,
			Discharging,
			Full,
			Depleted,
			ManualCharge,
			ManualDischarge
		}
		
		
		[KSPField]
		public string resource1;
		[KSPField]
		public string resource2;
		[KSPField]
		public double reagentRatio = 3;
		[KSPField]
		public double outputRatio = 10;
		[KSPField]
		public double electricOutput = 10;
		[KSPField]
		public double Efficiency = 0.6; // 60% loss in each direction
		
		
		private Resource reagent1 = new Resource(); // Hydrogen
		private Resource reagent2 = new Resource(); // Oxygen
		private Resource product1 = new Resource(); // H20
		private Resource electric = new Resource();
		
		private class Resource
		{
			public PartResourceDefinition definition;
			public List<PartResource> available = new List<PartResource>(); // parts that contain this resource (specifically the PartResource from thatPart.Resources.List)
			public double amount;		// How much of it there is
			public double capacity;		// How much there can be
		}
		
		private TrackResource tr;
		private Queue<double> trLog = new Queue<double>();
		
		[KSPField]
		public state mode = state.Disabled;
		
		public double rate = 0;
		
		public Helper.FrameJump checkTimer = new Helper.FrameJump();
		
		public override void OnStart (PartModule.StartState state)
		{
			reagent1.definition = PartResourceLibrary.Instance.GetDefinition(resource1);
			reagent1.definition = PartResourceLibrary.Instance.GetDefinition(resource2);
			electric.definition = PartResourceLibrary.Instance.GetDefinition("ElectricCharge");
			
			tr = DictionaryManager.GetTrackResourceForVessel(vessel);
			
			BuildResourceDirectory();
			
			checkTimer.Setup(100);
			
		}
		
		public override void OnFixedUpdate ()
		{
			rate = tr.GetGeneration("ElectricCharge") - tr.GetConsumption("ElectricCharge");
			
			trLog.Enqueue(rate);
			trLog.Dequeue();
			
			if (checkTimer.Good) { CheckMode(); }
			
			switch (mode)
			{
			case state.Disabled:	return;
			case state.Full:		return;
			case state.Depleted:	return;
			case state.Charging:	return;
			}
		}
		
		private state CheckMode ()
		{
			
			if (mode == state.ManualCharge || mode == state.ManualDischarge) { return mode; } // Escape if Manual
			
			QueryResourceDirectory();
			
			if (trLog.Average() > 0)
			{
				// state.Charging
				// check if space for generated reagents
				if (reagent1.amount < reagent1.capacity && reagent2.amount < reagent2.capacity)
				{
					if (product1.amount > 0)
					{
						mode = state.Charging; // All systems go
					} else {
						mode = state.Depleted; // No product remains to electrolyze
					}
				} else {
					mode = state.Full; // No space remains to store electrolysis results
				}
			} else {
				// state.Discharging
				// check if reagents available to generate EC
				if (product1.amount < product1.capacity)
				{
					if (reagent1.amount > 0 && reagent2.amount > 0)
					{
						mode = state.Discharging; // All systems go
					} else {
						mode = state.Depleted; // no reagents remaining to 
					}
				} else {
					mode = state.Full; // No space remaining for product
				}
			}
			return state.Disabled;
		}
		
		private void BuildResourceDirectory()
		{
			//needs:
			//
			
			reagent1.available.Clear ();
			reagent2.available.Clear ();
			
			foreach (Part partAtHand in vessel.parts)
			{
				
				foreach (PartResource res in partAtHand.Resources.list)
				{
					if (res.name == resource1)
					{
						reagent1.available.Add (res);
					} else if (res.name == resource2) {
						reagent2.available.Add (res);
					}
				}
			}
			
			reagent1.capacity = reagent1.available.Sum(m => m.maxAmount);
			reagent2.capacity = reagent2.available.Sum(m => m.maxAmount);
			
		}
		
		private void QueryResourceDirectory()
		{
			reagent1.amount = reagent1.available.Sum(a => a.amount);
			reagent2.amount = reagent2.available.Sum(a => a.amount);
		}
		
		private void Charge ()
		{
			double avail = tr.GetGeneration("ElectricCharge") - tr.GetConsumption("ElectricCharge");
			
			if (mode == state.Charging || avail <= 0) { return; }
			
			if (mode == state.ManualCharge || avail <= 0) { avail = electricOutput; }
			
			
			
		}
		
	}

	public class Helper
	{
		
		// Tracks frames passing based on a cycle period specified
		public class FrameJump : MonoBehaviour
		{
			// Position in Cycle
			public int Index { get; private set; }
			
			// Cycle Length
			public int Periodicity { get; private set; }
			
			// Returns true at Index == 0
			public bool Good { get { return Index == 0; } }
			
			// returns false at Index == 0
			public bool Bad { get { return !Good; } }
			
			// If Start() fires before Setup() has been run
			public void Start () { if (Periodicity == 0) { enabled = false; } }
			
			// Increase Index by one every physics frame, modulo by Periodicity
			public void FixedUpdate() { Index = (Index + 1) % Periodicity; }
			
			// Fill necessary value and enable monobehavior in case it has been disabled
			public void Setup(int p)
			{
				this.Periodicity = p;
				enabled = true;
			}
			
			// Destroy object via Unity's garbage chute
			public void Die() { Destroy(this); }
			
		}
	}
}
