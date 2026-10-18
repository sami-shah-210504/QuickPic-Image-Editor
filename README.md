# Quick Pic Image Editor 
 Final project for **Fundamentals of Software Engineering**.
 A simple Windows Forms desktop image editor written in C# (.NET Framework 4.7.2).
 
## Features
- Open images (JPG, JPEG, PNG, BMP)
- Grayscale
- Invert colors
- Brightness and saturation adjustment
- Crop (drag a selection on the image)
- Resize (width / height)
- Undo / Redo
- Save the edited image

## Requirements
- Windows
- Visual Studio 2022 (with the ".NET desktop development" workload)

## Run
1. Open `FSEE.sln` in Visual Studio.
2. Press **F5** (or Build > Start Debugging).
3. Click **Select file**, choose an image, and start editing.
 
## Project structure
- `FSEE/Form1.cs` – start screen and file picker
- `FSEE/Form2.cs` – editor window (filters, crop, resize, undo/redo, save)
- `FSEE/Program.cs` – entry point

## Author(s)
- Syed Sami Shah
- Mian Abdul Wahab
