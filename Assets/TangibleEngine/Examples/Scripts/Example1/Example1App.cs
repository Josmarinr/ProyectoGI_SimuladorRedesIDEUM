
using System.Collections.Generic;
using Shared;
using TE;
using UnityEngine;

namespace Example1 {

  /// <summary>
  /// This class provides logic for demonstrating the use of a tangible 'hotspot'. When a tangible is over this pre-defined area, the UI is shown.
  /// Additionally, the contents of the UI change based on the pattern ID of the tangible that is over the 'hotspot'. If there's more than one tangible,
  /// the first tangible that was placed down and has remained over the 'hotspot' will be used to determine the content being displayed.
  ///
  /// Important Note:
  /// - If the game output window is set to a different resolution than 3840 x 2160 (UHD), a tangibles position will not be correct. Additionally,
  ///   tangibles may not recognize at all. This should not be an issue in release builds assuming that the application is ran in fullscreen and the
  ///   table's resolution is set to 3840x2160 (UHD).
  /// </summary>
  public class Example1App : MonoBehaviour {

    /// <summary>
    /// Reference to the camera responsible for displaying the UI. The camera is also used for performing raycasts into the scene.
    /// </summary>
    public Camera Camera;

    /// <summary>
    /// Reference to the collider used as the tangible 'hotspot'. Raycasts from the camera are checked to see if a tangible's center point intersects
    /// with this collider.
    /// </summary>
    public Collider2D TangibleHotspotCollider;

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
    /// The id of the 'active' tangible. This is the tangible that is used to determine the UI's visibility and what content is currently being shown.
    /// </summary>
    private int? _activeTangibleId;

    /// <summary>
    /// Holds the associations of a pattern id to an index of a content's GameObject (as stored in the Contents field).
    /// </summary>
    private readonly Dictionary<int, int> _patternIdToContentsMap = new Dictionary<int, int>();

    /// <summary>
    /// Holds the collection of content array indices that have not been associated with a pattern-id
    /// </summary>
    private readonly Queue<int> _unseenContentIndices = new Queue<int>();


    void Start() {
      //setup the unseen content indices queue
      for (var i = 0; i < Contents.Length; i++) {
        _unseenContentIndices.Enqueue(i);
      }

      //put the UI in its default state
      HideAllUiContent();
      ContentContainer.FadeOut(0f, 0f);

      //Subscribe to the relevant tangible engine events
      TangibleEngine.OnTangibleAdded += TangibleEngine_OnTangibleAdded;
      TangibleEngine.OnTangibleRemoved += TangibleEngine_OnTangibleRemoved;
      TangibleEngine.OnTangibleUpdated += TangibleEngine_OnTangibleUpdated;
    }

    void OnDestroy() {

      //Kill any potential tween happening in the content container.
      ContentContainer.StopFading();

      //Unsubscribe from the relevant tangible engine events
      TangibleEngine.OnTangibleAdded -= TangibleEngine_OnTangibleAdded;
      TangibleEngine.OnTangibleRemoved -= TangibleEngine_OnTangibleRemoved;
      TangibleEngine.OnTangibleUpdated -= TangibleEngine_OnTangibleUpdated;
    }

    private void TangibleEngine_OnTangibleAdded(Tangible t) {
      if (_activeTangibleId == null) {
        // There's no active tangible. Try to turn on the UI with this tangible.
        TrySetActiveTangible(t);
      }
    }

    private void TangibleEngine_OnTangibleRemoved(Tangible t) {
      if (_activeTangibleId == null) {
        // There's no active tangible, so there's nothing to do here. early out.
        return;
      }

      if (_activeTangibleId.Value == t.Id) {
        // Current active tangible has just been removed, turn off the UI.
        UnsetActiveTangible();
      }
    }

    private void TangibleEngine_OnTangibleUpdated(Tangible t) {
      if (_activeTangibleId == null) {
        // There's no active tangible. Try to turn on the UI with this tangible.
        TrySetActiveTangible(t);
      }
      else if (t.Id == _activeTangibleId && !IsWithinHotspot(t)) {
        // The current active tangible's position is no longer within the hotspot. Turn off the UI.
        UnsetActiveTangible();
      }
    }

    /// <summary>
    /// Check to see if the provided Tangible instance meets the criteria for becoming the active tangible. If the provided tangible does meet the
    /// criteria, show the UI and set the UI's content to match the content associated with the pattern-id of this tangible instance.
    /// </summary>
    /// <param name="t">The tangible to check against the criteria</param>
    private void TrySetActiveTangible(Tangible t) {
      //check to see if there's already an active tangible and return early if there is.
      if (_activeTangibleId != null) return;

      //check to see if the tangible is within the hotspot. If it is not, return early.
      if (!IsWithinHotspot(t)) return;

      //The provided tangible has met the criteria for becoming the active tangible. Set it as the active tangible and update the UI accordingly.
      _activeTangibleId = t.Id;
      ContentContainer.FadeIn();
      SetUiContent(t.PatternId);
    }

    /// <summary>
    /// Unset the current active tangible and turn off the UI
    /// </summary>
    private void UnsetActiveTangible() {
      _activeTangibleId = null;
      ContentContainer.FadeOut();
    }

    /// <summary>
    /// Checks to see if the provided tangible intersects with the hotspot's collider.
    /// </summary>
    /// <param name="t">The tangible instance to check</param>
    /// <returns>true if the provided tangible's position intersects with the hotspot's collider. False otherwise.</returns>
    private bool IsWithinHotspot(Tangible t) {
      // Get the world position of the tangible (which is in screen space by default)
      var worldPoint = Camera.ScreenToWorldPoint(t.Pos);
      // Try to see if the world position of the tangible overlaps with a 2D collider.
      var hitCollider = Physics2D.OverlapPoint(worldPoint);
      // Return true if the overlapped collider is the the hotspot collider.
      return hitCollider == TangibleHotspotCollider;
    }

    /// <summary>
    /// Updates the current UI content to match the content associated with the provided pattern-id. If there's no content associated with the provided
    /// pattern-id and there's still content that has not been shown, associate a piece of un-shown content with the provided pattern-id and display it.
    /// </summary>
    /// <param name="patternId"></param>
    private void SetUiContent(int patternId) {
      int targetContentIndex;
      if (_patternIdToContentsMap.ContainsKey(patternId)) {
        //the pattern id is associated with a content index, select the content index associated with the pattern id
        targetContentIndex = _patternIdToContentsMap[patternId];
      }
      else if (_unseenContentIndices.Count > 0) {
        //the pattern id is not associated with any content indices, and there's 'unseen' content. Make an association ans select the content index
        targetContentIndex = _unseenContentIndices.Dequeue();
        _patternIdToContentsMap[patternId] = targetContentIndex;
      }
      else {
        //There's no 'unseen' content to see. Use the default value.
        targetContentIndex = 0;
      }

      for (var i = 0; i < Contents.Length; i++) {
        //iterate through the array of contents and set each game object to be active if it's index is the target, or inactive if its index is not the 
        //target index.
        var content = Contents[i];
        content.SetActive(i == targetContentIndex);
      }
    }

    /// <summary>
    /// Hides all content associated with the UI.
    /// </summary>
    private void HideAllUiContent() {
      foreach (var content in Contents) {
        content.SetActive(false);
      }
    }
  }
}
