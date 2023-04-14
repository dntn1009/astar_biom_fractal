using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using csDelaunay;
using System.Linq;
using System.IO;

public class TexMapDrawer
{
    public static Sprite DrawSprite(Vector2Int size, Color[] colorDatas)
    {
        Texture2D texture = new Texture2D(size.x, size.y);
        texture.filterMode = FilterMode.Point;
        texture.SetPixels(colorDatas);
        texture.Apply();

        Rect rect = new Rect(0, 0, size.x, size.y);
        Sprite map = Sprite.Create(texture, rect, Vector2.one * 0.5f);

        return map;
    }

    public static Sprite DrawVoronoiToSprite(Voronoi vo, Color[] areaColor)
    {
        Rect rect = vo.PlotBounds;
        int width = Mathf.RoundToInt(rect.width);
        int height = Mathf.RoundToInt(rect.height);
        Color[] pixelColors = Enumerable.Repeat(Color.white, width * height).ToArray();
        List<Vector2> siteCoords = vo.SiteCoords();
        List<Color> posColor = new List<Color>();
        Dictionary<int, Color> posOre = new Dictionary<int, Color>();
    
        // 점찍기(무게중심 그리기)
        foreach (Vector2 coord in siteCoords)
        {
            int x = Mathf.RoundToInt(coord.x);
            int y = Mathf.RoundToInt(coord.y);

            //2차원을 1차원으로
            int index = x + width * y;
            Color area = areaColor[Random.Range(0, areaColor.Length)];
            pixelColors[index] = area;
            posColor.Add(pixelColors[index]);
            posOre.Add(index, area);
        }
        Vector2Int size = new Vector2Int(width, height);
        
        //선그리기 (모서리 그리기)
        foreach (Site site in vo.Sites)
        {
            List<Site> neighbors = site.NeighborSites();

            foreach (Site neighbor in neighbors)
            {
                // 이웃한 폴리곤들에게서 겹치는 가장자리를 유도해낸다.=> 삼각분할
                Edge edge = vo.FindEdgeFromAdjacentPolygons(site, neighbor);

                if (edge.ClippedVertices is null)
                    continue;

                // 가장자리를 이루는 모서리 정점(꼭지점) 2개를 얻어온다.
                Vector2 corner1 = edge.ClippedVertices[LR.LEFT];
                Vector2 corner2 = edge.ClippedVertices[LR.RIGHT];
                // 1차 함수 그래프를 그리듯 가장자리 선분을 그린다.
                Vector2 targetPoint = corner1;
                float delta = 1 / (corner2 - corner1).magnitude;
                float lerpRatio = 0f;
                while((int)targetPoint.x != (int)corner2.x || (int)targetPoint.y != (int)corner2.y)
                {
                    //corner1과 corner2 사이의 점을 선형보간을 통해 lerpRatio만큼 나누는 점을 받아온다.
                    targetPoint = Vector2.Lerp(corner1, corner2, lerpRatio);
                    lerpRatio += delta;
                    //텍스쳐의 좌표 영역은 ( 0 ~ size.x - 1) 이지만 생성한 보로노이 다이어그램의 좌표는 (~ (float)size.x)이다.
                    int x = Mathf.Clamp((int)targetPoint.x, 0, size.x - 1);
                    int y = Mathf.Clamp((int)targetPoint.y, 0, size.y - 1);

                    int index = x + size.x * y;
                    pixelColors[index] = Color.black;
                }

            }
        }

        foreach(int index in posOre.Keys)
        {
            Color area = posOre[index];

            int cx = index % width;
            int cy = index / width;
            pixelColors[cx + width * cy] = Color.cyan;
            ColorringFace(pixelColors, cx, cy, Color.black, area, size);
        }

        //Draw(width, height, pixelColors, posColor, 0, 0);

        // 텍스쳐화 시키고 스프라이트로 만들기
        return DrawSprite(size, pixelColors);

    }

    public static float[] GetRadialGrandientMask(Vector2Int size, int maskRadius)
    {
        float[] colorData = new float[size.x * size.y];

        Vector2Int center = size / 2;   // 맵 중심
        float radius = center.x;        // 맵 반지름
        int index = 0;
        for (int y = 0; y < size.y; y++)
        {
            for (int x = 0; x < size.x; x++)
            {
                Vector2Int position = new Vector2Int(x, y);
                //맵의 중점으로부터 거리에 따라 색을 결정(마스킹 범위도 고려).
                float distFromCenter = Vector2Int.Distance(center, position) + (radius-maskRadius);
                float colorFactor = distFromCenter / radius;
                //거리가 멀수록 색은 1에 가까워지지만 내륙쪽일수록 고지대를 지향해야 해서 색을 반전함.
                colorData[index++] = 1 - colorFactor;

            }
        }
        return colorData;
    }

    static void ColorringFace(Color[] pixelColors, int x, int y, Color checkColor, Color targetColor, Vector2Int size)
    {
        if (x >= size.x || x < 0 || y >= size.y || y < 0)
            return;
        if (pixelColors[x + size.x * y] == checkColor)
            return;
        if (pixelColors[x + size.x * y] == targetColor)
            return;
        pixelColors[x + size.x * y] = targetColor;

        ColorringFace(pixelColors, x - 1, y, checkColor, targetColor, size);
        ColorringFace(pixelColors, x + 1, y, checkColor, targetColor, size);
        ColorringFace(pixelColors, x, y - 1, checkColor, targetColor, size);
        ColorringFace(pixelColors, x, y + 1, checkColor, targetColor, size);
    }

    //하이트맵 => 카툰히어로에 적용
    //펄린 알고리즘

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
