using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidInfo : MonoBehaviour
{
    public int imageSize = 512;

    public int particleCount = 1024;
    [Range(0, 10)]
    public float cohesionWeight = 1;
    [Range(0, 10)]
    public float alignmentWeight = 1;
    [Range(0, 10)]
    public float separationWeight = 1;
    [Range(0, 1)]
    public float directionSenstivity = 0.1f;
    public float speed = 1;
    public int neighborRadius = 10;

    public static BoidInfo instance;

    private void Awake()
    {
        instance = this;
        InitUI();
        InitParticles();
    }
    void InitUI()
    {
        UIManagement uiManager = GetComponent<UIManagement>();

        uiManager.SetCohesionSlider(SetCoheisionWeight, cohesionWeight);
        uiManager.SetAlignmentSlider(SetAlignmentWeight, alignmentWeight);
        uiManager.SetSeparationSlider(SetSeparationWeight, separationWeight);
        uiManager.SetSpeedSlider(SetSpeed, speed);
        uiManager.SetDirectionSensSlider(SetDirectionSens, directionSenstivity);
        uiManager.SetMainText(imageSize, particleCount, neighborRadius);
    }
    void SetCoheisionWeight(float val)
    {
        cohesionWeight = val;
        //boidShader.SetFloat("cohesionWeight", cohesionWeight);
    }
    void SetAlignmentWeight(float val)
    {
        alignmentWeight = val;
        //boidShader.SetFloat("alignmentWeight", alignmentWeight);
    }
    void SetSeparationWeight(float val)
    {
        separationWeight = val;
        //boidShader.SetFloat("separationWeight", separationWeight);
    }
    void SetDirectionSens(float val)
    {
        directionSenstivity = val;
        //boidShader.SetFloat("speed", speed);
    }
    void SetSpeed(float val)
    {
        speed = val;
       //boidShader.SetFloat("speed", speed);
    }



    public List<particle> particles = new List<particle>();
    public List<uint> cellID = new List<uint>();
    void InitParticles()
    {
        int imageSize_half = imageSize / 2;
        for (int i = 0; i < particleCount; i++)
        {
            Vector2 direction = new Vector2();
            direction.x = Random.Range(-1f, 1f);
            direction.y = Random.Range(-1f, 1f);

            direction = direction.normalized;
            particle p = new particle
            {
                px = Random.Range(-imageSize_half, imageSize_half),
                py = Random.Range(-imageSize_half, imageSize_half),
                dx = direction.x,
                dy = direction.y,
               // groupNum = Random.Range(0, 10)
            };
            particles.Add(p);
            cellID.Add((uint)(Mathf.FloorToInt(p.px + imageSize_half) +
                              Mathf.FloorToInt(p.py + imageSize_half) * imageSize));
        }
    }

}
public struct particle
{
    public float px, py;
    public float dx, dy;
    //public int groupNum;
};