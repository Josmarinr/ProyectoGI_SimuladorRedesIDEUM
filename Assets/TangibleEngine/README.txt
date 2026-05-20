Tangible Engine v2
==================

Example scenes for Tangible Engine 2.

These examples (1-5) include the use of Touchscript rather than Unity's built in touch input.

Touchscript allows for the use of multiple touch sensors and multiple touch screens to display TE as one instance. 

The key in making Touchscript work with TE is to edit the TouchPointProvider.cs and change the touch input to use Touchscripts input system: var touches = TouchScript.TouchManager.Instance.Pointers;

You can read more about this functionality here: https://ideum.com/news/tangible-engine-multiple-screen-support

There is also a pdf called "Tangible Engine with Multiple Dislays" that explains how to set up the hardware along with the software.
