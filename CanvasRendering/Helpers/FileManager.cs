namespace CanvasRendering.Helpers;

public delegate Stream LoadFileDelegate(string file);

public class FileManager
{
    private static LoadFileDelegate loadFile;

    public static void SetLoadFileDelegate(LoadFileDelegate loadFileDelegate)
    {
        loadFile = loadFileDelegate;
    }

    public static Stream LoadFile(string filePath)
    {
        return loadFile(filePath);
    }
}