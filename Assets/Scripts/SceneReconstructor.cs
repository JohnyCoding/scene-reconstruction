using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SceneReconstructor : MonoBehaviour
{
    public GameObject[] referenceObjects;
    public GameObject itemObjectPrefab;
    public Button resetButton;
    public TextMeshProUGUI alertText;

    bool raycastingEnabled = false;
    bool ref1Set;
    bool ref2Set;
    Camera mainCamera;
    GameObject ref1EmptyObj;
    SceneObject sceneObject;
    List<GameObject> itemObjects;
    Vector3 ref1SetPos;
    Vector3 ref2SetPos;

    void Awake()
    {
        mainCamera = Camera.main;
        itemObjects = new List<GameObject>();
        resetButton.onClick.AddListener(ResetScene);
        // This object will be used to apply the rotation
        ref1EmptyObj = Instantiate(new GameObject(), referenceObjects[0].transform, false);
        // Sample scene object for testing purposes
        sceneObject = new SceneObject
        {
            id = "1",
            name = "Scene1",
            referencePoint1 = new Vector3(-1f, 0f, 0f),
            referencePoint2 = new Vector3(1f, 0f, 0f),
            items = new SceneItem[]{
                new SceneItem{name = "Item1", color = "#FD0000", position = new Vector3(0f, 0f, 3f)},
                new SceneItem{name = "Item2", color = "#07FF00", position = new Vector3(2f, 0f, 1.5f)},
                new SceneItem{name = "Item3", color = "#001FFF", position = new Vector3(-2f, 0f, 1.5f)}
            }
        };
        SetupSceneInitial();
        ReconstructScene(new Vector3[] { new Vector3(-1f, 0f, 0f), new Vector3(1f, 0f, 0f) });
    }

    void Update()
    {
        if (!raycastingEnabled) return;

        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 100.0f))
            {
                SetReferencePointAtPosition(hit.point);
            }
        }
    }

    void SetupSceneInitial()
    {
        referenceObjects[0].transform.position = sceneObject.referencePoint1;
        referenceObjects[1].transform.position = sceneObject.referencePoint2;
        referenceObjects[0].SetActive(false);
        referenceObjects[1].SetActive(false);
        for (int i = 0; i < sceneObject.items.Length; i++)
        {
            GameObject itemObjectInst = Instantiate(itemObjectPrefab, sceneObject.items[i].position, Quaternion.identity, ref1EmptyObj.transform);
            itemObjects.Add(itemObjectInst);
            itemObjectInst.SetActive(false);
        }
    }

    void ResetScene()
    {
        raycastingEnabled = true;
        referenceObjects[0].SetActive(false);
        referenceObjects[1].SetActive(false);
        ref1Set = false;
        ref2Set = false;
        foreach (GameObject item in itemObjects)
        {
            Destroy(item);
        }
        itemObjects = new List<GameObject>();
        SetupSceneInitial();
        alertText.text = "Place First Reference Point";
    }

    void SetReferencePointAtPosition(Vector3 position)
    {
        if (!ref1Set)
        {
            ref1SetPos = position;
            ref1Set = true;
            SetupReferencePointData(referenceObjects[0], ref1SetPos, "Reference Point 1");
            alertText.text = "Place Second Reference Point";
            return;
        }
        if (!ref2Set)
        {
            ref2SetPos = position;
            ref2Set = true;
            SetupReferencePointData(referenceObjects[1], ref2SetPos, "Reference Point 2");
        }
        if (ref1Set && ref2Set)
        {
            raycastingEnabled = false;
            ReconstructScene(new Vector3[] { ref1SetPos, ref2SetPos });
        }
    }

    void ReconstructScene(Vector3[] newReferencePoints)
    {
        if (newReferencePoints.Length < 2)
        {
            Debug.LogError("Not enough reference points");
            return;
        }

        Vector3 ref1Pos = newReferencePoints[0];
        Vector3 ref2Pos = newReferencePoints[1];
        SetupReferencePointData(referenceObjects[0], ref1Pos, "Reference Point 1");
        SetupReferencePointData(referenceObjects[1], ref2Pos, "Reference Point 2");

        Vector3 v1 = sceneObject.referencePoint2 - sceneObject.referencePoint1;
        Vector3 v2 = ref2Pos - ref1Pos;
        float scaleRatio = v2.sqrMagnitude / v1.sqrMagnitude;
        Quaternion rotation = Quaternion.FromToRotation(v1, v2);

        for (int i = 0; i < itemObjects.Count; i++)
        {
            Transform itemObjectTransform = itemObjects[i].transform;
            itemObjectTransform.position *= scaleRatio;
            itemObjectTransform.parent.rotation = rotation;
            SetupItemData(itemObjects[i], itemObjectTransform.position, sceneObject.items[i].name, sceneObject.items[i].color);
        }
        alertText.text = "Reconstruction Done";
    }

    void SetupReferencePointData(GameObject referenceObject, Vector3 position, string name)
    {
        referenceObject.SetActive(true);
        referenceObject.transform.position = position;
        referenceObject.name = name;
        TextMeshPro textMeshPro = referenceObject.GetComponentInChildren<TextMeshPro>();
        textMeshPro.text = $"{name}\n{position:G}";

        Transform textMeshProTransform = textMeshPro.transform;
        textMeshProTransform.LookAt(mainCamera.transform);
        textMeshProTransform.RotateAround(textMeshProTransform.position, textMeshProTransform.up, 180f);
    }

    void SetupItemData(GameObject itemObject, Vector3 position, string name, string colorHex)
    {
        itemObject.SetActive(true);
        Color colorFromHex;
        ColorUtility.TryParseHtmlString(colorHex, out colorFromHex);
        Renderer renderer = itemObject.GetComponent<Renderer>();
        renderer.material.SetColor("_Color", colorFromHex);
        itemObject.name = name;
        TextMeshPro textMeshPro = itemObject.GetComponentInChildren<TextMeshPro>();
        textMeshPro.text = $"{name}\n{position:G}";

        Transform textMeshProTransform = textMeshPro.transform;
        textMeshProTransform.LookAt(mainCamera.transform);
        textMeshProTransform.RotateAround(textMeshProTransform.position, textMeshProTransform.up, 180f);
    }
}
