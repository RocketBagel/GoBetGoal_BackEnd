using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using System;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;

namespace GoBetGoal_BackEnd.Services
{
    public class CloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService()
        {
            // 從設定檔讀取金鑰
            var cloudName = ConfigurationManager.AppSettings["Cloudinary.CloudName"];
            var apiKey = ConfigurationManager.AppSettings["Cloudinary.ApiKey"];
            var apiSecret = ConfigurationManager.AppSettings["Cloudinary.ApiSecret"];

            // 驗證金鑰是否存在
            if (string.IsNullOrEmpty(cloudName) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
            {
                throw new ArgumentException("Cloudinary configuration is missing in appSettings.");
            }

            Account account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account);
        }

        /// <summary>
        /// 非同步地上傳一張圖片
        /// </summary>
        /// <param name="fileBytes">圖片的位元組陣列</param>
        /// <param name="fileName">原始檔名</param>
        /// <param name="folderPath">要儲存在 Cloudinary 上的資料夾路徑</param>
        /// <returns>上傳結果</returns>
        public async Task<ImageUploadResult> UploadImageAsync(byte[] fileBytes, string fileName, string folderPath)
        {
            using (var stream = new MemoryStream(fileBytes))
            {
                var uploadParams = new ImageUploadParams()
                {
                    // 使用 MemoryStream 來上傳
                    File = new FileDescription(fileName, stream),
                    // 設定在 Cloudinary 上的儲存路徑和檔名
                    // PublicId 會是 "folder/path/random_guid"，確保檔名唯一
                    PublicId = $"{folderPath}/{Path.GetFileNameWithoutExtension(fileName)}_{Guid.NewGuid()}"
                };

                // 執行上傳
                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                return uploadResult;
            }
        }
    }
}