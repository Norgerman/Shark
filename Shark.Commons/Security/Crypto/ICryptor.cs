﻿using System;

namespace Shark.Security.Crypto
{
    public interface ICryptor : INamed, IDisposable
    {
        CryptoInfo Info { get; }

        /// <summary>
        /// init the crypter
        /// </summary>
        void Init(CryptoKey key);


        /// <summary>
        /// encrypt the buffer, and reset the crypto stream
        /// </summary>
        /// <param name="inputBuffer"></param>
        /// <returns></returns>
        byte[] Encrypt(ReadOnlySpan<byte> inputBuffer);

        /// <summary>
        /// decrypt the buffer, and reset the crypto stream
        /// </summary>
        /// <param name="inputBuffer"></param>
        /// <returns></returns>
        byte[] Decrypt(ReadOnlySpan<byte> inputBuffer);
    }
}
