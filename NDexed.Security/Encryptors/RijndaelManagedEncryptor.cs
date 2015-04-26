using System;
using System.Configuration;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using NDexed.Domain.Resources;

namespace NDexed.Security.Encryptors
{
    public class RijndaelManagedEncryptor: IEncryptor
    {
        private readonly string m_CryptoKey = ConfigurationManager.AppSettings["CryptoKey"];
        private readonly string m_InitialVector = ConfigurationManager.AppSettings["InitialVector"];

        public string EncryptValue(string value)
        {
            ValidateConfigSettings();

            string returnValue = null;

            if (!string.IsNullOrEmpty(value))
            {
                RijndaelManaged cryptoProvider = new RijndaelManaged
                {
                    Key = GenerateHash(m_CryptoKey),
                    IV = GenerateHash(m_InitialVector)
                };

                ICryptoTransform transform = cryptoProvider.CreateEncryptor(cryptoProvider.Key, cryptoProvider.IV);

                MemoryStream stream = new MemoryStream();
                using (stream)
                {
                    CryptoStream cryptoStream = new CryptoStream(stream, transform, CryptoStreamMode.Write);
                    using (cryptoStream)
                    {
                        byte[] data = Encoding.ASCII.GetBytes(value);

                        cryptoStream.Write(data, 0, data.Length);
                        cryptoStream.FlushFinalBlock();
                        cryptoStream.Close();

                        byte[] encryptedData = stream.ToArray();

                        returnValue = Convert.ToBase64String(encryptedData);
                    }
                }
            }

            return returnValue;
        }

        public string DecryptValue(string encryptedValue)
        {
            ValidateConfigSettings();

            string returnValue = null;

            if (!string.IsNullOrEmpty(encryptedValue))
            {
                byte[] encryptedBytes = Convert.FromBase64String(encryptedValue);
                RijndaelManaged cryptoProvider = new RijndaelManaged
                {
                    Key = GenerateHash(m_CryptoKey),
                    IV = GenerateHash(m_InitialVector)
                };

                ICryptoTransform transform = cryptoProvider.CreateDecryptor(cryptoProvider.Key, cryptoProvider.IV);

                MemoryStream stream = new MemoryStream(encryptedBytes);
                using (stream)
                {
                    CryptoStream cryptoStream = new CryptoStream(stream, transform, CryptoStreamMode.Read);
                    using (cryptoStream)
                    {
                        StreamReader reader = new StreamReader(cryptoStream);
                        using (reader)
                        {
                            returnValue = reader.ReadToEnd();

                        }
                    }
                }
            }
         
            return returnValue;
        }

        #region Private Methods

        private static byte[] GenerateHash(string value)
        {
            MD5 md5 = MD5.Create();

            byte[] bytes = Encoding.ASCII.GetBytes(value);
            byte[] hash = md5.ComputeHash(bytes);

            return hash;
        }

        private void ValidateConfigSettings()
        {
            if (string.IsNullOrEmpty(m_CryptoKey))
            {
                throw new ConfigurationErrorsException(string.Format(Resources.ErrorMessages.MissingConfigSetting, "CryptoKey"));
            }


            if (string.IsNullOrEmpty(m_InitialVector))
            {
                throw new ConfigurationErrorsException(string.Format(Resources.ErrorMessages.MissingConfigSetting, "InitialVector"));
            }
        }

        #endregion
    }
}
