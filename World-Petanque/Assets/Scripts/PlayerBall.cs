using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class PlayerBall : MonoBehaviour
{
    public bool wasThrown = false;

    private XRGrabInteractable grab;

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
    }

    void OnEnable()
    {
        grab.selectExited.AddListener(OnReleased);
    }

    void OnDisable()
    {
        grab.selectExited.RemoveListener(OnReleased);
    }

    void OnReleased(SelectExitEventArgs args)
    {
        wasThrown = true;
        Debug.Log("Speler heeft gegooid!");
    }

    public void ResetBall()
    {
        wasThrown = false;
        GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
    }
}