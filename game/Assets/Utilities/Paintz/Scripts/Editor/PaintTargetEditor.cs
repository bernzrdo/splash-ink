using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PaintTarget))]
public class PaintTargetEditor : Editor
{
    private static Texture2D logo;
    private GUIStyle guiStyle = new GUIStyle(); //create a new variable

    public override void OnInspectorGUI()
    {
        PaintTarget script = (PaintTarget)target;
        GameObject go = (GameObject)script.gameObject;
        Renderer render = go.GetComponent<Renderer>();

        //if (logo == null) logo = Resources.Load<Texture2D>("paintzlogo");

        //GUILayout.Space(8);

        //GUILayout.BeginHorizontal(GUI.skin.box);
        //Rect imgRect = GUILayoutUtility.GetRect(64, 32);
        //GUI.DrawTexture(imgRect, logo, ScaleMode.ScaleToFit, true);
        ////GUILayout.FlexibleSpace();

        //guiStyle.alignment = TextAnchor.MiddleCenter;
        //guiStyle.fontSize = 28;
        //guiStyle.fontStyle = FontStyle.Bold;
        //GUILayout.Label("Paintz", guiStyle);
        //GUILayout.FlexibleSpace();
        //GUILayout.EndHorizontal();

        if (Application.isPlaying)
        {
            GUILayout.BeginVertical(GUI.skin.box);

            //EditorGUILayout.ObjectField((Object)script.splatTexPick, typeof(Texture2D), true);

            script.PaintAllSplats = GUILayout.Toggle(script.PaintAllSplats, "Paint All Splats");

            if (GUILayout.Button("Clear Paint"))
            {
                script.ClearPaint();
            }
            if (GUILayout.Button("Clear All Paint"))
            {
                PaintTarget.ClearAllPaint();
            }

            GUILayout.EndVertical();

            guiStyle.alignment = TextAnchor.MiddleCenter;
            guiStyle.fontSize = 16;
            guiStyle.fontStyle = FontStyle.Bold;
            GUILayout.Label("SCORES", guiStyle);
            GUILayout.BeginVertical(GUI.skin.box);

            GUILayout.BeginHorizontal();
            GUILayout.Label(PaintTarget.scores.x.ToString());
            GUILayout.Label(PaintTarget.scores.y.ToString());
            GUILayout.Label(PaintTarget.scores.z.ToString());
            GUILayout.Label(PaintTarget.scores.w.ToString());
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Tally Scores"))
            {
                PaintTarget.TallyScore();
            }
            GUILayout.EndVertical();


            if (GUILayout.Button("Save Splat Texture"))
            {
                string Path = Application.dataPath + "/splat_" + Time.realtimeSinceStartup + ".renderTexture";

                Debug.Log("Splat Saved: " + Path);

                byte[] bytes = toTexture2D(script.splatTex).EncodeToPNG();
                System.IO.File.WriteAllBytes(Path, bytes);
                AssetDatabase.Refresh();
            }

        }
        else
        {
            GUILayout.BeginVertical(GUI.skin.box);

            script.paintTextureSize = (TextureSize)EditorGUILayout.EnumPopup("Paint Texture", script.paintTextureSize);
            script.SetupOnStart = GUILayout.Toggle(script.SetupOnStart, "Setup On Start");
            script.PaintAllSplats = GUILayout.Toggle(script.PaintAllSplats, "Paint All Splats");

            GUILayout.EndVertical();

            if (render == null)
            {
                EditorGUILayout.HelpBox("Missing Render Component", MessageType.Error);
            }
            else
            {
                foreach (Material mat in render.sharedMaterials)
                {
                    if (!mat.shader.name.Contains("Paint"))
                    {
                        EditorGUILayout.HelpBox(mat.name + "\nMissing Paint Shader", MessageType.Warning);
                    }
                }
            }

            bool foundCollider = false;
            bool foundMeshCollider = false;
            if (go.GetComponent<MeshCollider>())
            {
                foundCollider = true;
                foundMeshCollider = true;
            }
            if (go.GetComponent<BoxCollider>()) foundCollider = true;
            if (go.GetComponent<SphereCollider>()) foundCollider = true;
            if (go.GetComponent<CapsuleCollider>()) foundCollider = true;
            if (!foundCollider)
            {
                EditorGUILayout.HelpBox("Missing Collider Component", MessageType.Warning);
            }
            if (!foundMeshCollider)
            {
                EditorGUILayout.HelpBox("WARNING: Color Pick only works with Mesh Collider", MessageType.Warning);
            }
        }
    }

    Texture2D toTexture2D(RenderTexture rTex)
    {
        Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.ARGB32, false);
        RenderTexture.active = rTex;
        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();
        return tex;
    }
}