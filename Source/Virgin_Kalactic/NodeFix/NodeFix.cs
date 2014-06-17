

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using KSP;
using UnityEngine;


namespace NodeFix
{
	[KSPAddon (KSPAddon.Startup.MainMenu, true) ]
	public class NodeFix : MonoBehaviour
	{
		public void Start ()
		{
			
			UrlDir.UrlConfig[] partNodes = GameDatabase.Instance.GetConfigs ("PART");
			List<AvailablePart> parts = PartLoader.Instance.parts;
			
			Debug.Log ("==== Node Fixer ====");
			
			foreach (UrlDir.UrlConfig partAtHand in partNodes)
			{
				

				ConfigNode configAtHand = partAtHand.config;
				Debug.Log ("Checking Part: " + configAtHand.GetValue ("name")); // partAtHand.id is blank, .name is PART, .GetValue("name") is blank
				
				/*ConfigNode.ValueList bup = configAtHand.values;
				ConfigNode.ConfigNodeList pub = configAtHand.;
				foreach (ConfigNode puba in pub)
				{
					//Debug.Log (puba + " || " + puba.id + " || " + puba.CountValues);
				}
				foreach (ConfigNode.Value bupa in bup)
				{
					Debug.Log (bupa.name + " || " + bupa.value);
				}*/
				
				ConfigNode[] nodes = configAtHand.GetNodes ("NODE");
				
				foreach (ConfigNode nodeAtHand in nodes)
				{
					Debug.Log ("Checking Node: " + nodeAtHand.GetValue("name")); // nodeAtHand.id and .GetValue("node") are blank, .name is NODE
					
					if (nodeAtHand.HasValue ("size"))
					{
						Debug.Log ("Original Size Confirmed");
						AvailablePart part = parts.FirstOrDefault (p => p.name == configAtHand.GetValue ("name"));
						Debug.Log ("bup2");
						if (part != null)
						{
							Debug.Log ("PartPrefab Located");
							AttachNode attach = part.partPrefab.attachNodes.FirstOrDefault (a => a.id == nodeAtHand.GetValue ("id"));
							Debug.Log ("AttachNode Located");
							
							int size;
							if (int.TryParse(nodeAtHand.GetValue ("size"), out size))
							{
								
								attach.size = size;
								Debug.Log("AttachNode Fixed");
								
							} else {
								Debug.Log ("Node is Invalid: Size is not int");
							}
							
						} else {
							Debug.Log ("Part does not have any AttachNodes");
						}
						
					} else {
						Debug.Log ("Node is Invalid: No Size Defined");
					}
					
				}
			}
		}
		
	}
	
	
}

