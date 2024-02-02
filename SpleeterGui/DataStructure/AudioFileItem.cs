using NGettext.Wpf;
using Prism.Mvvm;
using System;

namespace SpleeterGui.DataStructure
{
    public class AudioFileItem : BindableBase
    {
        #region Fields

        private string _fileFullPath;
        private string _fileName;
        private int _fileProcessingPercentage = 0;
        private AudioFileStatus _audioFileStatus;
        private string _failedReason = null;

        #endregion

        #region Properties

        public string FileFullPath
        {
            get => _fileFullPath;
            set => SetProperty(ref _fileFullPath, value);
        }

        public string FileName
        {
            get => _fileName;
            set
            {
                SetProperty(ref _fileName, value);

                RaisePropertyChanged(nameof(DisplayText_Filename));
            }
        }

        public int FileProcessingPercentage
        {
            get => _fileProcessingPercentage;
            set
            {
                SetProperty(ref _fileProcessingPercentage, value);

                RaisePropertyChanged(nameof(DisplayText_Status));
            }
        }

        public AudioFileStatus Status
        {
            get => _audioFileStatus;
            set
            {
                SetProperty(ref _audioFileStatus, value);

                RaisePropertyChanged(nameof(DisplayText_Status));
            }
        }

        public string FailedReason
        {
            get => _failedReason;
            set => SetProperty(ref _failedReason, value);
        }

        public string DisplayText_Filename
        {
            get => FileName;
        }

        public string DisplayText_Status
        {
            get
            {
                string status;
                switch (Status)
                {
                    case AudioFileStatus.NotProcessed:
                        status = Translation._("To Process");
                        break;

                    case AudioFileStatus.QueuedToProcess:
                        status = Translation._("Queued");
                        break;

                    case AudioFileStatus.Processing:
                        status = string.Format(Translation._("Processing ({0}%)"), FileProcessingPercentage);
                        break;

                    case AudioFileStatus.Processed:
                        status = Translation._("Processed");
                        break;

                    case AudioFileStatus.Failed:
                        status = Translation._("Failed");
                        break;

                    case AudioFileStatus.Cancelled:
                        status = Translation._("Cancelled");
                        break;

                    default:
                        status = Status.ToString();
                        break;
                }

                return status;
            }
        }

        #endregion
    }
}
