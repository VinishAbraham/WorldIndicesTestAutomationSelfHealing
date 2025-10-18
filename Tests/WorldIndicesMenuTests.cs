using OpenQA.Selenium;
using System;
using System.Threading.Tasks;

public class WorldIndicesMenuPage
{
    private readonly IWebDriver _driver;
    private readonly OllamaHelper _ollama;

    public WorldIndicesMenuPage(IWebDriver driver, OllamaHelper ollama)
    {
        _driver = driver;
        _ollama = ollama;
    }

    // Try to find by Id first, else self-heal with LLM
    public async Task<IWebElement?> FindMenuItemAsync(string id, string description)
    {
        try
        {
            return _driver.FindElement(By.Id(id));
        }
        catch (NoSuchElementException)
        {
            Console.WriteLine($"[Self-Heal] Could not find element with Id '{id}'. Invoking Ollama...");
            string html = _driver.PageSource;
            string? selector = await _ollama.SuggestSelectorAsync(html, $"#{id}", description);
            if (!string.IsNullOrWhiteSpace(selector))
            {
                Console.WriteLine($"[Self-Heal] Ollama suggested selector: {selector}");
                try
                {
                    return _driver.FindElement(By.CssSelector(selector));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Self-Heal] Ollama's selector failed: {ex.Message}");
                    return null;
                }
            }
            else
            {
                Console.WriteLine("[Self-Heal] Ollama did not return a selector.");
                return null;
            }
        }
    }
}