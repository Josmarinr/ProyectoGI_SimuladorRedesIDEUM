using System.Collections.Generic;
using Shared;
using TE;
using UnityEngine;
using UnityEngine.UI;

namespace Example2 {

  /// <summary>
  /// This class provides logic for demonstrating the use of a 'floating' UI that is attached to an active tangible. Additionally, this implementation
  /// shows how pattern information that is associated with an active tangible can be used. See <see cref="FloatingUi"/> for more information.
  /// 
  /// Important Note:
  /// - If the game output window is set to a different resolution than 3840 x 2160 (UHD), a tangibles position will not be correct. Additionally,
  ///   tangibles may not recognize at all. This should not be an issue in release builds assuming that the application is ran in fullscreen and the
  ///   table's resolution is set to 3840x2160 (UHD).
  /// </summary>
  public class Example2App : MonoBehaviour {

    private static readonly int SelectedProperty, NormalProperty;

    static Example2App() {
      SelectedProperty = Animator.StringToHash("Selected");
      NormalProperty = Animator.StringToHash("Normal");
    }

    /// <summary>
    /// Reference to the prefab of the floating UI
    /// </summary>
    public GameObject FloatingUiPrefab;

    /// <summary>
    /// Reference to the canvas. Floating UI instances are made children of this object.
    /// </summary>
    public Canvas Canvas;

    /// <summary>
    /// Reference to the camera responsible for rendering the UI. The camera is used to transform the position of tangibles to world space coordinates.
    /// </summary>
    public Camera Camera;

    /// <summary>
    /// Reference to the call to action text that is used to prompt the user that they should place down a tangible.
    /// </summary>
    public CanvasGroupVisibility CallToActionText;

    public GameObject BlocksCollidersContainer, BevelCollidersContainer;
    
    public Button BlocksButton, BevelButton;
    public Animator BlocksAnimator, BevelAnimator;

    /// <summary>
    /// Collection of floating UI instances that are not currently active
    /// </summary>
    private readonly Queue<FloatingUi> _floatingUiQueue = new Queue<FloatingUi>();

    /// <summary>
    /// Collection which holds the mapping from tangible id to floating UI instances.
    /// </summary>
    private readonly Dictionary<int, FloatingUi> _tangibleToFloatingUiMap = new Dictionary<int, FloatingUi>();

    void Start() {

      BlocksButton.onClick.AddListener(HandleBlocksClick);
      BevelButton.onClick.AddListener(HandleBevelClick);

      // Use one of the two edge modes to determine the floating ui rotation behavior
      SelectMode(true);

      //Subscribe to the relevant tangible engine events
      TangibleEngine.OnTangibleAdded += TangibleEngine_OnTangibleAdded;
      TangibleEngine.OnTangibleRemoved += TangibleEngine_OnTangibleRemoved;
      TangibleEngine.OnTangibleUpdated += TangibleEngine_OnTangibleUpdated;
    }

    void OnDestroy() {
      //Unsubscribe from the relevant tangible engine events
      TangibleEngine.OnTangibleAdded -= TangibleEngine_OnTangibleAdded;
      TangibleEngine.OnTangibleRemoved -= TangibleEngine_OnTangibleRemoved;
      TangibleEngine.OnTangibleUpdated -= TangibleEngine_OnTangibleUpdated;
    }

    private void TangibleEngine_OnTangibleAdded(Tangible t) {
      // get a currently unused floating ui
      var item = GetFloatingUi();
      //Update the floating ui to show information about the pattern
      item.SetInfo(t.Pattern);
      //Update the position of the floating ui
      item.UpdateTransform(t, Camera);
      //Map the tangible's id to this floating ui
      _tangibleToFloatingUiMap[t.Id] = item;
      if (_tangibleToFloatingUiMap.Count == 1) {
        //if there's exactly one active floating ui, cause the call to action text to fade out
        CallToActionText.FadeOut();
      }
    }

    private void TangibleEngine_OnTangibleRemoved(Tangible t) {
      //Try to get the floating ui instance associated with this tangible's id
      if (_tangibleToFloatingUiMap.TryGetValue(t.Id, out var item)) {
        //un-map the floating ui from the tangible's id
        _tangibleToFloatingUiMap.Remove(t.Id);
        //return the floating ui to the pool
        ReturnFloatingUi(item);
        if (_tangibleToFloatingUiMap.Count <= 0) {
          //if this was the last floating ui, show the call to action text
          CallToActionText.FadeIn();
        }
      }
    }

    private void TangibleEngine_OnTangibleUpdated(Tangible t) {
      if (_tangibleToFloatingUiMap.TryGetValue(t.Id, out var item)) {
        //if there's a floating ui mapped to this tangible's id, update the transform of the floating ui
        item.UpdateTransform(t, Camera);
      }
    }

    /// <summary>
    /// Gets or creates a <see cref="FloatingUi"/> prefab. A <see cref="FloatingUi"/> is created when there are no instances stored in the pool
    /// </summary>
    /// <returns></returns>
    private FloatingUi GetFloatingUi() {
      FloatingUi item;
      if (_floatingUiQueue.Count > 0) {
        item = _floatingUiQueue.Dequeue();
      }
      else {
        var go = Instantiate(FloatingUiPrefab, Canvas.transform);
        item = go.GetComponent<FloatingUi>();
      }
      item.gameObject.SetActive(true);
      return item;
    }

    /// <summary>
    /// Returns a <see cref="FloatingUi"/> instance to the pool of currently inactive <see cref="FloatingUi"/> instances.
    /// </summary>
    /// <param name="item"></param>
    public void ReturnFloatingUi(FloatingUi item) {
      item.gameObject.SetActive(false);
      _floatingUiQueue.Enqueue(item);
    }


    private void HandleBlocksClick() {
      SelectMode(false);
    }

    private void HandleBevelClick() {
      SelectMode(true);
    }

    public void SelectMode(bool bevel) {
      ResetTriggers(BlocksAnimator, BevelAnimator);
      if (bevel) {
        BlocksAnimator.SetTrigger(NormalProperty);
        BevelAnimator.SetTrigger(SelectedProperty);
        BlocksCollidersContainer.SetActive(false);
        BevelCollidersContainer.SetActive(true);
      }
      else {
        BlocksAnimator.SetTrigger(SelectedProperty);
        BevelAnimator.SetTrigger(NormalProperty);
        BlocksCollidersContainer.SetActive(true);
        BevelCollidersContainer.SetActive(false);
      }
    }

    private static void ResetTriggers(params Animator[] anims) {
      foreach (var a in anims) {
        a.ResetTrigger(SelectedProperty);
        a.ResetTrigger(NormalProperty);
      }
    }
  }
}