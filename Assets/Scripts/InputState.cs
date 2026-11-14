using UnityEngine;
namespace GrandLife {
public static class InputState {
    public static Vector2 Move;      // from the touch joystick
    public static bool Brake;        // from the touch brake button
    public static Vector2 CurrentMove {
        get { var k = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
              return Move.sqrMagnitude > 0.01f ? Move : k; }
    }
    public static bool BrakeNow => Brake || Input.GetKey(KeyCode.Space);
}}
