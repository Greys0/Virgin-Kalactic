using System;
using KSP;
using UnityEngine;
using System.Collections.Generic;
using System.Data.Linq;


namespace NodeUtilities
{
    public class NodeToggle : PartModule
    {

        [KSPEvent(guiActive = false, guiActiveEditor = true, guiName = "Show Nodes")]
        public void ShowNodes()
        {
            MyDebugLog("ShowNodes: start");
            this.Events["HideNodes"].active = true;
            this.Events["ShowNodes"].active = false;

            refreshANVisualList();

            foreach (AttachNode node in aNVisualList)
            {
                createVisibleNode(node);
            }
            MyDebugLog("ShowNodes: end");
        }

        [KSPEvent(guiActive = false, guiActiveEditor = true, guiName = "Hide Nodes", active = false)]
        public void HideNodes()
        {
            MyDebugLog("HideNodes: start");
            this.Events["HideNodes"].active = false;
            this.Events["ShowNodes"].active = true;

            foreach (AttachNode node in aNVisualList)
            {
                node.icon.renderer.enabled = false;
            }
            MyDebugLog("HideNodes: end");
        }

        private List<AttachNode> aNList;
        private List<AttachNode> aNVisualList;
        //private Dictionary<string, bool> attachNodesStates = new Dictionary<string, bool>();

        EditorVesselOverlays vesselOverlays;
        Material crashTestNodeMaterial;

        public override void OnStart(PartModule.StartState state)
        {
            MyDebugLog("OnStart: start");
            if (HighLogic.LoadedSceneIsEditor) 
            {
                if (aNList == null)
                {
                        MyDebugLog("Processing AttachNodes for: " + part.name);

                    aNList = new List<AttachNode>(part.attachNodes);
                    MyDebugLog("Nodes: " + aNList.Count);

                    foreach (AttachNode node in aNList)
                    {
                        MyDebugLog("Node: " + node.id);
                        //attachNodesStates.Add(node.id, true);
                        populateToggle(node);
                    }
                }

                refreshANVisualList();
            }


            vesselOverlays = (EditorVesselOverlays)GameObject.FindObjectOfType(
                typeof(EditorVesselOverlays));

            crashTestNodeMaterial = vesselOverlays.CoMmarker.gameObject.renderer.material;
            MyDebugLog("OnStart: end");
        }

        private void populateToggle(AttachNode node)
        {
            MyDebugLog("populateToggle: " + node.id);

            BaseEvent item = new BaseEvent(new BaseEventList(part, this), node.GetHashCode().ToString(), () => toggle(node.GetHashCode()));

            item.active = true;
            item.guiActiveEditor = true;
            item.guiName = node.id + " || Active";

            Events.Add(item);
            MyDebugLog("populateToggle: end");
        }

        public void toggle(int caller)
        {
            MyDebugLog("toggle Start: " + caller);
            int hashcode = caller.GetHashCode();
            MyDebugLog(hashcode);
            AttachNode node = aNList.Find(a => a.GetHashCode() == caller.GetHashCode());
            MyDebugLog("Toggling Node: " + node.id);
            AttachNode nodeVisual = aNVisualList.Find(a => a.GetHashCode() == caller.GetHashCode());
            MyDebugLog(nodeVisual);

            if (part.attachNodes.Contains(node))
            {
                MyDebugLog("Node Exists, Removing");
                part.attachNodes.Remove(node);
                Events[node.GetHashCode().ToString()].guiName = node.id + " || Inactive";
                enableVisualNodes(nodeVisual, false);
            }
            else
            {
                MyDebugLog("Node Absent, Adding");
                part.attachNodes.Add(node);
                MyDebugLog("attachNodes: " + node.id);
                Events[node.GetHashCode().ToString()].guiName = node.id + " || Active";
                enableVisualNodes(nodeVisual, true);
            }
            MyDebugLog("Toggle: end");
        }

        private void createVisibleNode(AttachNode node)
        {
            MyDebugLog("createVisibleNode: " + node);
            if (!Events[node.GetHashCode().ToString()].guiName.Contains("Inactive"))
            {

                if (node.icon == null)
                {
                    node.icon = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    node.icon.renderer.material = crashTestNodeMaterial;
                }

                node.icon.SetActive(true);
                node.icon.transform.localScale = ((Vector3.one * node.radius) * (node.size != 0 ? (float)node.size : (float)node.size + 0.5f));
                node.icon.renderer.material.color = XKCDColors.RadioactiveGreen;
                node.icon.transform.position = (this.part.transform.TransformPoint(node.position));
                node.icon.renderer.enabled = true;
            }
            MyDebugLog("Toggle: end");
        }

        void enableVisualNodes(AttachNode node, bool isEnabled)
        {
            MyDebugLog("enableVisualNodes: start: node: " + node + " enabled: " + isEnabled);
            if (this.Events["HideNodes"].active == true)
            {
                createVisibleNode(node);
                MyDebugLog("Set Node: " + isEnabled);
                node.icon.renderer.enabled = isEnabled;
            }
            MyDebugLog("enableVisualNodes: exit");
        }

        void resetVisualNodes(Part part)
        {
            MyDebugLog("createVisibleNode: " + part);
            this.Events["HideNodes"].active = false;
            this.Events["ShowNodes"].active = true;

            foreach (AttachNode node in aNVisualList)
            {
                node.icon.renderer.enabled = false;
            }
            MyDebugLog("enableVisualNodes: exit");
        }

        // Populates ANVisualList with current attachNodes
        void refreshANVisualList()
        {
            MyDebugLog("refreshANVisualList: start");
            if (aNVisualList == null)
            {
                MyDebugLog("Setting aNVisualList");
                aNVisualList = new List<AttachNode>(part.attachNodes);
            }
            MyDebugLog("refreshANVisualList: end");
        }

        // Pipe out debug if DEBUG constant is true.
        void MyDebugLog(object message)
        {
#if DEBUG
            Debug.Log(message);
#endif
        }
    }
}
