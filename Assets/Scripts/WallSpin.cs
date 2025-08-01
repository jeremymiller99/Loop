using UnityEngine;

public class WallSpin : MonoBehaviour
{
    [SerializeField] private float spinSpeed = 10f;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0, 0, spinSpeed * Time.deltaTime);
    }
}
