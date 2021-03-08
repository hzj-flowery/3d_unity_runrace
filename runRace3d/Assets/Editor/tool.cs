using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public enum AlignType
{
    Top = 1,
    Left = 2,
    Right = 3,
    Bottom = 4,
    HorizontalCenter = 5,       //水平居中
    VerticalCenter = 6,         //垂直居中
    Horizontal = 7,             //横向分布
    Horizontal_Bottom = 8,      //横向底部对齐
    Horizontal_Top = 9,         //横向顶部对齐
    Vertical = 10,               //纵向分布
    Vertical_Left = 11,          //纵向左对齐
    Vertical_Right = 12,        //纵向右对齐
}

struct Material_Arr {
    public static string defaultMaterial = "bg_lv";
}

public class NewBehaviourScript : MonoBehaviour {

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    [MenuItem("MyTool/DeleteAllObj", true)]
    private static bool DeleteValidate()
    {
        if (Selection.objects.Length > 0)
            return true;
        else
            return false;
    }

    [MenuItem("MyTool/DeleteAllObj", false)]
    private static void MyToolDelete()
    {
        //Selection.objects 返回场景或者Project中选择的多个对象
        foreach (Object item in Selection.objects)
        {
            //记录删除操作，允许撤销
            Undo.DestroyObjectImmediate(item);
        }
    }

    [MenuItem("MyTool/吸附 横向底部对齐 ")]
    static void AdsorbHorizontalBotttom()
    {
        Adsorb(AlignType.Horizontal_Bottom);
    }
    [MenuItem("MyTool/吸附 横向顶部对齐 ")]
    static void AdsorbHorizontalTop()
    {
        Adsorb(AlignType.Horizontal_Top);
    }
    [MenuItem("MyTool/吸附 纵向左对齐 ")]
    static void AdsorbVerticalLeft()
    {
        Adsorb(AlignType.Vertical_Left);
    }
    [MenuItem("MyTool/吸附 纵向右对齐 ")]
    static void AdsorbVerticalRight()
    {
        Adsorb(AlignType.Vertical_Right);
    }
    
	[MenuItem("MyTool/ChangeNewMat")]
    static void ChangeNewMat()
    {
        GameObject[] objs = Selection.gameObjects;
        for (int j = 0; j < objs.Length; j++)
        {
            Renderer[] objRendder = objs[j].transform.GetComponentsInChildren<Renderer>();
            Material mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/runraceExport/material/"+ Material_Arr.defaultMaterial+".mat");
            objRendder[0].material = mat;
        }
        
    }
    [MenuItem("MyTool/创建预制体")]
    static void CreatePrefab()
    {
        if (!Directory.Exists(Application.dataPath + "/prefabs"))
        {
            AssetDatabase.CreateFolder("Assets/runraceExport", "prefabs");
            AssetDatabase.Refresh();
        }
        string[] prefabIds = AssetDatabase.FindAssets("t:Prefab", new string[] { "Assets/runraceExport/prefabs" });
        string[] prefabNames = new string[prefabIds.Length];
        for (int i = 0; i < prefabIds.Length; i++)
        {
            string pName = AssetDatabase.GUIDToAssetPath(prefabIds[i]);
            pName = pName.Split(new string[] { "Assets/runraceExport/prefabs/", ".prefab" }, System.StringSplitOptions.RemoveEmptyEntries)[0];
            prefabNames[i] = pName;
        }

        GameObject[] selectObjs = Selection.gameObjects;
        for (int i = 0; i < selectObjs.Length; i++)
        {
            //Debug.Log(selectObjs[i].name);
            //PrefabUtility.CreatePrefab("Assets/Prefabs/" + selectObjs[i].name + ".prefab", selectObjs[i]);

            string objName = selectObjs[i].name;
            objName = objName.Split(' ')[0];
            bool isHave = false;
            for (int j = 0; j < prefabNames.Length; j++)
            {
                if (string.Equals(objName, prefabNames[j]))
                {
                    isHave = true;
                    break;
                }
            }

            if (isHave)
            {
                Debug.Log(objName);
                GameObject pObj = AssetDatabase.LoadAssetAtPath("Assets/runraceExport/prefabs/" + objName + ".prefab", typeof(GameObject)) as GameObject;
                Vector3 p = selectObjs[i].transform.position;
                Vector3 r = selectObjs[i].transform.eulerAngles;
                Vector3 s = selectObjs[i].transform.localScale;
                string oName = selectObjs[i].name;

                GameObject cP = PrefabUtility.ConnectGameObjectToPrefab(selectObjs[i], pObj);
                cP.name = oName;
                cP.transform.position = p;
                cP.transform.eulerAngles = r;
                cP.transform.localScale = s;
            }
            else
            {
                var newprefab = PrefabUtility.CreateEmptyPrefab("Assets/runraceExport/prefabs/" + objName + ".prefab");
                PrefabUtility.ReplacePrefab(selectObjs[i], newprefab, ReplacePrefabOptions.ConnectToPrefab);
            }
        }
        AssetDatabase.Refresh();
    }



    public static void Adsorb(AlignType type)
    {
        GameObject[] objects = Selection.gameObjects;
        int len = objects[0].transform.childCount;
        Transform parentObj = objects[0].transform;
        GameObject[] objectsChild = new GameObject[len];
        for (int j = 0; j < len; j++)
        {
            objectsChild[j] = parentObj.GetChild(j).gameObject;
        }

       

        if (objectsChild != null && objectsChild.Length > 0)
        {
            //吸附 3d
            GameObject firstObj = objectsChild[0];
            int firstPos = 0;

            
            for (int i = 0; i < objectsChild.Length; i++)
            {
                if (objectsChild[i].name.IndexOf("target") >= 0)
                {
                    firstObj = objectsChild[i];
                    firstPos = i;
                    break;
                }
            }
            BoxCollider box = firstObj.GetComponentInChildren<BoxCollider>();
            List<Vector3> posArr = new List<Vector3>();

            float firstSize_w = box.size.x * box.transform.localScale.x;
            float firstSize_h = box.size.y * box.transform.localScale.y;
            float firstPos_x = firstObj.transform.localPosition.x + firstSize_w / 2;//x的最右边缘
            float firstPos_y = firstObj.transform.localPosition.y - firstSize_h / 2;//y的最上边缘
                                                                                    
            for (int i = 0; i < objectsChild.Length; i++)
            {
                if (i == firstPos)
                {
                    continue;
                }
                GameObject curObj = objectsChild[i];
                BoxCollider curBox = curObj.GetComponentInChildren<BoxCollider>();
                float temp_w = curBox.size.x * curBox.transform.localScale.x;//
                float temp_h = curBox.size.y * curBox.transform.localScale.y;
                if (type == AlignType.Horizontal_Bottom)
                {
                    //横向底部对齐
                    float newOffY = (firstSize_h - temp_h) / 2;
                    curObj.transform.localPosition = new Vector3(firstPos_x + temp_w / 2, firstObj.transform.localPosition.y - newOffY, firstObj.transform.localPosition.z);
                    firstPos_x = firstPos_x + temp_w;
                }
                else if (type == AlignType.Horizontal_Top)
                {
                    //横向顶部对齐
                    float newOffY = (firstSize_h - temp_h) / 2;
                    curObj.transform.localPosition = new Vector3(firstPos_x + temp_w / 2, firstObj.transform.localPosition.y + newOffY, firstObj.transform.localPosition.z);
                    firstPos_x = firstPos_x + temp_w;
                }
                else if (type == AlignType.Vertical_Left)
                {
                    //纵向左对齐
                    float newOffX = (firstSize_w - temp_w) / 2;
                    curObj.transform.localPosition = new Vector3(firstObj.transform.localPosition.x - newOffX, firstPos_y - temp_h / 2, firstObj.transform.localPosition.z);
                    firstPos_y = firstPos_y - temp_h;
                }
                else if (type == AlignType.Vertical_Right)
                {
                    //纵向右对齐
                    float newOffX = (firstSize_w - temp_w) / 2;
                    curObj.transform.localPosition = new Vector3(firstObj.transform.localPosition.x + newOffX, firstPos_y - temp_h / 2, firstObj.transform.localPosition.z);
                    firstPos_y = firstPos_y - temp_h;
                }
            }
        }
    }

    
    }

