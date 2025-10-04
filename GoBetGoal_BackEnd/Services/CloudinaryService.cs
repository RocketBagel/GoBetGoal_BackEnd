using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using System;
using System.Configuration;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
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
        public async Task<(string SecureUrl, string PublicId)> UploadImageAsync(byte[] fileBytes, string fileName, string folderPath)
        {
            // 使用 'using' 區塊來確保 MemoryStream 會被自動關閉和釋放資源
            using (var stream = new MemoryStream(fileBytes))
            {
               
                var uploadParams = new ImageUploadParams()
                {
                    // 現在 'stream' 變數是存在的，並且是從 fileBytes 建立的
                    File = new FileDescription(fileName, stream),
                    // ✅ 這裡用 Folder 指定「資料夾」
                    Folder = folderPath,
                    // ✅ PublicId 建議只放檔名，避免混淆
                    PublicId = $"{Path.GetFileNameWithoutExtension(fileName)}_{Guid.NewGuid()}"
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                // 建議加上錯誤檢查
                if (uploadResult.Error != null)
                {
                    throw new Exception($"Cloudinary upload failed: {uploadResult.Error.Message}");
                }

                return (uploadResult.SecureUrl.ToString(), uploadResult.PublicId);
            }
        }

        // 新增刪除方法
        public async Task<DeletionResult> DeleteImageAsync(string publicId)
        {
            var deletionParams = new DeletionParams(publicId);
            return await _cloudinary.DestroyAsync(deletionParams);
        }
    }
}