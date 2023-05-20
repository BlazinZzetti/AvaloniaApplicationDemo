using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Dialogs;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform;

namespace AvaloniaApplicationDemo;

public partial class MainWindow : Window
{
    private string appStart
    {
        get
        {
            return AppContext.BaseDirectory.Replace("net6.0\\", "").Replace("net6.0/", "");
        }
    }

    private string dolphinPath
    {
        get { return Configuration.Instance.DolphinLocation; }
    }
        
    private string savePath
    {
        get { return dolphinPath + @"User\GC\USA\Card A\"; }
    }
        
    private string gameSettingsFilePath
    {
        get { return dolphinPath + @"User\GameSettings\GUPX8P.ini"; }
    }

    private string customTexturesPath
    {
        get { return dolphinPath + @"User\Load\Textures\GUPX8P\"; }
    }

    private string sxResourcesPath
    {
        get { return appStart + @"ShadowSXResources\"; }
    }
        
    private string sxResourcesCustomTexturesPath
    {
        get { return sxResourcesPath + @"CustomTextures\GUPX8P\"; }
    }
        
    private string sxResourcesISOPatchingPath
    {
        get { return sxResourcesPath + @"PatchingFiles\"; }
    }

    private OperatingSystemType currentOS
    {
        get { return AvaloniaLocator.Current.GetService<IRuntimePlatform>().GetRuntimeInfo().OperatingSystem; }
    }
    
    public MainWindow()
    {
        InitializeComponent();
        Configuration.Instance.LoadSettings();
        RegisterEvents();
    }

    private void RegisterEvents()
    {
        PlayButton.Click += OnPlayButtonPressed;
        OpenGameLocationButton.Click += OpenGameLocationButtonPressed;
        OpenSaveFileLocationButton.Click += OnSaveFileButtonPressed;
    }

    private void EnableButtons(bool enable)
    {
        PlayButton.IsEnabled = enable;
        CreateROMButton.IsEnabled = enable;
        OpenGameLocationButton.IsEnabled = enable;
        OpenSaveFileLocationButton.IsEnabled = enable;
        SettingsButton.IsEnabled = enable;
    }
    
    private async void OnPlayButtonPressed(object? sender, RoutedEventArgs e)
    {
        EnableButtons(false);
        
        if (string.IsNullOrEmpty(Configuration.Instance.DolphinLocation))
        {
            await OpenSetDolphinDialog();
        }

        if (!string.IsNullOrEmpty(Configuration.Instance.DolphinLocation))
        {
            //Check if Rom Location has been set at all.
            if (string.IsNullOrEmpty(Configuration.Instance.RomLocation))
            {
                await OpenSetRomDialog();
            }

            //Only continue if Rom Location has been set, in case it was not in the above code. 
            if (!string.IsNullOrEmpty(Configuration.Instance.RomLocation))
            {
                //Double check if the provided path has a file, if not re-prompt for a ROM.
                if (!File.Exists(Configuration.Instance.RomLocation))
                {
                    ShowMessageBox("ROM file not found. Please provide ROM location again.");
                    //OpenRomDialog();
                }

                //At this point assume there is a correct ROM. Technically nothing stopping a user from
                //choosing whatever ROM they want to launch, but trying to account for that without additional
                //annoying checks and processes is not worth it.

                UpdateCustomAssets();

                //Double check the .exe is found before attempting to run it.
                if (File.Exists(dolphinPath + @"\Dolphin.exe"))
                {
                    Process.Start("\"" + dolphinPath + @"\Dolphin.exe" + "\"",
                        @" -b " + "\"" + Configuration.Instance.RomLocation + "\"");
                    Close();
                }
                else
                {
                    ShowMessageBox("Could not find dolphin.exe. Please double check directory files.");
                }
            }
        }
        
        EnableButtons(true);
    }

    private void ShowMessageBox(string message)
    {
        var mb = new MessageBox();
        mb.SetupMessageBox(message);
        mb.ShowDialog(this);
    }

    private async Task OpenSetRomDialog()
    {
        var result = await SetFilePath("Set Path to SX ROM", 
        new FileDialogFilter()
        {
            Name = "ROM File",
            Extensions = new List<string>() {"iso"}
        });
        
        Configuration.Instance.RomLocation = result == null ? "" : result.First();
    }
    
    private async Task<string[]?> SetFilePath(string title, FileDialogFilter filter)
    {
        var ofd = new OpenFileDialog();
        ofd.Title = title;
        ofd.Filters = new List<FileDialogFilter>() { filter };
        ofd.Directory = appStart;
        ofd.AllowMultiple = false;
        return await ofd.ShowAsync(this);
    }

    private async Task OpenSetDolphinDialog()
    {
        var result = await SetFolderPath("Set Path to Dolphin");
        Configuration.Instance.DolphinLocation = String.IsNullOrEmpty(result) ? "" : result;
    }

    private async Task<string?> SetFolderPath(string title)
    {
        var ofd = new OpenFolderDialog();
        ofd.Title = "Set Path to Dolphin";
        ofd.Directory = appStart; 
        return await ofd.ShowAsync(this);
    }

    private void OpenGameLocationButtonPressed(object? sender, RoutedEventArgs e)
    {
        OpenFolder(appStart);
    }

    private void OnSaveFileButtonPressed(object? sender, RoutedEventArgs e)
    {
        var success = OpenFolder(savePath);
        if (!success)
        {
            ShowMessageBox("Please launch game to generate the save directory.");
        }
    }

    /// <summary>
    /// Open the provided folder path in the file explorer of the current operating system.
    /// </summary>
    /// <param name="folderPath"></param>
    /// <returns>Returns True if File Path Exists.</returns>
    private bool OpenFolder(string folderPath)
    {
        if (Directory.Exists(folderPath))
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = GetExplorerPath(),
                Arguments = folderPath,
                UseShellExecute = true
            };
            Process.Start(psi);
            return true;
        }

        return false;
    }
    
    private string GetExplorerPath()
    {
        switch (currentOS)
        {
            case OperatingSystemType.WinNT:
                return "explorer.exe";
            case OperatingSystemType.Linux:
                return "xdg-open";
            case OperatingSystemType.OSX:
                return "open";
            default:
                throw new Exception("Unsupported Operating System");
        }
    }

    private void SettingsButton_Click(object sender, EventArgs e)
    {
        //var settingsDialog = new SettingsDialog();
        //settingsDialog.ShowDialog();
    }
    
    /*private void CreateRomButton_Click(object sender, EventArgs e)
    {
        var xdeltaExePath = sxResourcesISOPatchingPath + @"\xdelta-3.1.0-x86_64.exe";
        var vcdiffPath = sxResourcesISOPatchingPath + @".\vcdiff\ShadowSX.vcdiff";
        var patchBatPath = sxResourcesISOPatchingPath + @"\Patch ISO.bat";
                
        var allPatchFilesFound = File.Exists(xdeltaExePath);
        allPatchFilesFound &= File.Exists(vcdiffPath);
        allPatchFilesFound &= File.Exists(patchBatPath);
        
        if (allPatchFilesFound)
        {
            var gupe8pLocation = "";
            var patchedRomDestination = "";
            
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                ofd.Filter = "ROM file (*.iso)|*.iso";
                ofd.RestoreDirectory = true;

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    gupe8pLocation = ofd.FileName;
                }
            }

            if (!string.IsNullOrEmpty(gupe8pLocation))
            {
                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.InitialDirectory = gupe8pLocation;
                    sfd.FileName = "ShadowSX";
                    sfd.Filter = "ROM file (*.iso)|*.iso";
                    sfd.RestoreDirectory = true;

                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        patchedRomDestination = sfd.FileName;
                    }
                }
            }
            else
            {
                MessageBox.Show("Operation Cancelled");
                return;
            }
            
            //We can assume that gupe8pLocation is not empty or null. 
            if (!string.IsNullOrEmpty(patchedRomDestination))
            {
                var batArguments = string.Format("\"{0}\" \"{1}\" \"{2}\" \"{3}\"", gupe8pLocation, patchedRomDestination,
                        xdeltaExePath, vcdiffPath);
                    
                var processResult = Process.Start("\"" + patchBatPath + "\"", batArguments);
                if (processResult != null)
                {
                    processResult.WaitForExit();

                    switch (processResult.ExitCode)
                    {
                        case 0:
                            //MessageBox by default doesnt have alignment options. Hack it to look nice to avoid needing to create a new control dialog.
                            var messageResult = MessageBox.Show(
                                "                           ROM Created Successfully." + Environment.NewLine + Environment.NewLine
                                + "Would you like to set the location of this ROM as the " + Environment.NewLine
                                + "location this launcher will use to launch the game?", "ROM Patch Successful",
                                MessageBoxButtons.YesNo);

                            if (messageResult == DialogResult.Yes)
                            {
                                Configuration.Instance.RomLocation = patchedRomDestination;
                                Configuration.Instance.SaveSettings();
                            }

                            break;
                        default:
                            //MessageBox by default doesnt have alignment options. Hack it to look nice to avoid needing to create a new control dialog.
                            MessageBox.Show(
                                "                           ROM Patching Failed. " + Environment.NewLine + Environment.NewLine
                                + "Please ensure that provided paths are valid and that " + Environment.NewLine
                                + "the Shadow ROM provided is a full size clean rip. " + Environment.NewLine + Environment.NewLine
                                + "                   Expected ROM CRC32: F582CF1E", "ROM Patch Failed");
                            break;
                    }
                }
                else
                {
                    MessageBox.Show("ROM Patching failed to launch.");
                }
            }
            else
            {
                MessageBox.Show("Operation Cancelled");
            }
        }
        else
        {
            MessageBox.Show("One or more files needed to complete the ROM patching were missing.  " +
                            "Please double check directory files.");
        }
    }*/
    
    private void UpdateCustomAssets()
    {
        #region UI Display Textures

        if (Directory.Exists(customTexturesPath + @"\Buttons"))
        {
            Directory.Delete(customTexturesPath + @"\Buttons", true);
        }

        var buttonAssetsFolder =
            Configuration.UiButtonStyles.Keys.ToArray()[Configuration.Instance.UiButtonDisplayIndex];
        if (!string.IsNullOrEmpty(buttonAssetsFolder))
        {
            var newButtonFilePath = sxResourcesCustomTexturesPath + @"\Buttons\" + buttonAssetsFolder;
            var newButtonUiFiles = Directory.EnumerateFiles(newButtonFilePath);
            
            Directory.CreateDirectory(customTexturesPath + @"\Buttons");
            
            foreach (var buttonFile in newButtonUiFiles)
            {
                File.Copy(buttonFile, customTexturesPath + @"\Buttons" + buttonFile.Replace(newButtonFilePath, ""));
            }
        }

        #endregion
        
        #region Gloss Removal

        if (Directory.Exists(customTexturesPath + @"\GlossAdjustment"))
        {
            Directory.Delete(customTexturesPath + @"\GlossAdjustment", true);
        }

        var glossAssetsFolder = 
            Configuration.GlossAdjustmentOptions.Keys.ToArray()[Configuration.Instance.GlossAdjustmentIndex];
        if (!string.IsNullOrEmpty(glossAssetsFolder))
        {
            var removeGlossFilePath = sxResourcesCustomTexturesPath + @"\GlossAdjustment\" + glossAssetsFolder;
            var removeGlossFiles = Directory.EnumerateFiles(removeGlossFilePath);
            
            Directory.CreateDirectory(customTexturesPath + @"\GlossAdjustment");
            
            foreach (var removeGlossFile in removeGlossFiles)
            {
                File.Copy(removeGlossFile, customTexturesPath + @"\GlossAdjustment" + removeGlossFile.Replace(removeGlossFilePath, ""));
            }
        }

        #endregion
    }
}