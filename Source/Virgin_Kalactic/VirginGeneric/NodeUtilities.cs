using System;
using KSP;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;


namespace NodeUtilities
{
	public class NodeToggle : PartModule
	{
		
		private List<AttachNode> aNList;
		//private Dictionary<string, bool> attachNodesStates = new Dictionary<string, bool>();
		
		public override void OnStart (PartModule.StartState state)
		{
			Debug.Log("NodeToggle Prep");
			if (aNList == null && HighLogic.LoadedSceneIsEditor)
			{
				
				Debug.Log("Processing AttachNodes for: " + part.name);
				
				aNList = new List<AttachNode>(part.attachNodes);
				Debug.Log("Nodes: " + aNList.Count);
				
				foreach (AttachNode node in aNList)
				{
					Debug.Log("Node: " + node.id);
					//attachNodesStates.Add(node.id, true);
					populateToggle(node);
				}
			}
		}
		
		private void populateToggle (AttachNode node)
		{
			Debug.Log ("-Creating Event for: " + node.id);
			
			BaseEvent item = new BaseEvent(new BaseEventList(part, this), node.GetHashCode().ToString(), () => toggle(node.GetHashCode()));
			item.active = true;
			item.guiActiveEditor = true;
			item.guiName = node.id + " || Active";
			
			Events.Add (item);

		}
		
		public void toggle (int caller)
		{
			int hashcode = caller.GetHashCode();
			AttachNode node = aNList.Find(a => a.GetHashCode() == caller.GetHashCode());
			Debug.Log ("Toggling Node: " + node.id);
			
			
			if (part.attachNodes.Contains(node))
			{
				Debug.Log("Node Exists, Removing");
				part.attachNodes.Remove(node);
				Events[node.GetHashCode().ToString()].guiName = node.id + " || Inactive";
			} else {
				Debug.Log("Node Absent, Adding");
				part.attachNodes.Add (node);
				Events[node.GetHashCode().ToString()].guiName = node.id + " || Active";
			}
			Debug.Log ("Toggle Complete");
		}
		
	}
}