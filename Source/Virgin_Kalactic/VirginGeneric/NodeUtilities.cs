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
            this.Events["HideNodes"].active = true;
            this.Events["ShowNodes"].active = false;
            if (aNVisualList == null)
                aNVisualList = new List<AttachNode>(part.attachNodes);
            foreach (AttachNode node in aNVisualList)
            {
                createVisibleNode(node);
            }
        }

        [KSPEvent(guiActive = false, guiActiveEditor = true, guiName = "Hide Nodes", active = false)]
        public void HideNodes()
        {
            this.Events["HideNodes"].active = false;
            this.Events["ShowNodes"].active = true;

            foreach (AttachNode node in aNVisualList)
            {
                node.icon.renderer.enabled = false;
            }
        }


        private List<AttachNode> aNList;
        private List<AttachNode> aNVisualList;
        //private Dictionary<string, bool> attachNodesStates = new Dictionary<string, bool>();

        EditorVesselOverlays vesselOverlays;
        Material crashTestNodeMaterial;
        public override void OnStart(PartModule.StartState state)
        {
            //Debug.Log("NodeToggle Prep");
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


            vesselOverlays = (EditorVesselOverlays)GameObject.FindObjectOfType(
                typeof(EditorVesselOverlays));

            crashTestNodeMaterial = vesselOverlays.CoMmarker.gameObject.renderer.material;
        }

        private void populateToggle(AttachNode node)
        {
            Debug.Log("-Creating Event for: " + node.id);

            BaseEvent item = new BaseEvent(new BaseEventList(part, this), node.GetHashCode().ToString(), () => toggle(node.GetHashCode()));

            item.active = true;
            item.guiActiveEditor = true;
            item.guiName = node.id + " || Active";

            Events.Add(item);
        }

        public void toggle(int caller)
        {
            int hashcode = caller.GetHashCode();
            AttachNode node = aNList.Find(a => a.GetHashCode() == caller.GetHashCode());
            AttachNode nodeVisual = aNVisualList.Find(a => a.GetHashCode() == caller.GetHashCode());
            Debug.Log("Toggling Node: " + node.id);


            if (part.attachNodes.Contains(node))
            {
                Debug.Log("Node Exists, Removing");
                part.attachNodes.Remove(node);
                Events[node.GetHashCode().ToString()].guiName = node.id + " || Inactive";
                enableVisualNodes(nodeVisual, false);
            }
            else
            {
                Debug.Log("Node Absent, Adding");
                part.attachNodes.Add(node);
                Events[node.GetHashCode().ToString()].guiName = node.id + " || Active";
                enableVisualNodes(nodeVisual, true);
            }
            Debug.Log("Toggle Complete");
        }

        private void createVisibleNode(AttachNode node)
        {

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
        }

        void enableVisualNodes(AttachNode node, bool isEnabled)
        {
            if (this.Events["HideNodes"].active == true)
            {
                node.icon.renderer.enabled = isEnabled;
            }
        }

        void resetVisualNodes(Part part)
        {
            this.Events["HideNodes"].active = false;
            this.Events["ShowNodes"].active = true;

            foreach (AttachNode node in aNVisualList)
            {
                node.icon.renderer.enabled = false;
            }
        }

    }
}
