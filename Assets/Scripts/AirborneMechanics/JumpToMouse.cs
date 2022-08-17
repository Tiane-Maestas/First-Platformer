using UnityEngine;
public class JumpToMouse : AirborneAction
{
    private float _forceMagnitude;
    private bool _SecondJumpAllowed = true;
    public GameObject guideLine;
    public LineRenderer guideRenderer;
    public JumpToMouse(Rigidbody2D playerBody, float jumpForce) : base(playerBody)
    {
        _forceMagnitude = jumpForce;
        guideLine = new GameObject("JumpToMouse Guide Line");
        guideLine.AddComponent<LineRenderer>();
        guideRenderer = guideLine.GetComponent<LineRenderer>();
        guideRenderer.SetWidth(0.1f, 0.1f);
        // lineR.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
        // lineR.SetColors(color, color);
        // GameObject.Destroy(line, 0.001f);
    }

    public override void PerformAction()
    {
        base.PerformAction();
        if (_SecondJumpAllowed)
        {
            // Draw Line to Mouse To Guide the player.
            guideLine.transform.position = playerBody.position;
            guideRenderer.SetPosition(0, playerBody.position);
            guideRenderer.SetPosition(1, Camera.main.ScreenToWorldPoint(Input.mousePosition));

            // Vector to jump along. Normalized for direction only.
            Vector2 jumpVector = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition) - playerBody.position;
            jumpVector.Normalize();
            // Do Jump
            if (Input.GetMouseButton(0))
            {
                playerBody.AddForce(jumpVector * _forceMagnitude);
                _SecondJumpAllowed = false;
            }
        }
    }
    public override void ResetAction()
    {
        base.ResetAction();
        _SecondJumpAllowed = true;
    }
}