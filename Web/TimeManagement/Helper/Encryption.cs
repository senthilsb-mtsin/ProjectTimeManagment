using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading;
using System.Xml;
using System.Security.Cryptography;
using System.Text;
using System.Web.Security;

namespace TimeManagement
{
    static class Encryption
    {

        public static string GetSHA1HashData(string data)
        {
            try
            {

                return (FormsAuthentication.HashPasswordForStoringInConfigFile(data, "SHA1")).ToLower();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public static string CreateRandomPassword(int passwordLength)
        {
            //string allowedChars = "abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNOPQRSTUVWXYZ0123456789";
            //char[] chars = new char[passwordLength];
            //Random rd = new Random();

            //for (int i = 0; i < passwordLength; i++)
            //{
            //    chars[i] = allowedChars[rd.Next(0, allowedChars.Length)];
            //}

            //return new string(chars);

            return "password";
        }
        public static string RandomString(int length)
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            StringBuilder res = new StringBuilder();
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                byte[] uintBuffer = new byte[sizeof(uint)];

                while (length-- > 0)
                {
                    rng.GetBytes(uintBuffer);
                    uint num = BitConverter.ToUInt32(uintBuffer, 0);
                    res.Append(valid[(int)(num % (uint)valid.Length)]);
                }
            }

            return res.ToString();
        }

    }
}