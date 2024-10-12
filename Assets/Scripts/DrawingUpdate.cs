using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DrawingUpdate : MonoBehaviour
{
    [SerializeField] private Camera drawingCamera;
    [SerializeField] GameObject objectToHide;
    public event Action<Texture2D> OnScreenshotTaken;

    public int screenshotWidth = 1920;   // ������ ��������
    public int screenshotHeight = 1080;  // ������ ��������

    public void TakeScreenshot()
    {
        // ������� UI ����� ���������
        objectToHide.SetActive(false);

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
        objectToHide.SetActive(true);

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
