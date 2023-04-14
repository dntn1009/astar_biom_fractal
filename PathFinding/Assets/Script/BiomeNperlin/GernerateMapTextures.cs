using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using csDelaunay;
using System.IO;
public class GernerateMapTextures : MonoBehaviour
{
    //����ȭ
    [Header("���̿� �� ����")]
    [SerializeField] Vector2Int _size;
    [SerializeField] int _nodeAmount = 0;
    [SerializeField] int _loydIterateCount = 0; // ���̵� �˰��� Ƚ��(�������)
    [SerializeField] Color[] _areaColors;
    [Header("������ �� ����")]
    [SerializeField, Range(0f, 0.4f)] float _noiseFrequency = 0;
    [SerializeField] int _noise0ctave = 0;
    [SerializeField] int _seed = 10;
    [SerializeField, Range(0, 0.5f)] float _landNoiseThreshold = 0;
    [SerializeField] int _noiseMaskRadius = 0;
    [SerializeField, Range(2, 40)] int _floorLevelCount = 3; 
    [Header("�� Viewer")]
    [SerializeField] SpriteRenderer _voronoiMapRender = null;
    [SerializeField] SpriteRenderer _NoiseMapRenderer = null;


    void Awake()
    {
        Voronoi vo = GenerateVoronoi(_size, _nodeAmount, _loydIterateCount);
        _voronoiMapRender.sprite = TexMapDrawer.DrawVoronoiToSprite(vo, _areaColors);

    }

    void Update()
    {
        GenerateNoiseMap();

        /*if(Input.GetKeyDown(KeyCode.F3))
        {
            // ���̿ȸ� png ����
            TexMapDrawer.CreateFileFromTexture2D(_voronoiMapRender.sprite.texture, "biom.png");
        }
        if (Input.GetKeyDown(KeyCode.F4))
        {
            // ������� png ����
            TexMapDrawer.CreateFileFromTexture2D(_voronoiMapRender.sprite.texture, "rand.png");
        }*/
    }

    private void OnGUI()
    {
        if(GUI.Button(new Rect(0, 0, 220, 60), "���̿� Map ����"))
        {
            TexMapDrawer.CreateFileFromTexture2D(_voronoiMapRender.sprite.texture, "biom.png");
        }
        if (GUI.Button(new Rect(0, 65, 220, 60), "����Ż Map ����"))
        {
            TexMapDrawer.CreateFileFromTexture2D(_NoiseMapRenderer.sprite.texture, "rand.png");
        }
    }
    Voronoi GenerateVoronoi(Vector2Int size, int nodeAmount, int loydCount)
    {
        List<Vector2> centroids = new List<Vector2>();
        for (int n = 0; n < nodeAmount; n++)
        {
            int rx = Random.Range(0, size.x);
            int ry = Random.Range(0, size.y);

            centroids.Add(new Vector2(rx, ry));
        }
        Rect rt = new Rect(0, 0, size.x, size.y);
        Voronoi vo = new Voronoi(centroids, rt, loydCount);
        return vo;
    }

    float[] CreateMapShape(Vector2Int size, float frequency, int octave)
    {
        int seed = (_seed == 0) ? Random.Range(1, int.MaxValue) : _seed;
        float[] colorDatas = new float[size.x * size.y];
        float[] mask = TexMapDrawer.GetRadialGrandientMask(size, _noiseMaskRadius);

        FastNoiseLite noise = new FastNoiseLite();
        // ����� �޸� ������� �����ϵ��� ����.
        noise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        // �⺻���� ����Ż ������ Ÿ������ ����(Fractional Brownian motion).
        noise.SetFractalType(FastNoiseLite.FractalType.FBm);
        noise.SetFrequency(frequency);
        noise.SetFractalOctaves(octave);
        noise.SetSeed(seed);

        //���� 0~1 �����̸� 0 = ������ 1 = ����� ��Ÿ����.
        int index = 0;
        for(int y = 0; y < size.y; y++)
        {
            for(int x = 0; x < size.x; x++)
            {
                float noiseColorFactor = noise.GetNoise(x, y);
                //������ ������ -1 ~ 1 �����̱� ������ 0 ~ 1 ������ ������ ��ȭ
                noiseColorFactor = (noiseColorFactor + 1) * 0.5f * mask[index];
                float color = (noiseColorFactor >= (1 - 1f / _floorLevelCount)) ? 1f
                    : ((int)(noiseColorFactor * _floorLevelCount) / (float)_floorLevelCount);
                //noiseColorFactor = FloorLevel(noiseColorFactor, _floorLevelCount, mask[index]);

                //colorDatas[index] = noiseColorFactor;
                //float color = noiseColorFactor > _landNoiseThreshold ? noiseColorFactor : 0f;
                colorDatas[index] = color;

                index++;
            }
        }

        return colorDatas;
    }



    public float FloorLevel(float noiseColorFactor, int _floor, float mask)
    {
        int Acount = 1;
        for(int i = 0; i < _floor; i ++)
        {
            //if()
        }

        float test = 0;
        return test;
    }

    public void GenerateNoiseMap()
    {
       
        float[] noiseColor = CreateMapShape(_size, _noiseFrequency, _noise0ctave);
        Color[] colors = new Color[noiseColor.Length];

        for(int i = 0; i < colors.Length; i++)
        {

            float r = noiseColor[i];
            float g = noiseColor[i];
            float b = noiseColor[i];
            colors[i] = new Color(r, g, b, 1);
        }

        _NoiseMapRenderer.sprite = TexMapDrawer.DrawSprite(_size, colors);
    }


    public static void CreateFileFromTexture2D(Texture2D texture, string saveFileName)
    {
        byte[] by = texture.EncodeToPNG();
        string path = Application.dataPath + "/Images/" + saveFileName;
        FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write);
        BinaryWriter bw = new BinaryWriter(fs);
        bw.Write(by);

        bw.Close();
        fs.Close();
    }

}
