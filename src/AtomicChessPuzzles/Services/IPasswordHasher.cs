using System;

namespace AtomicChessPuzzles.Services
{
    public interface IPasswordHasher
    {
        Tuple<string, string> HashPassword(string password);
        string HashPassword(string password, string salt);
    }
}