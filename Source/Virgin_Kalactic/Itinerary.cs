using System;
using UnityEngine;
using KSP;
using System.Linq;
using System.Collections.Generic;

namespace Virgin_Kalactic
{
	
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

}