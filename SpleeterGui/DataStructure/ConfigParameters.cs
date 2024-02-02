namespace SpleeterGui
{
    public class ConfigParameters
    {
        /// <summary>
        /// Spleeter model name
        /// </summary>
        /// <remarks>
        /// 2stems, 4stems-16khz, 5stems-22khz, ...
        /// </remarks>
        public string SpleeterModelName { get; set; } = null;

        /// <summary>
        /// Output audio file format
        /// </summary>
        public OutputFormat OutputFormat { get; set; } = OutputFormat.SameAsInput;

        /// <summary>
        /// Output audio file bitrate in kbps
        /// </summary>
        /// <remarks>
        /// NULL for lossless
        /// </remarks>
        public int? OutputBitrate { get; set; } = null;

        /// <summary>
        /// Output audio file folder path
        /// </summary>
        /// <remarks>
        /// NULL stands for "Same as input"
        /// </remarks>
        public string OutputFolderPath { get; set; } = null;

        /// <summary>
        /// Overwrite when the target output file exists
        /// </summary>
        public bool OutputOverwriteExisted { get; set; } = false;

        /// <summary>
        /// Other command line options
        /// </summary>
        public string OtherCommandLineOptions { get; set; } = string.Empty;
    }
}
