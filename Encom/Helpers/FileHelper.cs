namespace Encom.Helpers
{
    public class FileHelper
    {
        public static void DeleteFile(string fileName, IWebHostEnvironment env, params string[] folders)
        {
            string fullPath = Path.Combine(env.WebRootPath);

            foreach (string folder in folders)
            {
                fullPath = Path.Combine(fullPath, folder);
            }

            fullPath = Path.Combine(fullPath, fileName);

            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }
    }
}
