using System;
using KSP;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;


namespace NodeUtilities
{
	public class NodeToggle : PartModule
	{
		
		[KSPField]
		private List<AttachNode> aNList = new List<AttachNode>();
		private Dictionary<string, bool> attachNodesStates = new Dictionary<string, bool>();
		
		public override void OnStart (PartModule.StartState state)
		{
			Debug.Log("NodeToggle Prep");
			if (HighLogic.LoadedSceneIsEditor && aNList.Count == 0)
			{
				Debug.Log("Processing AttachNodes for: " + part.name);
				aNList = part.attachNodes;
				Debug.Log("Nodes: " + aNList.Count);
				
				foreach (AttachNode node in aNList)
				{
					Debug.Log("Node: " + node.id);
					attachNodesStates.Add(node.id, true);
					populateToggle(node);
				}
				
				
			}
		}
		
		private void populateToggle (AttachNode node)
		{
			Debug.Log ("-Creating Event for: " + node.id);
			
			BaseEvent item = new BaseEvent(new BaseEventList(part, this), node.id, () => toggle(node.id));
			item.active = true;
			item.guiActiveEditor = true;
			item.guiName = node.id;
			
			Events.Add (item);

		}
		
		public void toggle (string caller)
		{
			Debug.Log ("toggling AttachNode: " + caller);
			AttachNode node = part.attachNodes.Find(a => a.id == caller);
			
			if (node != null)
			{
				part.attachNodes.Remove(node);
			} else {
				part.attachNodes.Add (aNList.Find(a => a.id == caller));
			}
		}
		
	}
}