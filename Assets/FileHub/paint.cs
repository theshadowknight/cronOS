#if false
enum DrawingState
{
    Drawing,
    DrawingLine,
    DrawingRectangle,
    DrawingElipse,
}

public class paint : ExtendedShellProgram
{
    private string filePath;
    bool waitingForStartPosition = false;
    SystemTexture image = null;
    SystemTexture overlay = null;
    File imageFile = null;
    int scale = 4;
    
    Vector2Int? mousePos = null;
    SystemColor mainColor = SystemColor.white;
    SystemColor UIColor = SystemColor.dark_gray;
    SystemColor secondaryColor = SystemColor.black;
    bool editingMainColor = true;
    DrawingState drawingState = DrawingState.Drawing;
    Vector2Int? startPos = null; //Vector2Int.incorrectVector;
    Vector2Int? lastPos = null; //Vector2Int.incorrectVector;

    KeySequence ks = null;
    bool running = true;

    public override string GetName()
    {
        return "paint";
    }

    protected override string InternalRun(Dictionary<AcceptedArgument, string> argPairs)
    {
        Debugger.Debug(argPairs.ToFormattedString2());
        string workingPath = argPairs.GetValueOrNull("-wd")?.Value ?? "/";
        string name = argPairs.GetValueOrNull("-n")?.Value ?? "image.img";

        int height = int.Parse(argPairs.GetValueOrNull("-h")?.Value ?? "10");
        int width = int.Parse(argPairs.GetValueOrNull("-w")?.Value ?? "10");

        filePath = argPairs.GetValueOrNull("-f")?.Value ?? workingPath;


        if (fileSystem.TryGetFile(filePath + "/" + name, out File file))
        {
            image = SystemTexture.FromData(file.data);
            imageFile = file;
        }
        else
        {
//todo 1 fix: -nf doesn't make new file when file exists
            if (argPairs.ContainsKey("-nf"))
            {
                image = new SystemTexture(width, height);
            }
            else
            {
                //todo 4 error
                return "error";
            }
        }

        overlay = new SystemTexture(image.width, image.height);
        lock (mainLock)
        {
            keyHandler.DumpInputBuffer();
            keyHandler.DumpStringInputBuffer();
//todo 2 rework work loop: paint mode, menu. In paint you draw in menu you can open and save file.
            while (running)
            {
                Debugger.Debug("paint loop");
                Draw();
                ProcessInput();
                runtime.Wait();
            }

            imageFile ??= Drive.MakeFile(name, new byte[0]);

            byte[] data = image.ToData();
            imageFile.data = data;
            File parent = fileSystem.GetFileByPath(filePath);
            parent.SetChild(imageFile);
        }

        Debugger.Debug("paint lock off");
        keyHandler.DumpInputBuffer();
        keyHandler.DumpStringInputBuffer();
        return "paint finished work.";
    }

    private static readonly List<AcceptedArgument> _argumentTypes = new List<AcceptedArgument>
    {
        new AcceptedArgument("working directory", true, "-wd"),
        new AcceptedArgument("current file", true, "-f"),
        new AcceptedArgument("new file", false, "-nf"),
        new AcceptedArgument("name", true, "-n"),
        new AcceptedArgument("width", true, "-w"),
        new AcceptedArgument("height", true, "-h"),
        new AcceptedArgument("color palette", true, "-cp")
    };


    protected override List<AcceptedArgument> argumentTypes => _argumentTypes;


    void Draw()
    {
        screenBuffer.FillAll(SystemColor.black);
        overlay.FillAll(SystemColor.black);
        if (waitingForStartPosition)
        {
        }
        else
        {
            if (mousePos.HasValue)
            {
                switch (drawingState)
                {
                    case DrawingState.DrawingLine:
                        if (startPos.HasValue)
                        {
                            overlay.DrawLine(startPos.Value, mousePos.Value + new Vector2Int(1, 1), UIColor);
                        }

                        break;
                    case DrawingState.DrawingRectangle:
                        if (startPos.HasValue)
                        {
                            overlay.DrawRectangle(startPos.Value, mousePos.Value + new Vector2Int(1, 1), UIColor);
                        }

                        break;
                    case DrawingState.DrawingElipse:
                        if (startPos.HasValue)
                        {
                            overlay.DrawEllipseInRect(startPos.Value, mousePos.Value + new Vector2Int(1, 1), UIColor);
                        }

                        break;
                    default:
                        overlay.SetAt(mousePos.Value.x + 1, mousePos.Value.y, UIColor);
                        overlay.SetAt(mousePos.Value.x - 1, mousePos.Value.y, UIColor);
                        overlay.SetAt(mousePos.Value.x, mousePos.Value.y + 1, UIColor);
                        overlay.SetAt(mousePos.Value.x, mousePos.Value.y - 1, UIColor);
                        break;
                }
            }
        }

        for (int iterY = 0; iterY < image.height; iterY++)
        {
            for (int iterX = 0; iterX < image.width; iterX++)
            {
                for (int iterYScale = 0; iterYScale < scale; iterYScale++)
                {
                    for (int iterXScale = 0; iterXScale < scale; iterXScale++)
                    {
                        screenBuffer.SetAt(iterX * scale + iterXScale, iterY * scale + iterYScale,
                            image.GetAt(iterX, iterY));
                        if (overlay.GetAt(iterX, iterY) != SystemColor.black)
                        {
                            screenBuffer.SetAt(iterX * scale + iterXScale, iterY * scale + iterYScale,
                                overlay.GetAt(iterX, iterY));
                        }
                    }
                }
            }
        }

        screenBuffer.DrawLine(image.width * scale, 0, image.width * scale, image.height * scale, SystemColor.white);
        screenBuffer.DrawLine(0, image.height * scale, image.width * scale, image.height * scale, SystemColor.white);
        screenBuffer.Fill(image.width * scale + 2, 2, 20, 20, editingMainColor ? SystemColor.blue : SystemColor.white);
        screenBuffer.Fill(image.width * scale + 2, 22, 20, 20, editingMainColor ? SystemColor.white : SystemColor.blue);

        screenBuffer.Fill(image.width * scale + 4, 4, 16, 16, mainColor);
        screenBuffer.Fill(image.width * scale + 4, 24, 16, 16, secondaryColor);

        screen.SetScreenBuffer(screenBuffer);
    }

    void ChangeColor(ref SystemColor selectedColor)
    {
        int num = ks.ReadDigit(out Key key);
        if (key != Key.None && num != -1)
        {
            selectedColor = num;
        }

        if (ks.ReadKey(Key.Q))
        {
            selectedColor = 10;
        }

        if (ks.ReadKey(Key.W))
        {
            selectedColor = 11;
        }

        if (ks.ReadKey(Key.E))
        {
            selectedColor = 12;
        }

        if (ks.ReadKey(Key.R))
        {
            selectedColor = 13;
        }

        if (ks.ReadKey(Key.T))
        {
            selectedColor = 14;
        }

        if (ks.ReadKey(Key.Y))
        {
            selectedColor = 15;
        }
    }

    void ProcessInput()
    {
        ks = keyHandler.WaitForInputBuffer(true ? 20 : 0);

        if (ks.ReadKey(Key.Escape))
        {
            running = false;
            return;
        }

        if (ks.ReadKey(Key.Plus) || ks.ReadKey(Key.KeypadPlus))
        {
            scale++;
            if (scale > 40)
            {
                scale = 40;
            }
        }

        if (ks.ReadKey(Key.Minus) || ks.ReadKey(Key.KeypadMinus))
        {
            scale--;
            if (scale < 1)
            {
                scale = 1;
            }
        }

        if (ks.ReadKey(Key.A))
        {
            drawingState = DrawingState.Drawing;
            waitingForStartPosition = false;
        }

        if (ks.ReadKey(Key.S))
        {
            drawingState = DrawingState.DrawingLine;
            waitingForStartPosition = true;
            // return;
        }

        if (ks.ReadKey(Key.D))
        {
            drawingState = DrawingState.DrawingRectangle;
            waitingForStartPosition = true;
            // return;
        }

        if (ks.ReadKey(Key.F))
        {
            drawingState = DrawingState.DrawingElipse;
            waitingForStartPosition = true;
            // return;
        }

        Debugger.Debug(drawingState);
        if (ks.ReadKey(Key.U))

        {
            editingMainColor = !editingMainColor;
        }

        if (editingMainColor)
        {
            ChangeColor(ref mainColor);
        }
        else
        {
            ChangeColor(ref secondaryColor);
        }

        lastPos = mousePos;
        mousePos = mouseHandler.GetScreenPosition();
        if (mousePos.HasValue)
        {
            Debugger.Debug(mousePos);
            mousePos = new Vector2Int(mousePos.Value.x / scale, mousePos.Value.y / scale);
            Debugger.Debug(mousePos);
        }

        if (ks.ReadKey(Key.Mouse0, false) || ks.ReadKey(Key.Mouse1, false))
        {
            if (waitingForStartPosition)
            {
                startPos = mousePos;
                waitingForStartPosition = false;
                ks.ReadAndCooldownKey(Key.Mouse1);
                ks.ReadAndCooldownKey(Key.Mouse0);
            }
            else
            {
                switch (drawingState)
                {
                    case DrawingState.DrawingLine:
                    {
                        if (!startPos.HasValue || !mousePos.HasValue)
                        {
                            break;
                        }

                        image.DrawLine(startPos.Value, mousePos.Value + new Vector2Int(1, 1),
                            ks.ReadAndCooldownKey(Key.Mouse0) ? mainColor : secondaryColor);
                        drawingState = DrawingState.Drawing;
                        ks.ReadAndCooldownKey(Key.Mouse1);
                        startPos = null;
                        break;
                    }
                    case DrawingState.DrawingRectangle:
                    {
                        if (!startPos.HasValue || !mousePos.HasValue)
                        {
                            break;
                        }

                        image.DrawRectangle(startPos.Value, mousePos.Value,
                            ks.ReadAndCooldownKey(Key.Mouse0) ? mainColor : secondaryColor);
                        drawingState = DrawingState.Drawing;
                        ks.ReadAndCooldownKey(Key.Mouse1);
                        startPos = null;
                        break;
                    }
                    case DrawingState.DrawingElipse:
                    {
                        if (!startPos.HasValue || !mousePos.HasValue)
                        {
                            break;
                        }

                        image.DrawEllipseInRect(startPos.Value, mousePos.Value + new Vector2Int(1, 1),
                            ks.ReadAndCooldownKey(Key.Mouse0) ? mainColor : secondaryColor);
                        drawingState = DrawingState.Drawing;

                        ks.ReadAndCooldownKey(Key.Mouse1);
                        startPos = null;
                        break;
                    }
                    case DrawingState.Drawing:
                    {
                        if (!lastPos.HasValue || !mousePos.HasValue)
                        {
                            break;
                        }

                        //image.SetAt(mousePos, ks.ReadKey(Key.Mouse0) ? mainColor : secondaryColor);
                        image.DrawLine(lastPos.Value, mousePos.Value,
                            ks.ReadKey(Key.Mouse0) ? mainColor : secondaryColor);

                        break;
                    }
                }
            }
        }
    }
}
#endif