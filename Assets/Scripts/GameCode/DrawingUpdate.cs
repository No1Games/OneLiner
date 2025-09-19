using System;
using System.Collections.Generic;
using UnityEngine;

public class DrawingUpdate : MonoBehaviour
{
    [SerializeField] private Camera drawingCamera; // Камера, з якої робимо скріншот
    [SerializeField] List<GameObject> objectsToHide; // Об'єкти UI, які потрібно приховати під час скріншоту
    public event Action<Texture2D> OnScreenshotTaken; // Подія, що викликається після створення скріншоту

    public int screenshotWidth = 1920;   // Ширина скріншоту
    public int screenshotHeight = 1080;  // Висота скріншоту

    public void TakeScreenshot()
    {
        // Приховуємо UI перед скріншотом
        foreach (GameObject obj in objectsToHide)
        {
            obj.SetActive(false);
        }

        // Створюємо RenderTexture з потрібною роздільною здатністю
        RenderTexture renderTexture = new RenderTexture(screenshotWidth, screenshotHeight, 24);
        drawingCamera.targetTexture = renderTexture;  // Призначаємо RenderTexture камері

        // Рендеримо сцену
        drawingCamera.Render();

        // Встановлюємо RenderTexture як активний для копіювання пікселів
        RenderTexture.active = renderTexture;

        // Створюємо Texture2D для збереження скріншоту
        Texture2D screenshot = new Texture2D(screenshotWidth, screenshotHeight, TextureFormat.RGB24, false);

        // Копіюємо пікселі з RenderTexture в Texture2D
        screenshot.ReadPixels(new Rect(0, 0, screenshotWidth, screenshotHeight), 0, 0);
        screenshot.Apply();

        // Відв'язуємо RenderTexture від камери і робимо його неактивним
        drawingCamera.targetTexture = null;
        RenderTexture.active = null;

        // Знищуємо тимчасовий RenderTexture
        Destroy(renderTexture);

        // Показуємо UI назад
        foreach (GameObject obj in objectsToHide)
        {
            obj.SetActive(true);
        }

        // Викликаємо подію, щоб повідомити, що скріншот готовий
        if (screenshot != null)
        {
            ProcessScreenshot(screenshot);
        }
    }

    private void ProcessScreenshot(Texture2D screenshot)
    {
        // Викликаємо подію та передаємо скріншот
        OnScreenshotTaken?.Invoke(screenshot);
    }
}
