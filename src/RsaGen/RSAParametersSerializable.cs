using System;
using System.Runtime.Serialization;
using System.Security.Cryptography;

public class RSAParametersSerializable {
    private RSAParameters _rsaParameters;

    public RSAParameters RSAParameters {
        get {
            return _rsaParameters;
        }
    }

    public RSAParametersSerializable(RSAParameters rsaParameters) {
        _rsaParameters = rsaParameters;
    }

    private RSAParametersSerializable() {
    }

    public byte[] D { get { return _rsaParameters.D; } set { _rsaParameters.D = value; } }

    public byte[] DP { get { return _rsaParameters.DP; } set { _rsaParameters.DP = value; } }

    public byte[] DQ { get { return _rsaParameters.DQ; } set { _rsaParameters.DQ = value; } }

    public byte[] Exponent { get { return _rsaParameters.Exponent; } set { _rsaParameters.Exponent = value; } }

    public byte[] InverseQ { get { return _rsaParameters.InverseQ; } set { _rsaParameters.InverseQ = value; } }

    public byte[] Modulus { get { return _rsaParameters.Modulus; } set { _rsaParameters.Modulus = value; } }

    public byte[] P { get { return _rsaParameters.P; } set { _rsaParameters.P = value; } }

    public byte[] Q { get { return _rsaParameters.Q; } set { _rsaParameters.Q = value; } }
}