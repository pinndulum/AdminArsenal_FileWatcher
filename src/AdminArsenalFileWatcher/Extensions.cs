using System.IO;
using System.Threading;

namespace AdminArsenalFileWatcher
{
    public static class Extensions
    {
        public const int EOF = -1;
        public const int CarriageReturn = 13;
        public const int LineFeed = 10;

        public enum States { Initial = -1, Normal = 0, FoundCR = 1, FoundCRLF = 2 }

        public static int? LineCount(this FileInfo file)
        {
            int lineCount = 0;
            try
            {
                using (var fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var state = States.Initial;
                    var filebyte = 0;
                    do
                    {
                        filebyte = fs.ReadByte();
                        if (state == States.Initial && filebyte != EOF)
                        {
                            // start by counting the first line even if there isn't a CR\LF there is a line when there is a single char before EOF.
                            lineCount++;
                        }

                        switch (filebyte)
                        {
                            case CarriageReturn:
                                // found CR, begin looking for LF. If next char is another CR keep checking for subsequent LF.
                                state = States.FoundCR;
                                break;
                            case LineFeed:
                                // if prev char was CR and this one is LF a CR/LF was found, otherwise continue checking.
                                state = state == States.FoundCR ? States.FoundCRLF : 0;
                                break;
                            default:
                                // normal state - nothing to do here.
                                state = States.Normal;
                                break;
                        }

                        if (state == States.FoundCRLF)
                        {
                            // found char sequence CR/LF - increment line count.
                            lineCount++;
                        }
                    }
                    while (filebyte != -1);
                }
            }
            catch (IOException)
            {
                // IOException here means file is locked. return null to indicate that the file could not be read at this time.
                return null;
            }
            return lineCount;
        }
    }
}
