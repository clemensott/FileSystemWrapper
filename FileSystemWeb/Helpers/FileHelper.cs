using System.IO;

namespace FileSystemWeb.Helpers
{
    static class FileHelper
    {
        public static string GenerateUniqueFileName(string path)
        {
            if (!File.Exists(path)) return path;

            string directoryName = Path.GetDirectoryName(path);
            string fileName = Path.GetFileNameWithoutExtension(path);
            string extension = Path.GetExtension(path);

            int nummeration;
            if (!EndsWithNumberation(ref fileName, out nummeration)) nummeration = 0;

            string newPath;
            do
            {
                string newFileName = $"{fileName} ({nummeration++}){extension}";
                newPath = Path.Combine(directoryName, newFileName);
            } while (File.Exists(newPath));

            return newPath;
        }

        /// <summary>
        /// Finds Numeration. Example "FileName (23)" would find 23.
        /// </summary>
        /// <returns>Returns true if find Numeration</returns>
        private static bool EndsWithNumberation(ref string fileName, out int number)
        {
            if (fileName.Length < 3 || 
                fileName[fileName.Length - 1] != ')' ||
                char.IsNumber(fileName[fileName.Length - 2]))
            {
                number = -1;
                return false;
            }

            string numberText = string.Empty;
            for (int i = fileName.Length - 2; i > 0; i--)
            {
                if (!char.IsNumber(fileName[i]) && (char.IsNumber(fileName[i - 1]) || fileName[i - 1] == '('))
                {
                    number = -1;
                    return false;
                }

                numberText = fileName[i] + numberText;
            }

            number = int.Parse(numberText);
            fileName = fileName.Remove(fileName.Length - numberText.Length);
            return true;
        }
    }
}
