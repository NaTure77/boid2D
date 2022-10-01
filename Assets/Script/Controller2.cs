using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Controller2 : MonoBehaviour
{
    public RawImage rawImage;

    public ComputeShader boidShader;

    ComputeBuffer boidBuffer;
    ComputeBuffer neighborCandidateBuffer;

    RenderTexture renderTexture;
    RenderTexture renderTexture_bef;

    int kernelID_calc;
    int kernelID_reset;
    int kernelID_resetImage;
    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;

        InitRenderTexture();
        InitNeighborCandidate();
        InitComputeShader();
        InitSortShader();
        StartCoroutine(UpdateLoop());
    }

    void SetValues()
    {
        boidShader.SetFloat("cohesionWeight", BoidInfo.instance.cohesionWeight);
        boidShader.SetFloat("alignmentWeight", BoidInfo.instance.alignmentWeight);
        boidShader.SetFloat("separationWeight", BoidInfo.instance.separationWeight);
        boidShader.SetFloat("speed", BoidInfo.instance.speed);
        boidShader.SetFloat("directionSenstivity", BoidInfo.instance.directionSenstivity);
    }

    struct int2
    {
        public int2(int x, int y) { this.x = x; this.y = y; }
        int x, y;
    };
    
    List<int2> candidates = new List<int2>();
    void InitNeighborCandidate()
    {
        int neighborRadius = BoidInfo.instance.neighborRadius;
        for(int i = -neighborRadius; i <= neighborRadius; i++)
        {
            for (int j = -neighborRadius; j <= neighborRadius; j++)
            {
                if(i * i + j * j <= neighborRadius * neighborRadius)
                {
                    candidates.Add(new int2(i,j));
                }
            }
        }

        neighborCandidateBuffer = new ComputeBuffer(candidates.Count, sizeof(int) * 2);
        neighborCandidateBuffer.SetData(candidates.ToArray());
        boidShader.SetInt("candidateCount", candidates.Count);
        boidShader.SetInt("neighborRadius", neighborRadius);
    }
    void InitComputeShader()
    {
        kernelID_calc = boidShader.FindKernel("CalculateNext");
        kernelID_reset = boidShader.FindKernel("ResetBef");
        kernelID_resetImage = boidShader.FindKernel("ResetBef_image");

        //resultTableBuffer = new ComputeBuffer(imageSize * imageSize, sizeof(int));
        //resultTableBefBuffer = new ComputeBuffer(imageSize * imageSize, sizeof(int));

        //boidShader.SetTexture(kernelID_reset, "Result", renderTexture);
        //boidShader.SetTexture(kernelID_reset, "Result_bef", renderTexture_bef);

        boidShader.SetTexture(kernelID_resetImage, "Result", renderTexture);
        boidShader.SetTexture(kernelID_resetImage, "Result_bef", renderTexture_bef);

        boidShader.SetTexture(kernelID_calc, "Result", renderTexture);
        boidShader.SetTexture(kernelID_calc, "Result_bef", renderTexture_bef);

        //boidShader.SetBuffer(kernelID_reset, "particles", boidBuffer);

        boidBuffer = new ComputeBuffer(BoidInfo.instance.particleCount, sizeof(float) * 4);
        boidBuffer.SetData(BoidInfo.instance.particles);
        boidShader.SetBuffer(kernelID_calc, "particles",boidBuffer);

        //boidShader.SetBuffer(kernelID_reset, "ResultTable", resultTableBuffer);
        //boidShader.SetBuffer(kernelID_reset, "ResultTable_bef", resultTableBefBuffer);
        //boidShader.SetBuffer(kernelID_calc, "ResultTable", resultTableBuffer);
        //boidShader.SetBuffer(kernelID_calc, "ResultTable_bef", resultTableBefBuffer);

        boidShader.SetBuffer(kernelID_calc, "neighborCandidate", neighborCandidateBuffer);

        int imageSize = BoidInfo.instance.imageSize;
        boidShader.SetInt("imageSize", imageSize);
        boidShader.SetInt("imageSize_half", imageSize/2);
        boidShader.SetInt("particleNum", BoidInfo.instance.particleCount);
    }
    void InitRenderTexture()
    {
        int imageSize = BoidInfo.instance.imageSize;
        renderTexture?.Release();
        renderTexture = new RenderTexture(imageSize, imageSize, 32);
        renderTexture.enableRandomWrite = true;
        renderTexture.filterMode = FilterMode.Point;
        renderTexture.Create();

        renderTexture_bef?.Release();
        renderTexture_bef = new RenderTexture(imageSize, imageSize, 32);
        renderTexture_bef.enableRandomWrite = true;
        renderTexture_bef.filterMode = FilterMode.Point;
        renderTexture_bef.Create();

        rawImage.texture = renderTexture_bef;
    }

    ComputeBuffer _keys;
    ComputeBuffer _values;
    ComputeBuffer _values_bef;
    ComputeBuffer _cellStart;
    public  ComputeShader bitonicSortShader;
    int _kernelSort, _kernelCellStart, _kernelInit;
    void InitSortShader()
    {
        _kernelInit = bitonicSortShader.FindKernel("InitKeys");
        _kernelSort = bitonicSortShader.FindKernel("BitonicSortInt");
        _kernelCellStart = bitonicSortShader.FindKernel("FindCellStart");

        int imageSize = BoidInfo.instance.imageSize;
        int particleCount = BoidInfo.instance.particleCount;
        _keys = new ComputeBuffer(particleCount, sizeof(int));
        _values = new ComputeBuffer(particleCount, sizeof(int));
        _values_bef = new ComputeBuffer(particleCount, sizeof(int));
        _cellStart = new ComputeBuffer(imageSize * imageSize, sizeof(int));

        _values_bef.SetData(BoidInfo.instance.cellID.ToArray());
        bitonicSortShader.SetInt("count", particleCount);
        bitonicSortShader.SetBuffer(_kernelInit, "Keys", _keys);
        

        bitonicSortShader.SetBuffer(_kernelSort, "Keys", _keys);
        bitonicSortShader.SetBuffer(_kernelSort, "Values", _values_bef);

        bitonicSortShader.SetBuffer(_kernelCellStart, "cellStart", _cellStart);
        bitonicSortShader.SetBuffer(_kernelCellStart, "Keys", _keys);
        bitonicSortShader.SetBuffer(_kernelCellStart, "Values", _values_bef);

        boidShader.SetBuffer(kernelID_calc, "cellStart", _cellStart);
        boidShader.SetBuffer(kernelID_calc, "keys", _keys);
        boidShader.SetBuffer(kernelID_calc, "particle_cellID", _values);
        boidShader.SetBuffer(kernelID_calc, "particle_cellID_bef", _values_bef);

        boidShader.SetBuffer(kernelID_reset, "particle_cellID", _values);
        boidShader.SetBuffer(kernelID_reset, "particle_cellID_bef", _values_bef);
        boidShader.SetBuffer(kernelID_resetImage, "cellStart", _cellStart);

    }
    void Sort()
    {
        int particleCount = BoidInfo.instance.particleCount;
        // init keys
        bitonicSortShader.Dispatch(_kernelInit, Mathf.CeilToInt(particleCount / 8f), 1, 1);
        for (var dim = 2; dim <= _keys.count; dim <<= 1)
        {
            bitonicSortShader.SetInt("dim", dim);
            for (var block = dim >> 1; block > 0; block >>= 1)
            {
                bitonicSortShader.SetInt("block", block);
                bitonicSortShader.Dispatch(_kernelSort, Mathf.CeilToInt(particleCount / 8f), 1, 1);
            }
        }
        bitonicSortShader.Dispatch(_kernelCellStart, Mathf.CeilToInt(particleCount / 8f), 1, 1);
    }
    public float frameSpeed = 0.01f;

    Vector3 mousePos_bef;
    IEnumerator UpdateLoop()
    {
        mousePos_bef = Input.mousePosition;
        int particleCount = BoidInfo.instance.particleCount;
        int imageSize = BoidInfo.instance.imageSize;
        while (true)
        {
            SetValues();
            Vector3 mousePos = Input.GetMouseButtonDown(0)? Input.mousePosition : Vector3.Lerp(mousePos_bef, Input.mousePosition, Time.deltaTime * 80);

            Vector2 mouseVector = mousePos - mousePos_bef;
            mousePos_bef = mousePos;
            mousePos -= new Vector3(Screen.width, Screen.height, 0) / 2;
            Vector2 mousePos2 = mousePos;

            bool mouseWork = (mousePos2.x < 512 && mousePos2.x >= -512 && mousePos2.y < 512 && mousePos2.y >= -512) && Input.GetMouseButton(0);

            boidShader.SetVector("mousePosition", mousePos2 * imageSize / 1024);
            boidShader.SetVector("mouseVector", mouseVector);
            boidShader.SetBool("mouseClicked", mouseWork);
            Sort();
            
            boidShader.Dispatch(kernelID_calc, Mathf.CeilToInt(particleCount / 8f), 1, 1);
            //boidShader.Dispatch(kernelID_reset, Mathf.CeilToInt(imageSize / 8f), Mathf.CeilToInt(imageSize / 8f), 1);
            boidShader.Dispatch(kernelID_resetImage, Mathf.CeilToInt(imageSize / 8f), Mathf.CeilToInt(imageSize / 8f), 1);
            boidShader.Dispatch(kernelID_reset, Mathf.CeilToInt(particleCount / 8f), 1, 1);
            yield return null;
        }
    }

    private void OnDestroy()
    {
        //if (!gameObject.activeSelf) return;
        boidBuffer.Release();
        _keys.Release();
        _values.Release();
        _values_bef.Release();
        _cellStart.Release();
        neighborCandidateBuffer.Release();
    }

    public void ExitApp()
    {
        Application.Quit();
    }
}

//- Cohesion: 주변 무리의 중심 방향으로 이동.  ((모든 이웃 boid의 위치 합 / 이웃 boid 개수) -현재 위치) *cohesionWeight;
//-Alignment: 무리가 향하는 평균 방향으로 이동.  (모든 이웃 boid의 방향 합 / 이웃 boid 개수) *alignmentWeight;
//-Separation: 뭉쳐있는 무리를 피해 이동. (모든 이웃의 위치 x에 대해 s += x - 현재 위치); s /= 이웃 boid 개수; return s * SeparationWeight;

