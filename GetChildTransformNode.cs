using System.Collections.Generic;
using UnityEngine;
using Warudo.Core.Attributes;
using Warudo.Core.Data.Models;
using System.Linq;
using Warudo.Core.Graphs;
using Cysharp.Threading.Tasks;
using Warudo.Core.Data;
using System;

namespace veasu.transform
{
  [NodeType(Id = "com.veasu.getchildtransformnode", Title = "Get Child Transform", Category = "Transform")]
  public class GetChildTransformNode : Warudo.Core.Graphs.Node
  {
    [DataInput]
    [Label("Root Object")]
    Warudo.Plugins.Core.Assets.GameObjectAsset rootAsset;

    [DataInput]
    [HiddenIf(nameof(hideGameObject))]
    [AutoComplete(nameof(AutoCompleteChildObjectList))]
    [Label("Child Object")]
    public string childObjects;

    [DataOutput]
    [HiddenIf(nameof(hideGameObject))]
    [Label("Child Object Transform")]
    public TransformData childTransform() {
      if (selectedObject != null) {
        return outputData;
      }
      return null;
    }

    private bool hideGameObject() {
      return transformDict == null || transformDict.Count == 0;
    }
    private async UniTask<AutoCompleteList> AutoCompleteChildObjectList() => AutoCompleteList.Single(transformDict.Select<KeyValuePair<string,Transform>, AutoCompleteEntry>((Func<KeyValuePair<string,Transform>, AutoCompleteEntry>)(it => new AutoCompleteEntry()
    {
      label = it.Key,
      value = it.Key
    })));

    private TransformData outputData = new TransformData();
    private Dictionary<string, Transform> transformDict;
    private string selectedObject = null;

    protected override void OnCreate() {
      Watch<Warudo.Plugins.Core.Assets.GameObjectAsset>(nameof(rootAsset), (from, to) => {
        if (to != null && to.Active) {
          var tempDict = new Dictionary<string, Transform>();
          foreach (Transform child in to.GameObject.transform)
          {
            if (child != null) {
              tempDict.Add(child.name, child);
            }
          }
          this.transformDict = tempDict;
        } else {
          this.transformDict = new Dictionary<string, Transform>();
        }
        childObjects = null;
        selectedObject = null;
        BroadcastDataInput(nameof(childObjects));
      });

      Watch<string>(nameof(childObjects), (from, to) => {
        if (to != null) {
          selectedObject = to;
        } else {
          selectedObject = null;
        }
      });
    }

    public override void OnUpdate() {
      if (selectedObject != null) {
        outputData.CopyFromWorldTransform(transformDict[selectedObject].transform);
      }
    }
  }

}
