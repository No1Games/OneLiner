using System;
using System.Collections.Generic;
using UnityEngine;

public class DrawingUpdate : MonoBehaviour
{
    [SerializeField] private Camera drawingCamera;
    [SerializeField] List<GameObject> objectsToHide;
    public event Action<Texture2D> OnScreenshotTaken;

    public int screenshotWidth = 1920;   // ������ ��������
    public int screenshotHeight = 1080;  // ������ ��������

    public void TakeScreenshot()
    {
        // ������� UI ����� ���������
        foreach(GameObject obj in objectsToHide)
        {
            obj.SetActive(false);
        }
        

        // ��������� RenderTexture � �������� ��������� ��������
        RenderTexture renderTexture = new RenderTexture(screenshotWidth, screenshotHeight, 24);
        drawingCamera.targetTexture = renderTexture;  // ����'����� RenderTexture �� ������

        // ��������� �����
        drawingCamera.Render();

        // ������������ RenderTexture �� ��������
        RenderTexture.active = renderTexture;

        // ��������� Texture2D ��� ��������� ����������
        Texture2D screenshot = new Texture2D(screenshotWidth, screenshotHeight, TextureFormat.RGB24, false);

        // ������� ����� � RenderTexture � Texture2D
        screenshot.ReadPixels(new Rect(0, 0, screenshotWidth, screenshotHeight), 0, 0);
        screenshot.Apply();

        // ³��'����� RenderTexture �� ������ � ������ ���� ����������
        drawingCamera.targetTexture = null;
        RenderTexture.active = null;

        // ������� ���������� RenderTexture
        Destroy(renderTexture);

        // ������� UI �����
        foreach (GameObject obj in objectsToHide)
        {
            obj.SetActive(true);
        }

        // ��������� ����, ���� ������� ��������
        if (screenshot != null)
        {
            ProcessScreenshot(screenshot);
        }
    }

    private void ProcessScreenshot(Texture2D screenshot)
    {
        // ��������� ���� �� �������� �����
        OnScreenshotTaken?.Invoke(screenshot);
    }
    


}
