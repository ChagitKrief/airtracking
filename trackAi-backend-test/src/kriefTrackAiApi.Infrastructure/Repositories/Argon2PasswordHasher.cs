using Isopoh.Cryptography.Argon2;
using System;
using System.Security.Cryptography;
using System.Text;

public class Argon2PasswordHasher
{
    private const int DefaultTimeCost = 4;
    private const int DefaultMemoryCost = 65536;
    private const int DefaultLanes = 4;
    private const int SaltSize = 16;

    public string HashPassword(string plainPassword)
    {
        byte[] salt = new byte[SaltSize];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        var config = new Argon2Config
        {
            Type = Argon2Type.DataIndependentAddressing,
            TimeCost = DefaultTimeCost,
            MemoryCost = DefaultMemoryCost,
            Lanes = DefaultLanes,
            Threads = DefaultLanes,
            Password = Encoding.UTF8.GetBytes(plainPassword),
            Salt = salt,
            HashLength = 32
        };

        using (var argon2 = new Argon2(config))
        {
            var hashedPassword = argon2.Hash();
            return config.EncodeString(hashedPassword.Buffer);
        }
    }

    public bool VerifyPassword(string plainPassword, string encodedHash)
    {
        return Argon2.Verify(encodedHash, plainPassword);
    }
}
