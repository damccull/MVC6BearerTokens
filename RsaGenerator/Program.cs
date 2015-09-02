using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Newtonsoft.Json;
using System.Diagnostics;

namespace RsaGenerator {
    class Program {
        static void Main(string[] args) {
            //Console.WriteLine(GenerateRsaKeys());
            //Console.ReadKey();
            Trace.WriteLine(GenerateRsaKeys(),"RSA-KEY");
        }
        private static string GenerateRsaKeys() {
            RSACryptoServiceProvider myRSA = new RSACryptoServiceProvider(2048);
            var publicKey = new RSAParametersSerializable(myRSA.ExportParameters(true));
            string json = JsonConvert.SerializeObject(publicKey);
            return json;
        }
    }
}
