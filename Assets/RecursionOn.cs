using UnityEngine;
using UnityEditor;

public class GLBEnableEmission : MonoBehaviour
{
    [MenuItem("Tools/Enable Emission on GLB Materials")]
    static void EnableEmission()
    {
        if (Selection.activeGameObject == null)
        {
            Debug.LogWarning("Select the root GameObject of your GLB first.");
            return;
        }

        GameObject root = Selection.activeGameObject;
        MeshRenderer[] renderers = root.GetComponentsInChildren<MeshRenderer>();

        foreach (MeshRenderer renderer in renderers)
        {
            Material[] mats = renderer.sharedMaterials;

            for (int i = 0; i < mats.Length; i++)
            {
                Material mat = mats[i];
                if (mat == null) continue;

                // Make sure the material is editable
                Material newMat = new Material(mat);
                newMat.name = mat.name + "_EmissionEnabled";

                // Enable emission and set a color
                newMat.EnableKeyword("_EMISSION");
                newMat.SetColor("_EmissionColor", Color.white); // Change color as needed
                newMat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;

                // Assign back to renderer
                mats[i] = newMat;
            }

            renderer.materials = mats;
        }

        Debug.Log("Emission enabled on all materials.");
    }
}
