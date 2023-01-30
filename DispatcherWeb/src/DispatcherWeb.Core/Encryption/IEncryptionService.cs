namespace DispatcherWeb.Encryption
{
    public interface IEncryptionService
    {
        string Encrypt(string plainText);
        string Decrypt(string ciphertext);
        string EncryptIfNotEmpty(string plainText);
        string DecryptIfNotEmpty(string cipherText);
    }
}