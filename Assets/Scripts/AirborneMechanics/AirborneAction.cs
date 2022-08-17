using UnityEngine;
public class AirborneAction
{
    protected Rigidbody2D playerBody;
    public AirborneAction(Rigidbody2D playerBody)
    {
        this.playerBody = playerBody;
    }

    public virtual void StartAction()
    {

    }
    public virtual void PerformAction()
    {

    }
    public virtual void ResetAction()
    {

    }
}