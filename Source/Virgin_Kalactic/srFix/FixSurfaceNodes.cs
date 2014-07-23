using System;
using System.Linq;
using System.Collections.Generic;
using KSP;
using UnityEngine;

namespace srFix
{
	
	[KSPAddon (KSPAddon.Startup.MainMenu, true)]
	public class FixSurfaceNodes : MonoBehaviour
	{
		
		public void Start ()
		{
			
			int tallyFix = 0;
			int tallySrf = 0;
			
			Debug.Log ("==== Fixing srfAttachNodes for all parts ====");
			
			List<AvailablePart> parts = PartLoader.Instance.parts;
			
			foreach (AvailablePart partAtHand in parts)
			{
				Debug.Log("Part: " + partAtHand.title);
				
				AttachNode node;
				node = partAtHand.partPrefab.attachNodes.Find (x => x.id == "srfAttach");
				
				if (node == null) {
					node = partAtHand.partPrefab.attachNodes.Find (x => x.id == "attach");
					if (node != null) { Debug.Log("attach node found"); }
					
				} else {
					Debug.Log("srfAttach node found");
					
				}
				
				if (node != null) {
					tallySrf++;
					if (partAtHand.partPrefab.srfAttachNode != node)
					{
						Debug.Log("srfAttachNode Not Set, Fixing...");
						partAtHand.partPrefab.srfAttachNode = node;
						tallyFix++;
						
					} else {
						Debug.Log("srfAttachNode Already Set" + partAtHand.partPrefab.srfAttachNode.position.ToString());
					}
				} else {
					Debug.Log("No srfAttachNode Candidates");
					
				}
			}
			
			Debug.Log ("Parts Total: " + parts.Count);
			Debug.Log ("Srf Nodes: " + tallySrf);
			Debug.Log ("Parts Fixed: " + tallyFix);
		}
	}
}

