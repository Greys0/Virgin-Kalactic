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
			
			List<AvailablePart> pool = PartLoader.Instance.parts;
			
			foreach (AvailablePart part in pool)
			{
				Debug.Log("Part: " + part.name);
				
				AttachNode node;
				node = part.partPrefab.attachNodes.Find (x => x.id == "srfAttach");
				
				if (node == null) {
					node = part.partPrefab.attachNodes.Find (x => x.id == "attach");
					if (node != null) { Debug.Log("attach node found"); }
					
				} else {
					Debug.Log("srfAttach node found");
					
				}
				
				if (node != null) {
					tallySrf++;
					if (part.partPrefab.srfAttachNode != node)
					{
						Debug.Log("srfAttachNode Not Set, Fixing...");
						part.partPrefab.srfAttachNode = node;
						tallyFix++;
						Debug.Log ("Removing srfAttachNode from main list");
						part.partPrefab.attachNodes.Remove(node);
						
					} else {
						Debug.Log("srfAttachNode Already Set" + part.partPrefab.srfAttachNode.position.ToString());
					}
				} else {
					Debug.Log("No srfAttachNode Candidates");
					
				}
			}
			
			Debug.Log ("Parts Total: " + pool.Count);
			Debug.Log ("Srf Nodes: " + tallySrf);
			Debug.Log ("Parts Fixed: " + tallyFix);
		}
	}
}