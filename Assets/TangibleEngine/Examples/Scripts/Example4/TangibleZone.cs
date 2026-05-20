using Shared;
using System;
using System.Collections;
using System.Collections.Generic;
using TE;
using UnityEngine;

namespace Example4
{
  public class TangibleZone : MonoBehaviour
  {
    /// <summary>
    /// The id of the 'active' tangible. This is the tangible that is used to determine the UI's visibility and what content is currently being shown.
    /// </summary>
    public int? _activeTangibleId;

    /// <summary>
    /// Reference to the UI container. This object is used to control the visibility of the entire UI.
    /// </summary>
    public CanvasGroupVisibility ContentContainer;

    /// <summary>
    /// References to the content of the UI. Each tangible has a unique 'pattern id' which is used to display unique content. For the purposes of this
    /// example, each piece of content is mapped to a pattern id at runtime.
    /// </summary>
    public GameObject[] Contents;

    /// <summary>
    /// Holds the collection of content array indices that have not been associated with a pattern-id
    /// </summary>
    private readonly Queue<int> _unseenContentIndices = new Queue<int>();

    /// <summary>
    /// Holds the associations of a pattern id to an index of a content's GameObject (as stored in the Contents field).
    /// </summary>
    private readonly Dictionary<int, int> _patternIdToContentsMap = new Dictionary<int, int>();

    private void Start()
    {
      //setup the unseen content indices queue
      for (var i = 0; i < Contents.Length; i++)
      {
        _unseenContentIndices.Enqueue(i);
      }
      HideAllUiContent();
      ContentContainer.FadeOut(0f, 0f);
    }

    internal void ShowInfo(Tangible t)
    {
      if (_activeTangibleId == null)
      {
        _activeTangibleId = t.Id;
        ContentContainer.FadeIn();
        SetUiContent(t.PatternId);
      }
    }

    /// <summary>
    /// Hides all content associated with the UI.
    /// </summary>
    private void HideAllUiContent()
    {
      foreach (var content in Contents)
      {
        content.SetActive(false);
      }
    }

    /// <summary>
    /// Updates the current UI content to match the content associated with the provided pattern-id. If there's no content associated with the provided
    /// pattern-id and there's still content that has not been shown, associate a piece of un-shown content with the provided pattern-id and display it.
    /// </summary>
    /// <param name="patternId"></param>
    private void SetUiContent(int patternId)
    {
      int targetContentIndex;
      if (_patternIdToContentsMap.ContainsKey(patternId))
      {
        //the pattern id is associated with a content index, select the content index associated with the pattern id
        targetContentIndex = _patternIdToContentsMap[patternId];
      }
      else if (_unseenContentIndices.Count > 0)
      {
        //the pattern id is not associated with any content indices, and there's 'unseen' content. Make an association ans select the content index
        targetContentIndex = _unseenContentIndices.Dequeue();
        _patternIdToContentsMap[patternId] = targetContentIndex;
      }
      else
      {
        //There's no 'unseen' content to see. Use the default value.
        targetContentIndex = 0;
      }

      for (var i = 0; i < Contents.Length; i++)
      {
        //iterate through the array of contents and set each game object to be active if it's index is the target, or inactive if its index is not the 
        //target index.
        var content = Contents[i];
        content.SetActive(i == targetContentIndex);
      }
    }

    private void OnDestroy()
    {
      //Kill any potential tween happening in the content container.
      ContentContainer.StopFading();
    }

    internal void HideInfo()
    {
      _activeTangibleId = null;
      ContentContainer.FadeOut();
    }
  }
}
