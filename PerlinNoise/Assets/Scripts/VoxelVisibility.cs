using UnityEngine;

public class VoxelVisibility : MonoBehaviour
{
    public float height, maxHeight;

    Material mat;

    public void Start() {
        if (Physics.Raycast(transform.position, Vector3.up, 1) && Physics.Raycast(transform.position, Vector3.down, 1) && Physics.Raycast(transform.position, Vector3.right, 1) && Physics.Raycast(transform.position, Vector3.left, 1) && Physics.Raycast(transform.position, Vector3.forward, 1) && Physics.Raycast(transform.position, Vector3.back, 1)) {
            GetComponent<MeshRenderer>().enabled = false;
        }

        if (GetComponent<MeshRenderer>().enabled) {
            mat = GetComponent<Renderer>().material;

            mat.color = GetHeatColor(height, maxHeight);
        }

    }

    public static Color GetHeatColor(float value, float maxHeight, int min = 0) {
        float t = Mathf.InverseLerp(min, maxHeight, value);
        float hue = Mathf.Lerp(0.66f, 0f, t); // 0.66 = blue, 0 = red
        return Color.HSVToRGB(hue, 1f, 1f);
    }

    void OnDestroy() {
        if (mat != null) Destroy(mat);
    }
}
