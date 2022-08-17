using UnityEngine;
public class Glide : AirborneAction
{
    private float _drag;
    private PlayerController controller;
    private float _defaultControllForce;
    public Glide(Rigidbody2D playerBody, float drag) : base(playerBody)
    {
        this._drag = drag;
        controller = playerBody.GetComponent<PlayerController>();
        _defaultControllForce = controller.airborneControlForce;
        Debug.Log(_defaultControllForce);
    }

    public override void PerformAction()
    {
        base.PerformAction();
        if (Input.GetMouseButton(0) && playerBody.velocity.y <= 0)
        {
            playerBody.drag = this._drag;
            controller.airborneControlForce = (4 * _defaultControllForce);
        }
        else
        {
            playerBody.drag = 0f;
        }
    }
    public override void ResetAction()
    {
        base.ResetAction();
        playerBody.drag = 0f;
    }
}