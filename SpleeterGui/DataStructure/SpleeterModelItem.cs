using NGettext.Wpf;

namespace SpleeterGui.DataStructure
{
    public class SpleeterModelItem
    {
        public string FolderName { get; set; }

        public string ErrorMessage { get; set; }

        public string DisplayText
        {
            get
            {
                if (ErrorMessage != null)
                {
                    return "(" + ErrorMessage + ")";
                }

                string displayText = FolderName;
                switch (FolderName)
                {
                    case "2stems":
                        displayText += " " + Translation._("(vocals, accompaniment)");
                        break;

                    case "4stems":
                        displayText += " " + Translation._("(vocals, drums, bass, other)");
                        break;

                    case "5stems":
                        displayText += " " + Translation._("(vocals, drums, bass, piano, other)");
                        break;

                    default:
                        break;
                }

                return displayText;
            }
        }

        public SpleeterModelItem(string folderName, string errorMessage = null)
        {
            FolderName = folderName;
            ErrorMessage = errorMessage;
        }
    }
}
