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

    public int screenshotWidth = 1920;   // Ширина скріншота
    public int screenshotHeight = 1080;  // Висота скріншота

    public void TakeScreenshot()
    {
        // Сховати UI перед скріншотом
        objectToHide.SetActive(false);

        // Створюємо RenderTexture з потрібною роздільною здатністю
        RenderTexture renderTexture = new RenderTexture(screenshotWidth, screenshotHeight, 24);
        drawingCamera.targetTexture = renderTexture;  // Прив'язуємо RenderTexture до камери

        // Рендеримо сцену
        drawingCamera.Render();

        // Встановлюємо RenderTexture як активний
        RenderTexture.active = renderTexture;

        // Створюємо Texture2D для копіювання зображення
        Texture2D screenshot = new Texture2D(screenshotWidth, screenshotHeight, TextureFormat.RGB24, false);

        // Копіюємо пікселі з RenderTexture в Texture2D
        screenshot.ReadPixels(new Rect(0, 0, screenshotWidth, screenshotHeight), 0, 0);
        screenshot.Apply();

        // Відв'язуємо RenderTexture від камери і робимо його неактивним
        drawingCamera.targetTexture = null;
        RenderTexture.active = null;

        // Знищуємо тимчасовий RenderTexture
        Destroy(renderTexture);

        // Вмикаємо UI назад
        objectToHide.SetActive(true);

        // Викликаємо подію, якщо скріншот зроблено
        if (screenshot != null)
        {
            ProcessScreenshot(screenshot);
        }
    }

    private void ProcessScreenshot(Texture2D screenshot)
    {
        // Викликаємо подію та передаємо знімок
        OnScreenshotTaken?.Invoke(screenshot);
    }


}
