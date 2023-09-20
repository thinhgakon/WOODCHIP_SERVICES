using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace XHTD_SERVICES.Helper
{
    public static class FileHelper
    {
        public static string ConvertImageToBase64(string filePath)
        {
            try
            {
                byte[] imageData = File.ReadAllBytes(filePath);

                string base64Image = Convert.ToBase64String(imageData);

                return base64Image;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
