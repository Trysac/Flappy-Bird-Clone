using System;
using UnityEngine;

namespace FlappyDragon.Game
{
    /// <summary>
    /// Reads the player's actions from keyboard, mouse, touch and gamepad (legacy Input Manager,
    /// which works the same in the desktop and the WebGL players).
    /// </summary>
    public sealed class GameInput
    {
        /// <summary>Returns true when a screen position is over an on-screen button.</summary>
        readonly Func<Vector2, bool> isOverButton;

        public GameInput(Func<Vector2, bool> isOverButton) => this.isOverButton = isOverButton;

        /// <summary>Space, Up, W, a click or tap on the playfield, or the gamepad's south button.</summary>
        public bool Flap()
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)
                || Input.GetKeyDown(KeyCode.JoystickButton0))
            {
                return true;
            }
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                if (touch.phase == TouchPhase.Began && !isOverButton(touch.position))
                {
                    return true;
                }
            }
            // Touches also raise mouse events; only count the mouse when nothing touched the screen.
            return Input.touchCount == 0 && Input.GetMouseButtonDown(0) && !isOverButton(Input.mousePosition);
        }

        /// <summary>F, X, a right click, or the gamepad's west button.</summary>
        public bool Fire() => Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.X) || Input.GetMouseButtonDown(1)
            || Input.GetKeyDown(KeyCode.JoystickButton2);

        /// <summary>Escape, P, or the gamepad's start button.</summary>
        public bool Pause() => Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P)
            || Input.GetKeyDown(KeyCode.JoystickButton7);

        /// <summary>Enter or the gamepad's south button confirm the highlighted menu choice.</summary>
        public bool Confirm() => Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)
            || Input.GetKeyDown(KeyCode.JoystickButton0);
    }
}
