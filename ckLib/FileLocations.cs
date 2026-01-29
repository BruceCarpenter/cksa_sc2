using System.Diagnostics;

namespace ckLib
{
    public class FileLocations
    {
        static public string GetIdeaMiniPath()
        {
            if (Debugger.IsAttached)
                return "https://www.countrykitchensa.com/ckideas/mini/";
            return "/ckideas/mini/";
        }

        static public string GetIdeaImagePathName(string idea)
        {
            string jpgName = idea.Replace(" ", "").ToLower();
            return GetIdeaBigImagePath() + jpgName + ".jpg";
        }

        static public string GetIdeaImagePathName350(string idea)
        {
            string jpgName = idea.Replace(" ", "").ToLower();
            return GetIdeaBigImagePath350() + jpgName + ".jpg";
        }

        static public string GetIdeaBigImagePath()
        {
            if (Debugger.IsAttached)
            {
                return "https://www.countrykitchensa.com/ckideas/big/";
            }
            return "/ckideas/big/";
        }
        static public string GetIdeaBigImagePath350()
        {
            if(Debugger.IsAttached)
            {
                return "https://www.countrykitchensa.com/ckideas/350/";
            }
            return "/ckideas/350/";
        }

        static public string GetImagePathName(string itemNumber)
        {
            try
            {
                itemNumber = itemNumber.Trim();

                string imagePath = GetImagePath(itemNumber);

                string completeFileName = GetImagePathAbs() + imagePath + "/" + itemNumber + ".jpg";

                return (completeFileName.ToLower());
            }
            catch (Exception)
            {
                return "ProductError.jpg";
            }  
        }

        static public string GetImagePathName350(string itemNumber)
        {
            try
            {
                itemNumber = itemNumber.Trim();

                string imagePath = GetImagePath(itemNumber);

                string completeFileName = GetImagePathAbs350() + imagePath + "/" + itemNumber + ".jpg";

                return (completeFileName.ToLower());
            }
            catch (Exception)
            {
                return "ProductError.jpg";
            }
        }


        static public string GetImagePathName252(string itemNumber)
        {
            try
            {
                itemNumber = itemNumber.Trim();

                string imagePath = GetImagePath(itemNumber);

                string completeFileName = GetImagePathAbs252() + imagePath + "/" + itemNumber + ".jpg";

                return (completeFileName.ToLower());
            }
            catch (Exception)
            {
                return "ProductError.jpg";
            }
        }

        static public string GetImagePathName135(string itemNumber)
        {
            try
            {
                itemNumber = itemNumber.Trim();

                string imagePath = GetImagePath(itemNumber);

                string completeFileName = GetImagePathAbs135() + imagePath + "/" + itemNumber + ".jpg";

                return (completeFileName.ToLower());
            }
            catch (Exception)
            {
                return "ProductError.jpg";
            }
        }

        static public string GetMiniImagePathName(string itemNumber)
        {
            try
            {
                if (string.IsNullOrEmpty(itemNumber) == false)
                {
                    itemNumber = itemNumber.Trim();

                    string imagePath = GetImagePath(itemNumber);
                    string completeFileName = GetMiniPathAbs() + imagePath + "/" + itemNumber + ".jpg";

                    return completeFileName.ToLower();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return string.Empty;
        }

        /// <summary>
        /// Use to determine if the file exists.
        /// </summary>
        static public string GetAlternateProductPath(string itemNumber)
        {
            try
            {
                itemNumber = itemNumber.Trim();

                string imagePath = GetImagePath(itemNumber);
                string completeFileName = GetAlternateProductFile() + imagePath;

                return completeFileName.ToLower();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Use to determine the image URL.
        /// </summary>
        static public string GetAlternateProductUrl(string itemNumber)
        {
            try
            {
                itemNumber = itemNumber.Trim();

                string imagePath = GetImagePath(itemNumber);
                string completeFileName = GetAlternateProductPathAbs() + imagePath + "/" + itemNumber;

                return completeFileName.ToLower();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        static public string GetAlternateProductUrl66(string itemNumber)
        {
            try
            {
                itemNumber = itemNumber.Trim();

                string imagePath = GetImagePath(itemNumber);
                string completeFileName = GetAlternateProductPathAbs66() + imagePath + "/" + itemNumber;

                return completeFileName.ToLower();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #region Private

        static private string GetAlternateProductFile()
        {
            if (Debugger.IsAttached)
            {
                return @"C:\Source\cksa_sc1\CountryKitchen\images_product\alternative\";
            }

            return @"C:\inetpub\vhosts\CountryKitchensa.com\httpdocs\images_product\alternative\";
        }

        static public string GetAlternateIdeaPath()
        {
            if (Debugger.IsAttached)
            {
                return @"D:\SVNsource\CountryKitchen\ckideas\big\";
            }

            return @"C:\inetpub\vhosts\CountryKitchensa.com\httpdocs\ckideas\big\";
        }

        static public string GetMiniPathAbs()
        {
            //if (Debugger.IsAttached)
            return @"https://www.countrykitchensa.com/catalog/minis/";
            //return @"/catalog/minis/";
        }

        static public string GetImagePathAbs()
        {
            return @"https://www.countrykitchensa.com/catalog/images/";
        }
        static public string GetImagePathAbs350()
        {
            //if(Debugger.IsAttached)
            //{
            //    return @"https://www.countrykitchensa.com/catalog/350/";
            //}
            return @"/catalog/350/";
        }
        static public string GetImagePathAbs252()
        {
            if (Debugger.IsAttached)
            {
                return @"https://www.countrykitchensa.com/catalog/252/";
            }
            return @"/catalog/252/";
        }
        static public string GetImagePathAbs135()
        {
            if (Debugger.IsAttached)
            {
                return @"https://www.countrykitchensa.com/catalog/135/";
            }
            return @"/catalog/135/";
        }

        static private string GetAlternateProductPathAbs()
        {
            if (Debugger.IsAttached)
            {
                return @"https://www.countrykitchensa.com/images_product/alternative/";
            }
            return @"/images_product/alternative/";
        }
        static private string GetAlternateProductPathAbs66()
        {
            if (Debugger.IsAttached)
            {
                return @"https://www.countrykitchensa.com/images_product/66/";
            }
            return @"/images_product/66/";
        }

        static public string GetImagePath(string sItemNumber)
        {
            string sSubFolder;
            int nDashFind = sItemNumber.IndexOf("-");
            string sTemp = string.Empty;

            if (nDashFind == -1)
            {
                sTemp = sItemNumber.Substring(0, 1);
            }
            else
            {
                sTemp = sItemNumber.Substring(0, nDashFind);
            }

            if (Char.IsDigit(sTemp[0]))
            {
                // Remove the second character if it is not a digit.
                if (sTemp.Length > 1)
                {
                    if (!Char.IsDigit(sTemp[1]))
                    {
                        sTemp = sTemp.Substring(0, 1);
                    } // end if
                } // end if
                int nDigit = Int32.Parse(sTemp);
                if (nDigit <= 9) sSubFolder = "9";
                else if (nDigit <= 19) sSubFolder = "10";
                else if (nDigit <= 29) sSubFolder = "20";
                else if (nDigit <= 39) sSubFolder = "30";
                else if (nDigit <= 49) sSubFolder = "40";
                else if (nDigit <= 59) sSubFolder = "50";
                else if (nDigit <= 69) sSubFolder = "60";
                else if (nDigit <= 79) sSubFolder = "70";
                else if (nDigit <= 89) sSubFolder = "80";
                else if (nDigit <= 99) sSubFolder = "90";
                else if (nDigit <= 199) sSubFolder = "100";
                else if (nDigit <= 299) sSubFolder = "200";
                else if (nDigit <= 399) sSubFolder = "300";
                else if (nDigit <= 499) sSubFolder = "400";
                else if (nDigit <= 599) sSubFolder = "500";
                else if (nDigit <= 699) sSubFolder = "600";
                else if (nDigit <= 799) sSubFolder = "700";
                else if (nDigit <= 899) sSubFolder = "800";
                else if (nDigit <= 999) sSubFolder = "900";
                else if (nDigit <= 1999) sSubFolder = "1000";
                else if (nDigit <= 2999) sSubFolder = "2000";
                else if (nDigit <= 3999) sSubFolder = "3000";
                else if (nDigit <= 4999) sSubFolder = "4000";
                else if (nDigit <= 5999) sSubFolder = "5000";
                else if (nDigit <= 6999) sSubFolder = "6000";
                else if (nDigit <= 7999) sSubFolder = "7000";
                else if (nDigit <= 8999) sSubFolder = "8000";
                else if (nDigit <= 9999) sSubFolder = "9000";
                else sSubFolder = "10000";
            }	// End digits
            else
            {
                // a-d, e-h, i-j
                // k-o, p-s, t-z
                char lowerCase = Char.ToLower(sTemp[0]);
                if (lowerCase <= 'b') sSubFolder = "ab";
                else if (lowerCase <= 'd') sSubFolder = "cd";
                else if (lowerCase <= 'h') sSubFolder = "eh";
                else if (lowerCase <= 'j') sSubFolder = "ij";
                else if (lowerCase <= 'o') sSubFolder = "ko";
                else if (lowerCase <= 's') sSubFolder = "ps";
                else sSubFolder = "tz";
            }
            return (sSubFolder.ToLower());
        }

        #endregion Private
    }
}
