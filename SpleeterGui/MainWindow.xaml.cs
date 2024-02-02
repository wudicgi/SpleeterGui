using GongSolutions.Wpf.DragDrop;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using NGettext.Wpf;
using SpleeterGui.DataStructure;
using SpleeterGui.Processor;
using SpleeterGui.Util;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SpleeterGui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged, IDropTarget
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region Fields

        private ConsoleWindow _consoleWindow = null;

        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        #endregion

        #region Properties

        private ObservableCollection<AudioFileItem> _audioFileList = new ObservableCollection<AudioFileItem>();

        public ObservableCollection<AudioFileItem> AudioFileList
        {
            get => _audioFileList;
            set => SetProperty(ref _audioFileList, value);
        }

        private ObservableCollection<AudioFileItem> _audioFileList_selectedItems = new ObservableCollection<AudioFileItem>();

        public ObservableCollection<AudioFileItem> AudioFileList_SelectedItems
        {
            get => _audioFileList_selectedItems;
            set => SetProperty(ref _audioFileList_selectedItems, value);
        }

        private ObservableCollection<SpleeterModelItem> _spleeterModels = new ObservableCollection<SpleeterModelItem>();

        public ObservableCollection<SpleeterModelItem> SpleeterModels
        {
            get => _spleeterModels;
            set => SetProperty(ref _spleeterModels, value);
        }

        private SpleeterModelItem _selectedSpleeterModel = null;

        public SpleeterModelItem SelectedSpleeterModel
        {
            get => _selectedSpleeterModel;

            set
            {
                if (_selectedSpleeterModel != value)
                {
                    _selectedSpleeterModel = value;
                    RaisePropertyChanged(nameof(SelectedSpleeterModel));

                    bool frequencySelectable = IsSpleeterModelFrequencySelectable(_selectedSpleeterModel.FolderName);
                    stackPanelSpleeterModelFrequency.IsEnabled = frequencySelectable;
                }
            }
        }

        #endregion

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;
        }

        #region Window events handle

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateLanguageMenuItemCheckStates();

            AudioFileList.CollectionChanged += (sender2, e2) =>
            {
                buttonRemoveAll.IsEnabled = (AudioFileList.Count > 0);
            };

            if (_consoleWindow == null)
            {
                ConsoleWindow consoleWindow = new ConsoleWindow();
                consoleWindow.Owner = this;
                consoleWindow.IsVisibleChanged += (sender3, e3) =>
                {
                    if (consoleWindow.Visibility == Visibility.Visible)
                    {
                        menuItemViewShowConsoleWindow.IsChecked = true;
                    }
                    else
                    {
                        menuItemViewShowConsoleWindow.IsChecked = false;
                    }
                };

                _consoleWindow = consoleWindow;
            }
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            InitSpleeterModelsList();
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            // Do not use Window.InputBindings to get rid of the control focus problem

            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                switch (e.Key)
                {
                    case Key.O:
                        AddFiles();
                        break;

                    case Key.D:
                        AddFolders();
                        break;

                    default:
                        return;
                }
            }
            else
            {
                return;
            }

            e.Handled = true;
        }

        private void UpdateLanguageMenuItemCheckStates()
        {
            CultureInfo cultureInfo = Thread.CurrentThread.CurrentUICulture;
            string languageTag = cultureInfo.IetfLanguageTag;

            foreach (MenuItem menuItem in menuItemLanguage.Items)
            {
                menuItem.IsChecked = ((menuItem.Tag as string) == languageTag);
            }
        }

        private void InitSpleeterModelsList()
        {
            string modelsFolderFullPath = FileSystemUtil.GetFullPathBasedOnProgramFile("models");
            if (Directory.Exists(modelsFolderFullPath))
            {
                try
                {
                    IEnumerable<string> modelFolders = Directory.EnumerateDirectories(modelsFolderFullPath);
                    foreach (string modelFolder in modelFolders)
                    {
                        string folderName = Path.GetFileName(modelFolder);

                        SpleeterModels.Add(new SpleeterModelItem(folderName));
                    }
                }
                catch (Exception ex)
                {
                    SpleeterModels.Add(new SpleeterModelItem(null, String.Format(Translation._("Error occurred: {0}"), ex.Message)));
                }

                if (SpleeterModels.Count == 0)
                {
                    SpleeterModels.Add(new SpleeterModelItem(null, Translation._("Not found any folder in \"models\" folder")));
                }
            }
            else
            {
                SpleeterModels.Add(new SpleeterModelItem(null, Translation._("Not found \"models\" folder")));
            }

            if ((SelectedSpleeterModel == null) && (SpleeterModels.Count > 0))
            {
                SelectedSpleeterModel = SpleeterModels[0];
            }
        }

        #endregion

        #region Menu events handle

        private void MenuItemFileAddFiles_Click(object sender, RoutedEventArgs e)
        {
            AddFiles();
        }

        private void MenuItemFileAddFolders_Click(object sender, RoutedEventArgs e)
        {
            AddFolders();
        }

        private void MenuItemFileExit_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void MenuItemViewShowConsoleWindow_Click(object sender, RoutedEventArgs e)
        {
            if (menuItemViewShowConsoleWindow.IsChecked)
            {
                _consoleWindow.Show();
            }
            else
            {
                _consoleWindow?.Hide();
            }
        }

        private void MenuItemHelpAbout_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow aboutWindow = new AboutWindow();
            aboutWindow.Owner = this;

            aboutWindow.ShowDialog();
        }

        private void MenuItemLanguageItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItemClicked = sender as MenuItem;
            if (menuItemClicked == null)
            {
                return;
            }

            string cultureName = menuItemClicked.Tag as string;
            if (cultureName == null)
            {
                return;
            }

            I18n.ChangeCultureInfo(cultureName);

            foreach (MenuItem menuItem in menuItemLanguage.Items)
            {
                menuItem.IsChecked = false;
            }
            menuItemClicked.IsChecked = true;
        }

        #endregion

        #region Control events handle

        private int _continuousKeyDownCounter = 0;

        private bool ContinuousKeyDownExceedLimit(int upperLimit)
        {
            _continuousKeyDownCounter++;
            if (_continuousKeyDownCounter > upperLimit)
            {
                return true;
            }

            return false;
        }

        private void ContinuousKeyDownCounterReset()
        {
            _continuousKeyDownCounter = 0;
        }

        private void ListViewAudioFileList_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                if (e.Key == Key.V)
                {
                    // Ctrl + V

                    if (ContinuousKeyDownExceedLimit(1))
                    {
                        return;
                    }

                    IDataObject dataObject = Clipboard.GetDataObject();
                    if (dataObject == null)
                    {
                        return;
                    }

                    if (dataObject.GetDataPresent(DataFormats.FileDrop))
                    {
                        string[] filePathList = dataObject.GetData(DataFormats.FileDrop) as string[];
                        if (filePathList == null)
                        {
                            return;
                        }

                        AddFilesToAudioFileList(filePathList, true);
                    }

                    e.Handled = true;
                }
            }
            else if (e.KeyboardDevice.Modifiers == ModifierKeys.None)
            {
                switch (e.Key)
                {
                    case Key.Delete:
                        // Delete
                        if (ContinuousKeyDownExceedLimit(1))
                        {
                            return;
                        }
                        RemoveSelectedItems();
                        e.Handled = true;
                        break;

                    default:
                        break;
                }
            }
        }

        private void ListViewAudioFileList_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            ContinuousKeyDownCounterReset();
        }

        private void ListViewAudioFileList_MouseDown(object sender, MouseButtonEventArgs e)
        {
            HitTestResult hitTestResult = VisualTreeHelper.HitTest(this, e.GetPosition(this));
            if (hitTestResult.VisualHit.GetType() != typeof(ListBoxItem))
            {
                listViewAudioFileList.UnselectAll();
            }
        }

        private void ListViewAudioFileList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            buttonRemoveSelected.IsEnabled = (listViewAudioFileList.SelectedItem != null);
        }

        private void ContextMenuItemPreviewCommandLines_Click(object sender, RoutedEventArgs e)
        {
            IList selectedItems = listViewAudioFileList.SelectedItems;
            if ((selectedItems == null) || (selectedItems.Count == 0))
            {
                return;
            }

            List<object> selectedItemObjects = selectedItems.Cast<object>().ToList();

            ConfigParameters configParameters = GetConfigParameters();
            if (configParameters == null)
            {
                return;
            }

            string spleeterExeFullPath = FileSystemUtil.GetFullPathBasedOnProgramFile("Spleeter.exe");

            StringBuilder sb = new StringBuilder();
            foreach (object selectedItemObject in selectedItemObjects)
            {
                AudioFileItem selectedItem = selectedItemObject as AudioFileItem;
                if (selectedItem == null)
                {
                    continue;
                }

                string commandLineArguments = GetCommandLineArguments(selectedItem.FileFullPath, configParameters);

                sb.AppendLine(spleeterExeFullPath + " " + commandLineArguments);
            }

            MessageBox.Show(sb.ToString(),
                Translation._("Command lines"), MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ContextMenuItemResetToInitialState_Click(object sender, RoutedEventArgs e)
        {
            IList selectedItems = listViewAudioFileList.SelectedItems;
            if ((selectedItems == null) || (selectedItems.Count == 0))
            {
                return;
            }

            List<object> selectedItemObjects = selectedItems.Cast<object>().ToList();

            foreach (object selectedItemObject in selectedItemObjects)
            {
                AudioFileItem selectedItem = selectedItemObject as AudioFileItem;
                if (selectedItem == null)
                {
                    continue;
                }

                selectedItem.Status = AudioFileStatus.NotProcessed;
            }
        }

        private void ContextMenuItemRemoveFromList_Click(object sender, RoutedEventArgs e)
        {
            RemoveSelectedItems();
        }

        private void RadioButtonOutputFormatFlac_CheckedOrUnchecked(object sender, RoutedEventArgs e)
        {
            if (radioButtonOutputFormatFlac.IsChecked == true)
            {
                textBlockOutputBitrate.IsEnabled = false;
                stackPanelOutputBitrate.IsEnabled = false;
            }
            else
            {
                textBlockOutputBitrate.IsEnabled = true;
                stackPanelOutputBitrate.IsEnabled = true;
            }
        }

        private void RadioButtonOutputFolderSpecified_CheckedOrUnchecked(object sender, RoutedEventArgs e)
        {
            bool isChecked = (radioButtonOutputFolderSpecified.IsChecked == true);

            textBoxOutputFolderSpecifiedPath.IsEnabled = isChecked;
            buttonOutputFolderBrowse.IsEnabled = isChecked;
        }

        private void ButtonAddFiles_Click(object sender, RoutedEventArgs e)
        {
            AddFiles();
        }

        private void ButtonAddFolders_Click(object sender, RoutedEventArgs e)
        {
            AddFolders();
        }

        private void ButtonRemoveSelected_Click(object sender, RoutedEventArgs e)
        {
            RemoveSelectedItems();
        }

        private void ButtonRemoveAll_Click(object sender, RoutedEventArgs e)
        {
            RemoveAllItems();
        }

        private void ButtonOutputFolderBrowse_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dlg = new CommonOpenFileDialog();
            dlg.Title = Translation._("Select output folder");
            dlg.IsFolderPicker = true;
            dlg.Multiselect = false;
            dlg.EnsureFileExists = true;
            dlg.EnsurePathExists = true;

            CommonFileDialogResult dialogResult = dlg.ShowDialog(Application.Current.MainWindow);
            if (dialogResult != CommonFileDialogResult.Ok)
            {
                return;
            }

            string folderPath = dlg.FileNames.FirstOrDefault();
            if (folderPath == null) {
                return;
            }
            if (!Directory.Exists(folderPath))
            {
                return;
            }

            textBoxOutputFolderSpecifiedPath.Text = Path.GetFullPath(folderPath);
        }

        private void ButtonStartProcess_Click(object sender, RoutedEventArgs e)
        {
            StartProcess();
        }

        private void ButtonStop_Click(object sender, RoutedEventArgs e)
        {
            StopProcess();
        }

        #endregion

        #region Drag and drop handle

        public new void DragOver(IDropInfo dropInfo)
        {
            // First, handle the situation of dragging and dropping files
            // https://github.com/punker76/gong-wpf-dragdrop/issues/93
            DataObject dataObject = dropInfo.Data as DataObject;
            if ((dataObject != null) && dataObject.GetDataPresent(DataFormats.FileDrop))
            {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
                dropInfo.Effects = DragDropEffects.Link;

                return;
            }

            // For other cases (such as dragging to rearrange the order), invoke the default DropHandler
            GongSolutions.Wpf.DragDrop.DragDrop.DefaultDropHandler.DragOver(dropInfo);
        }

        public new void Drop(IDropInfo dropInfo)
        {
            // First, handle the situation of dragging and dropping files
            // https://github.com/punker76/gong-wpf-dragdrop/issues/93
            DataObject dataObject = dropInfo.Data as DataObject;
            if ((dataObject != null) && dataObject.ContainsFileDropList())
            {
                string[] files = (string[])dataObject.GetData(DataFormats.FileDrop);
                if ((files == null) || (files.Length == 0))
                {
                    return;
                }

                int insertIndex = dropInfo.InsertIndex;

                AddFilesToAudioFileList(files, true, insertIndex);

                return;
            }

            // For other cases (such as dragging to rearrange the order), invoke the default DropHandler
            GongSolutions.Wpf.DragDrop.DragDrop.DefaultDropHandler.Drop(dropInfo);

            // Attempt to retrieve the dragged AudioFileItem
            var dataAsList = DefaultDropHandler.ExtractData(dropInfo.Data);
            AudioFileItem[] droppedItems = dataAsList.OfType<AudioFileItem>().ToArray();

            // If there are dragged AudioFileItems, select them
            if (droppedItems.Length > 0)
            {
                SetSelectedItems(droppedItems);
            }
        }

        #endregion

        private bool IsSpleeterModelFrequencySelectable(string spleeterModelFolderName)
        {
            switch (spleeterModelFolderName) {
                case "2stems":
                case "4stems":
                case "5stems":
                    return true;

                default:
                    return false;
            }
        }

        private ConfigParameters GetConfigParameters()
        {
            string spleeterModelFolderName = SelectedSpleeterModel?.FolderName;
            if (spleeterModelFolderName == null)
            {
                MessageBox.Show("spleeterModelFolderName is null", "Internal Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

            string spleeterModelName;
            if (IsSpleeterModelFrequencySelectable(spleeterModelFolderName))
            {
                if (radioButtonSpleeterModelFrequency11khz.IsChecked == true)
                {
                    spleeterModelName = spleeterModelFolderName;
                }
                else if (radioButtonSpleeterModelFrequency16khz.IsChecked == true)
                {
                    spleeterModelName = spleeterModelFolderName + "-16khz";
                }
                else if (radioButtonSpleeterModelFrequency22khz.IsChecked == true)
                {
                    spleeterModelName = spleeterModelFolderName + "-22khz";
                }
                else
                {
                    MessageBox.Show("Wrong spleeter model frequency selection status",
                        "Internal Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }
            }
            else
            {
                spleeterModelName = spleeterModelFolderName;
            }

            OutputFormat outputFormat;
            if (radioButtonOutputFormatSameAsInput.IsChecked == true)
            {
                outputFormat = OutputFormat.SameAsInput;
            }
            else if (radioButtonOutputFormatMp3.IsChecked == true)
            {
                outputFormat = OutputFormat.Mp3;
            }
            else if (radioButtonOutputFormatM4a.IsChecked == true)
            {
                outputFormat = OutputFormat.M4a;
            }
            else if (radioButtonOutputFormatFlac.IsChecked == true)
            {
                outputFormat = OutputFormat.Flac;
            }
            else
            {
                MessageBox.Show("Unknown outputFormat", "Internal Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

            int? outputBitrate;
            if (outputFormat == OutputFormat.Flac)
            {
                outputBitrate = null;
            }
            else if (radioButtonOutputBitrate128kbps.IsChecked == true)
            {
                outputBitrate = 128;
            }
            else if (radioButtonOutputBitrate192kbps.IsChecked == true)
            {
                outputBitrate = 192;
            }
            else if (radioButtonOutputBitrate256kbps.IsChecked == true)
            {
                outputBitrate = 256;
            }
            else if (radioButtonOutputBitrate320kbps.IsChecked == true)
            {
                outputBitrate = 320;
            }
            else
            {
                MessageBox.Show("Unknown outputBitrate", "Internal Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

            string outputFolderPath;
            if (radioButtonOutputFolderSameAsInput.IsChecked == true)
            {
                outputFolderPath = null;
            }
            else if (radioButtonOutputFolderSpecified.IsChecked == true)
            {
                outputFolderPath = textBoxOutputFolderSpecifiedPath.Text;

                if (string.IsNullOrWhiteSpace(outputFolderPath))
                {
                    MessageBox.Show(Translation._("The specified output folder is empty."),
                        Translation._("Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                    textBoxOutputFolderSpecifiedPath.Focus();
                    return null;
                }

                if (!Directory.Exists(outputFolderPath))
                {
                    MessageBox.Show(string.Format(Translation._("The specified output folder \"{0}\" does not exist."), outputFolderPath),
                        Translation._("Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                    textBoxOutputFolderSpecifiedPath.Focus();
                    return null;
                }
            }
            else
            {
                MessageBox.Show("Unknown outputFolderPath", "Internal Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

            string otherCommandLineOptions = textBoxOtherCommandLineOptions.Text;

            bool outputOverwriteExisted = (checkBoxOutputOverwriteExisted.IsChecked == true);

            ConfigParameters configParameters = new ConfigParameters()
            {
                SpleeterModelName = spleeterModelName,
                OutputFormat = outputFormat,
                OutputBitrate = outputBitrate,
                OutputFolderPath = outputFolderPath,
                OutputOverwriteExisted = outputOverwriteExisted,
                OtherCommandLineOptions = otherCommandLineOptions
            };

            return configParameters;
        }

        private string GetCommandLineArguments(string inputAudioFilePath, ConfigParameters configParameters)
        {
            // D:\folder1\folder2\file.mp3
            string inputAudioFileFullPath = Path.GetFullPath(inputAudioFilePath);

            // D:\folder1\folder2
            string inputAudioFileFolderFullPath = Path.GetDirectoryName(inputAudioFileFullPath);

            // file
            string inputAudioFileNameWithoutExtension = Path.GetFileNameWithoutExtension(inputAudioFileFullPath);

            // .mp3
            string inputAudioFileExtension = Path.GetExtension(inputAudioFileFullPath);

            List<string> argumentList = new List<string>();

            if (configParameters.SpleeterModelName != null)
            {
                argumentList.Add("--model");
                argumentList.Add(ExternalProgram.EncodeParameterArgument(configParameters.SpleeterModelName));
            }

            string output;
            if ((configParameters.OutputFolderPath == null)
                && (configParameters.OutputFormat == OutputFormat.SameAsInput))
            {
                output = null;
            }
            else
            {
                string outputFolderPath = configParameters.OutputFolderPath
                    ?? inputAudioFileFolderFullPath;

                string outputFilename = inputAudioFileNameWithoutExtension + ".$(TrackName)";
                switch (configParameters.OutputFormat)
                {
                    case OutputFormat.SameAsInput:
                        outputFilename += inputAudioFileExtension;
                        break;
                    case OutputFormat.Mp3:
                        outputFilename += ".mp3";
                        break;
                    case OutputFormat.M4a:
                        outputFilename += ".m4a";
                        break;
                    case OutputFormat.Flac:
                        outputFilename += ".flac";
                        break;
                    default:
                        // TODO
                        break;
                }

                output = outputFolderPath + Path.DirectorySeparatorChar + outputFilename;
            }

            if (output != null)
            {
                argumentList.Add("--output");
                argumentList.Add(ExternalProgram.EncodeParameterArgument(output, true));
            }

            if (configParameters.OutputBitrate != null)
            {
                argumentList.Add("--bitrate");
                argumentList.Add(ExternalProgram.EncodeParameterArgument(((int)configParameters.OutputBitrate).ToString() + "k"));
            }

            if (configParameters.OutputOverwriteExisted)
            {
                argumentList.Add("--overwrite");
            }

            if (!string.IsNullOrWhiteSpace(configParameters.OtherCommandLineOptions))
            {
                argumentList.Add(configParameters.OtherCommandLineOptions);
            }

            argumentList.Add(ExternalProgram.EncodeParameterArgument(inputAudioFileFullPath, true));

            string commandLineArguments = string.Join(" ", argumentList);

            return commandLineArguments;
        }

        #region Audio file list operations

        private void AddFiles()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = Translation._("Add files");
            // dlg.InitialDirectory = Environment.CurrentDirectory;
            // dlg.RestoreDirectory = false;
            dlg.Filter = Translation._("All files") + " (*.*)|*.*|"
                + Translation._("Audio files") + " (*.mp3;*.m4a;*.flac;*.wav)|*.mp3;*.m4a;*.flac;*.wav";
            dlg.FilterIndex = 2;
            dlg.Multiselect = true;
            dlg.CheckFileExists = true;
            dlg.CheckPathExists = true;

            bool? dialogResult = dlg.ShowDialog(Application.Current.MainWindow);
            if (dialogResult != true)
            {
                return;
            }

            if (dlg.FileNames.Length > 0)
            {
                AddFilesToAudioFileList(dlg.FileNames, false);
            }
        }

        private void AddFolders()
        {
            CommonOpenFileDialog dlg = new CommonOpenFileDialog();
            dlg.Title = Translation._("Add folders");
            dlg.IsFolderPicker = true;
            dlg.Multiselect = true;
            dlg.EnsureFileExists = true;
            dlg.EnsurePathExists = true;

            CommonFileDialogResult dialogResult = dlg.ShowDialog(Application.Current.MainWindow);
            if (dialogResult != CommonFileDialogResult.Ok)
            {
                return;
            }

            List<string> fileList = new List<string>();
            foreach (string folderPath in dlg.FileNames)
            {
                if (!Directory.Exists(folderPath))
                {
                    continue;
                }

                string[] folderFiles = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories);
                foreach (string folderFile in folderFiles)
                {
                    string folderFileExtension = Path.GetExtension(folderFile).ToLower();
                    switch (folderFileExtension)
                    {
                        case ".mp3":
                        case ".m4a":
                        case ".flac":
                        case ".wav":
                            fileList.Add(folderFile);
                            break;

                        default:
                            break;
                    }
                }
            }

            if (fileList.Count > 100)
            {
                MessageBoxResult messageBoxResult = MessageBox.Show(
                    string.Format(Translation._("There are a large number of audio files in the selected folder ({0} found).{1}Are you sure to add them all?"), fileList.Count, Environment.NewLine),
                    Translation._("Too many files"),
                    MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);

                if (messageBoxResult != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            AddFilesToAudioFileList(fileList.ToArray(), false);
        }

        public void AddFilesToAudioFileList(string[] pathList, bool selectAddedFiles, int? insertIndex = null)
        {
            List<AudioFileItem> addedAudioFileItems = new List<AudioFileItem>();

            int failedToAddCount = 0;
            foreach (string path in pathList)
            {
                string fullpath = Path.GetFullPath(path);
                if (!File.Exists(fullpath))
                {
                    failedToAddCount++;
                    continue;
                }

                string filename = Path.GetFileName(path);

                AudioFileItem item = new AudioFileItem()
                {
                    FileFullPath = fullpath,
                    FileName = filename,
                    Status = AudioFileStatus.NotProcessed
                };

                if (insertIndex == null)
                {
                    AudioFileList.Add(item);
                }
                else
                {
                    AudioFileList.Insert((int)insertIndex, item);
                    insertIndex++;
                }

                addedAudioFileItems.Add(item);
            }

            AudioFileItem scrollToItem;
            if (selectAddedFiles)
            {
                SetSelectedItems(addedAudioFileItems);
                scrollToItem = addedAudioFileItems.LastOrDefault();
            }
            else
            {
                scrollToItem = AudioFileList.LastOrDefault();
            }
            if (scrollToItem != null)
            {
                listViewAudioFileList.ScrollIntoView(scrollToItem);
            }

            if (failedToAddCount != 0)
            {
                MessageBox.Show(string.Format(Translation._("A total of {0} files failed to be added, the files at the specified path may not exist."), failedToAddCount),
                    Translation._("Failed to add some files"),
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public void SetSelectedItems(IEnumerable<AudioFileItem> audioFileItems)
        {
            while (AudioFileList_SelectedItems.Count > 0)
            {
                AudioFileList_SelectedItems.RemoveAt(AudioFileList_SelectedItems.Count - 1);
            }

            foreach (AudioFileItem item in audioFileItems)
            {
                AudioFileList_SelectedItems.Add(item);
            }
        }

        private void RemoveSelectedItems()
        {
            int selectedIndex = listViewAudioFileList.SelectedIndex;
            if (selectedIndex == -1)
            {
                return;
            }

            IList selectedItems = listViewAudioFileList.SelectedItems;
            if ((selectedItems == null) || (selectedItems.Count == 0))
            {
                return;
            }

            List<object> selectedItemObjects = selectedItems.Cast<object>().ToList();

            foreach (object selectedItemObject in selectedItemObjects)
            {
                AudioFileItem selectedItem = selectedItemObject as AudioFileItem;
                if (selectedItem == null)
                {
                    continue;
                }

                if (AudioFileList.Contains(selectedItem))
                {
                    AudioFileList.Remove(selectedItem);
                }
            }

            if (selectedIndex >= 0 && selectedIndex < AudioFileList.Count)
            {
                listViewAudioFileList.SelectedItem = AudioFileList[selectedIndex];
            }
            else if (selectedIndex >= 1 && (selectedIndex - 1) < AudioFileList.Count)
            {
                listViewAudioFileList.SelectedItem = AudioFileList[selectedIndex - 1];
            }
            else
            {
                listViewAudioFileList.SelectedItem = null;
            }

        }

        private void RemoveAllItems()
        {
            AudioFileList.Clear();
        }

        #endregion

        #region Process

        public void StartProcess()
        {
            ConfigParameters configParameters = GetConfigParameters();
            if (configParameters == null)
            {
                return;
            }

            List<AudioFileItem> itemsToProcess = new List<AudioFileItem>();
            foreach (AudioFileItem audioFileItem in AudioFileList)
            {
                if ((audioFileItem.Status != AudioFileStatus.Processed)
                    && (audioFileItem.Status != AudioFileStatus.Processing))
                {
                    audioFileItem.Status = AudioFileStatus.QueuedToProcess;
                }

                if (audioFileItem.Status == AudioFileStatus.QueuedToProcess)
                {
                    itemsToProcess.Add(audioFileItem);
                }
            }

            if (itemsToProcess.Count == 0)
            {
                MessageBox.Show(Translation._("There is not any file to process."), Translation._("Error"),
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _cancellationTokenSource = new CancellationTokenSource();

            Task.Run(async () =>
            {
                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    buttonStop.IsEnabled = true;
                    buttonStartProcess.IsEnabled = false;
                });

                ProcessingStats processingStats = new ProcessingStats()
                {
                    TotalBeginDateTime = DateTime.Now,
                    CurrentFileBeginDateTime = DateTime.Now,
                    TotalFileCount = itemsToProcess.Count,
                    CurrentFileIndex = 0,
                };

                UpdateProcessingStats(processingStats, 0);

                bool haveUnhandledException = false;
                bool suppressCompleteMessageBox = false;
                while (true)
                {
                    if (_cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        break;
                    }

                    try
                    {
                        bool nothingToProcess = true;
                        for (int i = 0; i < itemsToProcess.Count; i++)
                        {
                            AudioFileItem audioFileItem = itemsToProcess[i];

                            if (_cancellationTokenSource.Token.IsCancellationRequested)
                            {
                                break;
                            }

                            if (audioFileItem.Status != AudioFileStatus.QueuedToProcess)
                            {
                                // should not reach here
                                continue;
                            }

                            nothingToProcess = false;

                            processingStats.CurrentFileIndex = i;

                            await ProcessFile(audioFileItem, configParameters, processingStats);

                            if (audioFileItem.Status == AudioFileStatus.QueuedToProcess)
                            {
                                audioFileItem.Status = AudioFileStatus.Failed;
                            }

                            if (audioFileItem.Status == AudioFileStatus.Failed)
                            {
                                bool haveRemainingFiles = ((i + 1) < itemsToProcess.Count);
                                MessageBoxResult messageBoxResult = MessageBox.Show((audioFileItem.FailedReason ?? Translation._("Unknown reason"))
                                        + (haveRemainingFiles ? (Environment.NewLine + Environment.NewLine + Translation._("Do you want to continue processing?")) : string.Empty),
                                    string.Format(Translation._("Failed to process file {0}"), audioFileItem.FileName),
                                    (haveRemainingFiles ? MessageBoxButton.YesNo : MessageBoxButton.OK),
                                    MessageBoxImage.Error);
                                if (haveRemainingFiles)
                                {
                                    if (messageBoxResult != MessageBoxResult.Yes)
                                    {
                                        suppressCompleteMessageBox = true;
                                        StopProcess();
                                    }
                                }
                                else
                                {
                                    if (itemsToProcess.Count == 1)
                                    {
                                        suppressCompleteMessageBox = true;
                                    }
                                }
                                break;
                            }
                        }

                        if (nothingToProcess)
                        {
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        haveUnhandledException = true;
                        MessageBox.Show(string.Format(Translation._("An exception occurred during processing:{0}{1}{0}{0}{2}"),
                                Environment.NewLine, ex.Message, ex.StackTrace),
                            Translation._("Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                    }
                }

                bool isAborted = _cancellationTokenSource.Token.IsCancellationRequested;

                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    buttonStop.IsEnabled = false;

                    if (haveUnhandledException)
                    {
                        for (int i = 0; i < itemsToProcess.Count; i++)
                        {
                            AudioFileItem audioFileItem = itemsToProcess[i];

                            if (audioFileItem.Status == AudioFileStatus.QueuedToProcess)
                            {
                                audioFileItem.Status = AudioFileStatus.NotProcessed;
                            }
                            else if (audioFileItem.Status == AudioFileStatus.Processing)
                            {
                                audioFileItem.Status = AudioFileStatus.Failed;
                            }
                        }
                    }
                    else if (isAborted)
                    {
                        if (!suppressCompleteMessageBox)
                        {
                            MessageBox.Show(Translation._("Processing has been aborted."),
                                Translation._("Aborted"), MessageBoxButton.OK, MessageBoxImage.Information);
                        }

                        for (int i = 0; i < itemsToProcess.Count; i++)
                        {
                            AudioFileItem audioFileItem = itemsToProcess[i];

                            if (audioFileItem.Status == AudioFileStatus.QueuedToProcess)
                            {
                                audioFileItem.Status = AudioFileStatus.NotProcessed;
                            }
                        }
                    }
                    else
                    {
                        int failedCount = 0;
                        for (int i = 0; i < itemsToProcess.Count; i++)
                        {
                            AudioFileItem audioFileItem = itemsToProcess[i];

                            if (audioFileItem.Status == AudioFileStatus.Failed)
                            {
                                failedCount++;
                            }
                        }

                        if (!suppressCompleteMessageBox)
                        {
                            if (failedCount == 0)
                            {
                                MessageBox.Show(Translation._("Processing completed."),
                                    Translation._("Completed"), MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            else
                            {
                                MessageBox.Show(string.Format(Translation._("Processing completed with {0} file failures."), failedCount),
                                    Translation._("Completed"), MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                    }

                    buttonStartProcess.IsEnabled = true;
                });
            });
        }

        public void StopProcess()
        {
            _cancellationTokenSource.Cancel();
        }

        private void UpdateProcessingStats(ProcessingStats processingStats, double currentFilePercentageValue)
        {
            double percentageCurrent = currentFilePercentageValue;

            double percentageOverall;
            if (processingStats.TotalFileCount != 0)
            {
                percentageOverall = (((double)processingStats.CurrentFileIndex + (percentageCurrent / 100.0))
                    / (double)processingStats.TotalFileCount) * 100.0;
            }
            else
            {
                percentageOverall = 0;
            }

            App.Current.Dispatcher.Invoke((Action)delegate
            {
                progressBarCurrentFile.Value = (int)Math.Floor(percentageCurrent * 100.0);
                textBlockProgressCurrentFile.Text = percentageCurrent.ToString("0.00") + "%";

                progressBarOverall.Value = (int)Math.Floor(percentageOverall * 100.0);
                textBlockProgressOverall.Text = percentageOverall.ToString("0.00") + "%";
            });
        }

        private async Task ProcessFile(AudioFileItem audioFileItem, ConfigParameters configParameters,
            ProcessingStats processingStats)
        {
            string spleeterExeFullPath = FileSystemUtil.GetFullPathBasedOnProgramFile("Spleeter.exe");
            if (!File.Exists(spleeterExeFullPath))
            {
                audioFileItem.FailedReason = string.Format(Translation._("Spleeter program \"{0}\" does not exist"), spleeterExeFullPath);
                audioFileItem.Status = AudioFileStatus.Failed;
                return;
            }

            string commandLineArguments = GetCommandLineArguments(audioFileItem.FileFullPath, configParameters);

            ExternalProgram externalProgram = new ExternalProgram(spleeterExeFullPath, commandLineArguments);

            // stdout and stderr are two separate streams. Keep the order of their output lines simply through the queue.
            ConcurrentQueue<Tuple<DateTime, string>> stdErrLinesQueue = new ConcurrentQueue<Tuple<DateTime, string>>();

            externalProgram.StdOutDataLineReceived += (line) =>
            {
                if (line == null)
                {
                    // The stdout stream has been closed. Output all stderr lines that have not been output yet.
                    while (stdErrLinesQueue.TryDequeue(out Tuple<DateTime, string> tuple))
                    {
                        _consoleWindow?.AddLine(tuple.Item2);
                    }
                    return;
                }

                Console.WriteLine(line);

                _consoleWindow?.AddLine(line);

                if (line.StartsWith("["))
                {
                    int pos_2 = line.IndexOf(']');
                    if (pos_2 > 0)
                    {
                        string percentage = line.Substring(1, pos_2 - 1);

                        if (double.TryParse(percentage.TrimEnd('%'), out double percentageValue))
                        {
                            audioFileItem.FileProcessingPercentage = (int)Math.Floor(percentageValue);

                            UpdateProcessingStats(processingStats, percentageValue);
                        }
                    }
                }
            };

            externalProgram.StdErrDataLineReceived += (line) =>
            {
                if (line == null)
                {
                    return;
                }

                Console.WriteLine(line);

                stdErrLinesQueue.Enqueue(new Tuple<DateTime, string>(DateTime.Now, line));
            };

            audioFileItem.Status = AudioFileStatus.Processing;

            _consoleWindow?.AddLine(string.Empty);
            _consoleWindow?.AddLine(string.Format("================================ {0:yyyy-MM-dd HH:mm:ss} ================================", DateTime.Now));
            _consoleWindow?.AddLine(string.Empty);

            _consoleWindow?.AddLine("// Execute:");
            _consoleWindow?.AddLine(string.Format("// {0} {1}", spleeterExeFullPath, commandLineArguments));
            _consoleWindow?.AddLine(string.Empty);

            bool started = externalProgram.StartExecute(out string exceptionMessage);
            if (!started)
            {
                audioFileItem.FailedReason = string.Format(Translation._("Failed to run Spleeter program:{0}{1}"),
                    Environment.NewLine, exceptionMessage);
                audioFileItem.Status = AudioFileStatus.Failed;
                return;
            }

            while (true)
            {
                // Output stderr lines that have timed out.
                while (stdErrLinesQueue.TryPeek(out Tuple<DateTime, string> tuple))
                {
                    if (DateTime.Now.Subtract(tuple.Item1).TotalMilliseconds >= 500)
                    {
                        if (stdErrLinesQueue.TryDequeue(out Tuple<DateTime, string> tuple_2))
                        {
                            _consoleWindow?.AddLine(tuple.Item2);
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                if (externalProgram.HasExited)
                {
                    break;
                }

                if (_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    break;
                }

                await Task.Delay(1000);
            }

            if (!externalProgram.HasExited)
            {
                // should not reach here

                externalProgram.Kill();

                if (_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    audioFileItem.Status = AudioFileStatus.Cancelled;
                }
                else
                {
                    audioFileItem.FailedReason = Translation._("The Spleeter program did not terminate normally");
                    audioFileItem.Status = AudioFileStatus.Failed;
                }
                return;
            }

            if (externalProgram.ExitCode != 0)
            {
                audioFileItem.FailedReason = string.Format(Translation._("The Spleeter program has exited unexpectedly, error code: {0}.{1}Please select View - Show Console Output to see the error message."),
                    externalProgram.ExitCode, Environment.NewLine);
                audioFileItem.Status = AudioFileStatus.Failed;
                return;
            }

            audioFileItem.Status = AudioFileStatus.Processed;
        }

        #endregion

        #region WPF utils

        protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
            {
                return false;
            }

            storage = value;
            RaisePropertyChanged(propertyName);

            return true;
        }

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            PropertyChanged?.Invoke(this, args);
        }

        #endregion
    }
}
