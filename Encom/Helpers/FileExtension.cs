namespace Encom.Helpers
{
    public static class FileExtension
    {
        public static bool CheckFileContenttype(this IFormFile file, string contentType)
        {
            return file.ContentType == contentType;
        }

        public static bool CheckFileLength(this IFormFile file, int length)
        {
            return (file.Length / 1024) > length;
        }

        public static async Task<string> CreateFileAsync(this IFormFile file, IWebHostEnvironment env, params string[] folders)
        {
            int lastIndex = file.FileName.LastIndexOf(".");

            string name = file.FileName.Substring(lastIndex);

            string fileName = $"{DateTime.UtcNow:yyyyMMddHHmmssfff}_{Guid.NewGuid()}{name}";

            string fullPath = Path.Combine(env.WebRootPath);

            foreach (string folder in folders)
            {
                fullPath = Path.Combine(fullPath, folder);
            }

            fullPath = Path.Combine(fullPath, fileName);

            using (FileStream stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fileName;
        }

        public static async Task<string> CreateDynamicFileAsync(this IFormFile file, int LanguageGroup, IWebHostEnvironment env, params string[] folders)
        {
            string fullPath = Path.Combine(env.WebRootPath);
            foreach (string folder in folders)
            {
                fullPath = Path.Combine(fullPath, folder);
            }

            string hashString;
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                file.OpenReadStream().Position = 0;

                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);

                    memoryStream.Position = 0;
                    byte[] hashBytes = sha256.ComputeHash(memoryStream);

                    hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                }
            }

            string extension = Path.GetExtension(file.FileName);

            string fileName = $"{hashString}{LanguageGroup}{extension}";

            string fullFilePath = Path.Combine(fullPath, fileName);

            if (!File.Exists(fullFilePath))
            {
                file.OpenReadStream().Position = 0;
                using (var stream = new FileStream(fullFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }

            return Path.Combine(folders).Replace("\\", "/") + "/" + fileName;
        }
    }
}
