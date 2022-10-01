using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Controller : MonoBehaviour
{
    public RawImage rawImage;

    public ComputeShader boidShader;

    ComputeBuffer boidBuffer;
    ComputeBuffer resultTableBuffer;
    ComputeBuffer resultTableBefBuffer;
    ComputeBuffer neighborCandidateBuffer;

    RenderTexture renderTexture;
    RenderTexture renderTexture_bef;
    

    int kernelID_calc;
    int kernelID_makeImage;
    int kernelID_resetBef;
    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;
        InitRenderTexture();
        InitNeighborCandidate();
        InitComputeShader();
       
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
                if(i * i + j * j <= neighborRadius * neighborRadius && !(i == 0 && j == 0))
                {
                    Debug.Log(i + ", " + j);
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
        kernelID_makeImage = boidShader.FindKernel("MakeImage");
        kernelID_resetBef = boidShader.FindKernel("ResetBef");

        int imageSize = BoidInfo.instance.imageSize;
        boidBuffer = new ComputeBuffer(BoidInfo.instance.particleCount, sizeof(float) * 4);
        resultTableBuffer = new ComputeBuffer(imageSize * imageSize, sizeof(int));
        resultTableBefBuffer = new ComputeBuffer(imageSize * imageSize, sizeof(int));

        boidBuffer.SetData(BoidInfo.instance.particles);
        boidShader.SetTexture(kernelID_makeImage, "Result", renderTexture);
        boidShader.SetTexture(kernelID_makeImage, "Result_bef", renderTexture_bef);


        boidShader.SetTexture(kernelID_calc, "Result", renderTexture);
        boidShader.SetTexture(kernelID_calc, "Result_bef", renderTexture_bef);

        boidShader.SetBuffer(kernelID_makeImage, "particles", boidBuffer);
        boidShader.SetBuffer(kernelID_calc, "particles", boidBuffer);

        boidShader.SetBuffer(kernelID_makeImage, "ResultTable", resultTableBuffer);
        boidShader.SetBuffer(kernelID_makeImage, "ResultTable_bef", resultTableBefBuffer);
        boidShader.SetBuffer(kernelID_calc, "ResultTable", resultTableBuffer);
        boidShader.SetBuffer(kernelID_calc, "ResultTable_bef", resultTableBefBuffer);

        boidShader.SetBuffer(kernelID_calc, "neighborCandidate", neighborCandidateBuffer);
        boidShader.SetBuffer(kernelID_makeImage, "neighborCandidate", neighborCandidateBuffer);


        boidShader.SetTexture(kernelID_resetBef, "Result", renderTexture);

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

    public float frameSpeed = 0.01f;

    Vector2 mousePos_bef;
    public TextMeshProUGUI mouseText;
    IEnumerator UpdateLoop()
    {
        int imageSize = BoidInfo.instance.imageSize;
        mousePos_bef = Input.mousePosition;
        while (true)
        {
           // mouseText.text = "Mouse Position\n" + Input.mousePosition.x + ",  " + Input.mousePosition.y;
           // Vector2 m = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            SetValues();
            Vector2 mousePos = Input.GetMouseButtonDown(0)? Input.mousePosition : Vector3.Lerp(mousePos_bef, Input.mousePosition, Time.deltaTime * 80);

            Vector2 mouseVector = mousePos - mousePos_bef;
            mousePos_bef = mousePos;
            mousePos -= new Vector2(Screen.width, Screen.height) / 2;

            bool mouseWork = (mousePos.x < 512 && mousePos.x >= -512 && mousePos.y < 512 && mousePos.y >= -512) && Input.GetMouseButton(0);

            boidShader.SetVector("mousePosition", mousePos * imageSize / 1024f * 1920f / Screen.width);
            boidShader.SetVector("mouseVector", mouseVector);
            boidShader.SetBool("mouseClicked", mouseWork);
            
            boidShader.Dispatch(kernelID_calc, Mathf.CeilToInt(BoidInfo.instance.particleCount / 8f), 1, 1);
            boidShader.Dispatch(kernelID_makeImage, Mathf.CeilToInt(imageSize / 8f), Mathf.CeilToInt(imageSize / 8f), 1);

            //Graphics.Blit(renderTexture_bef, renderTexture);
           // boidShader.Dispatch(kernelID_makeImage, Mathf.CeilToInt(imageSize / 8f), Mathf.CeilToInt(imageSize / 8f), 1);
            boidShader.Dispatch(kernelID_resetBef, Mathf.CeilToInt(imageSize / 8f), Mathf.CeilToInt(imageSize / 8f), 1);
            yield return null;
        }
    }

    private void OnDestroy()
    {
        boidBuffer.Release();
        resultTableBuffer.Release();
        resultTableBefBuffer.Release();
        neighborCandidateBuffer.Release();
    }

    public void SetFullModeScreen()
    {
        Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, FullScreenMode.FullScreenWindow);
    }
    public void SetWindowModeScreen()
    {
        Screen.SetResolution(1280, 720, FullScreenMode.Windowed);
    }
    public void ExitApp()
    {
        Application.Quit();
    }
}

//- Cohesion: 주변 무리의 중심 방향으로 이동.  ((모든 이웃 boid의 위치 합 / 이웃 boid 개수) -현재 위치) *cohesionWeight;
//-Alignment: 무리가 향하는 평균 방향으로 이동.  (모든 이웃 boid의 방향 합 / 이웃 boid 개수) *alignmentWeight;
//-Separation: 뭉쳐있는 무리를 피해 이동. (모든 이웃의 위치 x에 대해 s += x - 현재 위치); s /= 이웃 boid 개수; return s * SeparationWeight;

