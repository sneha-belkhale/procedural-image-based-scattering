using System;

using System.Collections.Generic;
using UnityEngine;

public class ImageBasedScattering : MonoBehaviour
{
    [Header("Basic")]
    public Texture2D referenceTex;
    public Material cubeMat;
    public GameObject startingPosHelper;
    public int generatedCubeCount;

    [Header("Scattering")]
    [Range(0, 0.005f)]
    public float flowAmount = 0.0001f;
    public int stepSize;
    public float randomness;

    [Header("Scaling")]
    public int cubesToBalance;
    public bool cycleScale;

    [Header("Debug")]
    public bool showPath;

    private int[] _grid;
    private GameObject[] _placedCubes;
    private int[] _lastPlacedIndices;
    private float _lastKeyDownTime;
    private int _texWidth;
    private int _texHeight;

    // Start is called before the first frame update
    void Start()
    {
        _placedCubes = new GameObject[generatedCubeCount];
        _lastPlacedIndices = new int[cubesToBalance];

        for (int j = 0; j < generatedCubeCount; j++)
        {
            _placedCubes[j] = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _placedCubes[j].GetComponent<Renderer>().material = cubeMat;
        }

        _texWidth = referenceTex.width;
        _texHeight = referenceTex.height;
        _grid = new int[_texWidth * _texHeight];

        GenerateCubes();
        _lastKeyDownTime = Time.fixedTime;
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.fixedTime - _lastKeyDownTime < 0.5)
        {
            return;
        }

        if (showPath)
        {
            DrawPath();
        }

        if (Input.GetKey(KeyCode.G))
        {
            Debug.Log("Generating New Map");
            GenerateCubes();
            _lastKeyDownTime = Time.fixedTime;
        }

    }

    void OnValidate()
    {
        if (_lastKeyDownTime>1f)
        {
            GenerateCubes();
        }
    }

    void DrawPath()
    {
        Vector3 lastPos = _placedCubes[0].transform.position;
        for (int i = 1; i < generatedCubeCount-1; i++)
        {
            if(_placedCubes[i].activeSelf)
            {
                Vector3 dir = _placedCubes[i].transform.position - lastPos;
                Debug.DrawRay(lastPos, dir, Color.green, 0.5f);
                lastPos = _placedCubes[i].transform.position;
            }
        }
    }

    void ResetGrid()
    {
        for (int i = 0; i < _texWidth * _texHeight; i++)
        {
            _grid[i] = 0;
        }
    }

    void ResetLastPlacedIndices()
    {
        for (int i = 0; i < cubesToBalance; i++)
        {
            _lastPlacedIndices[i] = 0;
        }
    }

    bool LastPlacedIndicesSet()
    {
        for (int i = 0; i < cubesToBalance; i++)
        {
            if(_lastPlacedIndices[i] == 0)
            {
                return false;
            }
        }
        return true;
    }

    void AddLastPlacedIndex(int newIdx)
    {
        for (int i = cubesToBalance - 1; i > 0; i--)
        {
            _lastPlacedIndices[i] = _lastPlacedIndices[i-1];
        }
        _lastPlacedIndices[0] = newIdx;
    }

    Vector3 GetLastPlacedCenter(Vector3 nextPos)
    {
        Vector3 center = Vector3.zero;
        for (int i = 0; i < cubesToBalance; i++)
        {
            center += _placedCubes[_lastPlacedIndices[i]].transform.position;
        }
        center += nextPos;
        center /= (cubesToBalance + 1);
        return center;
    }

    Vector3 GetLastPlacedVector(Vector3 center)
    {
        Vector3 weightedVec = Vector3.zero;
        for (int i = 0; i < cubesToBalance; i++)
        {
            int lastPlacedIdx = _lastPlacedIndices[i];
            weightedVec += _placedCubes[lastPlacedIdx].transform.localScale.x * (_placedCubes[lastPlacedIdx].transform.position - center);
        }
        return weightedVec;
    }

    Vector2 WorldToUV(Vector3 worldPos)
    {
        return new Vector2(-worldPos.x + _texWidth / 2f, -worldPos.z + _texHeight / 2f);
    }

    Vector3 UVToWorld(Vector2 uvPos)
    {
        return new Vector3(-uvPos.x + _texWidth / 2f, 2f, -uvPos.y + _texHeight / 2f);
    }

    void GenerateCubes()
    {

        UnityEngine.Random.InitState(101);

        ResetGrid();
        ResetLastPlacedIndices();

        Vector2 startingPos = WorldToUV(startingPosHelper.transform.position);

        Vector2Int nextUv = Vector2Int.FloorToInt(startingPos);
        Vector2Int lastDir = new Vector2Int(1, 1);

        for (int i = 0; i < generatedCubeCount; i++)
        {
            float minDist = 1000f;
            Vector2Int minUv = Vector2Int.one;
            Color texCol = referenceTex.GetPixel(nextUv.x, nextUv.y);
            //get neighboring pixels
            for (int j = -1; j < 2; j++)
            {
                for (int k = -1; k < 2; k++)
                {
                    if (j == 0 && k == 0)
                    {
                        continue;
                    }
                    int x1 = nextUv.x + j * stepSize + Mathf.FloorToInt(UnityEngine.Random.Range(0, randomness));
                    int y1 = nextUv.y + k * stepSize + Mathf.FloorToInt(UnityEngine.Random.Range(0, randomness));
                    Color texCol1 = referenceTex.GetPixel(x1, y1);
                   
                    //color similarity portion
                    float dist = Mathf.Pow(texCol.r - texCol1.r, 2.0f) +
                        Mathf.Pow(texCol.g - texCol1.g, 2.0f) +
                        Mathf.Pow(texCol.b - texCol1.b, 2.0f);

                    //flow portion 
                    Vector2Int dir = new Vector2Int(x1, y1) - nextUv;
                    float dot = Vector2.Dot(dir, lastDir);
                    dot /= (stepSize * stepSize);
                    dist -= flowAmount * dot;

                    if (dist < minDist)
                    {
                        minDist = dist;
                        minUv.x = x1;
                        minUv.y = y1;
                    }
                }
            }

            lastDir = minUv - nextUv;
            nextUv = minUv;

            bool overlap = false;
            Vector3 nextPos = UVToWorld(nextUv);

            //determine scale
            float scale = 0;
            if(!LastPlacedIndicesSet() || cycleScale)
            {
                scale = 0.25f * stepSize * (i%5 + 1);
            }
            else
            {
                Vector3 center = GetLastPlacedCenter(nextPos);
                Vector3 vNext = nextPos - center;
                Vector3 vTotal = GetLastPlacedVector(center);

                scale = Mathf.Sqrt(Vector3.Dot(vTotal, vTotal) / Vector3.Dot(vNext, vNext));
                scale += UnityEngine.Random.Range(0, 0.2f * scale); 
            }

            //determine if there is any overlap within the scale 
            for (int j = - Mathf.RoundToInt(scale / 2); j < Mathf.RoundToInt(scale / 2); j++)
            {
                for (int k = - Mathf.RoundToInt(scale / 2); k < Mathf.RoundToInt(scale / 2); k++)
                {
                    int idx = Mathf.Clamp(_texWidth * (nextUv.y + k) + (nextUv.x + j), 0, _texWidth * _texHeight - 1);
                    int t = _grid[idx];
                    if(t == 1)
                    {
                        overlap = true;
                    }
                    else
                    {
                        _grid[idx] = 1;
                    }
                }
            }
            if (!overlap)
            {
                _placedCubes[i].SetActive(true);
                _placedCubes[i].transform.position = nextPos;
                _placedCubes[i].transform.localScale = scale * Vector3.one;
                _placedCubes[i].GetComponent<Renderer>().material.SetColor("_Color", texCol);
                AddLastPlacedIndex(i);
            }
            else
            {
                _placedCubes[i].SetActive(false);
            }
        }
    }
}
