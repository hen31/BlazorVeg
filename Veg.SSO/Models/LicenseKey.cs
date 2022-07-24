using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Veg.SSO.Models
{
    public class LicenseKey
    {
        public long ID { get; set; }
        public string Application { get; set; }
        public DateTime BuyDate { get; set; }
        public int DurationInDays { get; set; }
        public string Key { get; set; }
        public bool Used { get; set; }


        public static LicenseKey GenerateKey(string application, int durationInDays)
        {
            var key = new LicenseKey();
            key.Application = application;
            key.DurationInDays = durationInDays;
            key.BuyDate = DateTime.Now;
            key.Key = GenerateKeyString();
            return key;
        }

        static string allowedCharacters = "1234567890qwertyuiopasdfghjklzxcvbnm";
        static Random _random;
        static Random Random
        {
            get
            {
                if (_random == null)
                {
                    _random = new Random();
                }
                return _random;
            }
        }


        private static string GenerateKeyString()
        {
            string[] characters = new string[16];
            for (int i = 0; i < characters.Length; i++)
            {
                characters[i] = GetRandomChar();
            }
            return string.Format("{0}{1}{2}{3}-{4}{5}{6}{7}-{8}{9}{10}{11}-{12}{13}{14}{15}", characters);
        }

        private static string GetRandomChar()
        {
            return allowedCharacters[Random.Next(0, allowedCharacters.Length - 1)].ToString();
        }
    }
}
