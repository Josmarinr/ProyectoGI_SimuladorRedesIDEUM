using Shared;
using TE;
using UnityEngine;

namespace Example3 {

  /// <summary>
  /// This class provides the logic for demonstrating the use of a tangible rotation to control the appearance of UI elements.
  ///
  /// Once a tangible is placed down, the UI enters its "active" state. The active state displays three selectable objects. These objects are selected
  /// by rotating the tangible so that the rotation indicator (located behind the TE logo) is pointing at one of the objects. The Physics2D system is
  /// utilized to simplify the 'pointing at' logic. Specifically there's a kinematic rigid body that rotates with the rotation indicator. Triggers are
  /// toggled inside the <see cref="InfoPopOut.OnTriggerEnter2D"/> method and the <see cref="InfoPopOut.OnTriggerExit2D"/> method.
  ///
  /// Important Note(s):
  /// - If the game output window is set to a different resolution than 3840 x 2160 (UHD), a tangibles position will not be correct. Additionally,
  ///   tangibles may not recognize at all. This should not be an issue in release builds assuming that the application is ran in fullscreen and the
  ///   table's resolution is set to 3840x2160 (UHD).
  /// - This class does handle rapid 'toggling' of tangibles - it is possible to get the UI to be out of sync with the active tangible. This logic has
  ///   been left out to keep the implementation straight forward and simple.
  /// </summary>
  public class Example3App : MonoBehaviour {
    
    /// <summary>
    /// Reference to the green TE logo ring. The color of this ring is changed when a tangible is added or removed.
    /// </summary>
    public GraphicColorTweener Te2LogoRing;

    /// <summary>
    /// Collection of references to the dots present in the TE logo. The color of these dots are changed when a tangible is added or removed.
    /// </summary>
    public GraphicColorTweener[] Te2LogoDots;
    
    /// <summary>
    /// Reference to the game object used to display the rotation of the currently active tangible.
    /// </summary>
    public Transform RotationIndicator;

    /// <summary>
    /// Reference to the behavior responsible for hiding and showing the rotation indicator. 
    /// </summary>
    public CanvasGroupVisibility RotationIndicatorGfx;

    /// <summary>
    /// Collection of references to the pop-out UI elements. These elements are shown when there's an active tangible and hidden when there's no
    /// active tangible.
    /// </summary>
    public InfoPopOut[] PopOuts;

    /// <summary>
    /// Reference to the graphic that acts as a call-to-action when there's no active tangible.
    /// </summary>
    public CanvasGroupVisibility IntroText;

    /// <summary>
    /// Reference to the graphic that acts as a call-to-action when there is an active tangible;
    /// </summary>
    public CanvasGroupVisibility TutorialText;

    /// <summary>
    /// Stores the id of the active tangible. If this nullable is null, there's no active tangible.
    /// </summary>
    private int? _activeTangibleId;

    /// <summary>
    /// Stores the starting rotation of the active tangible.
    /// </summary>
    private float _startingRotation;
    
    void Start() {
      //Set the state of the UI to the 'no active tangible' state.
      DoDeactivateSequence(true);

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
      //Check to see if there's no active tangible.
      if (_activeTangibleId == null) {
        //If there's no active tangible then assign the _activeTangibleId property
        _activeTangibleId = t.Id;
        //store the tangibles starting rotation
        _startingRotation = t.R;
        //Reset the rotation indicators rotation
        RotationIndicator.localEulerAngles = new Vector3(0, 0, 180);
        //trigger the activate animation
        DoActivateSequence();
      }
    }

    private void TangibleEngine_OnTangibleRemoved(Tangible t) {
      //check to see if the tangible being removed is the active tangible
      if (_activeTangibleId == t.Id) {
        //null the active tangible
        _activeTangibleId = null;
        //trigger the deactivate animation
        DoDeactivateSequence();
      }
    }

    private void TangibleEngine_OnTangibleUpdated(Tangible t) {
      //compute the rotation delta (starting angle from the current angle)
      var deltaRotation = t.R - _startingRotation;
      //TE's unit for rotation is radians, but Unity's transform methods take the degrees unit. Convert to degrees from radians
      deltaRotation *= Mathf.Rad2Deg;
      // rotate the rotation indicator from its original state (180 degrees) to the desired rotation
      RotationIndicator.localEulerAngles = new Vector3(0, 0, deltaRotation + 180f);
    }

    /// <summary>
    /// Animate all of the UI elements into the activated state
    /// </summary>
    private void DoActivateSequence() {

      Te2LogoRing.ToA(0.3f);
      foreach (var g in Te2LogoDots) {
        g.ToA(0.3f);
      }
      RotationIndicatorGfx.FadeIn(0.3f);
      foreach (var popOut in PopOuts) {
        popOut.CanvasGroupVisibility.FadeIn(0.3f);
      }
      IntroText.FadeOut(0.3f, 0);
      TutorialText.FadeIn(0.3f);
    }

    /// <summary>
    /// Animate all of the UI elements into their de-activated state
    /// </summary>
    /// <param name="skip"></param>
    private void DoDeactivateSequence(bool skip = false) {
      var durationMultiplier = skip ? 0f : 1f;
      
      RotationIndicatorGfx.FadeOut(0.3f * durationMultiplier, 0);
      foreach (var popOut in PopOuts) {
        popOut.CanvasGroupVisibility.FadeOut(0.3f * durationMultiplier, 0);
      }
      foreach (var g in Te2LogoDots) {
        g.ToB(0.3f * durationMultiplier, 0);
      }
      Te2LogoRing.ToB(0.3f * durationMultiplier, 0);
      TutorialText.FadeOut(0.3f * durationMultiplier, 0f * durationMultiplier);
      IntroText.FadeIn(0.3f);
    }
  }
}
