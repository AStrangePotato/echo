using UnityEngine;
using UnityEditor;

public class GLBMaterialFixer : MonoBehaviour
{
    [MenuItem("Tools/Replace GLB Materials with Editable Lit")]
    static void ReplaceMaterials()
    {
        if (Selection.activeGameObject == null)
        {
            Debug.LogWarning("Select a GLB root object in the hierarchy first.");
            return;
        }

        GameObject root = Selection.activeGameObject;
        MeshRenderer[] renderers = root.GetComponentsInChildren<MeshRenderer>();

        foreach (MeshRenderer renderer in renderers)
        {
            Material[] mats = renderer.sharedMaterials;
            Material[] newMats = new Material[mats.Length];

            for (int i = 0; i < mats.Length; i++)
            {
                Material mat = mats[i];
                if (mat == null) continue;

                // Create a copy
                Material newMat = new Material(mat);
                newMat.name = mat.name + "_EditableLit";

                // Switch shader to Standard/Lit
                newMat.shader = Shader.Find("Standard");

                // Disable emission if it exists
                if (newMat.IsKeywordEnabled("_EMISSION"))
                {
                    newMat.DisableKeyword("_EMISSION");
                    newMat.SetColor("_EmissionColor", Color.black);
                }

                newMats[i] = newMat;
            }

            renderer.materials = newMats; // Assign editable materials
        }

        Debug.Log("All materials replaced with editable Lit versions.");
    }
}
