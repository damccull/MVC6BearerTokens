using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Security.Cryptography;

namespace RsaGen
{
    public class Program
    {
        public void Main(string[] args)
        {
            Console.WriteLine($"{GenerateRsaKeys()}");
            //Console.Read();
        }

        private static string GenerateRsaKeys() {
            RSACryptoServiceProvider myRSA = new RSACryptoServiceProvider(2048);
            var publicKey = new RSAParametersSerializable(myRSA.ExportParameters(true));
            string json = JsonConvert.SerializeObject(publicKey);
            return json;
        }
    }
}
