using RyuSocks.Auth.Packets;
using System;
using Xunit;

namespace RyuSocks.Test.Auth
{
    public class UsernameAndPasswordRequestTests
    {
        [Theory]
        [InlineData(new byte[] { 0xFF, 0xFF, 0xAA, 0x00, 0xCC }, false)]
        [InlineData(new byte[] { 0x01, 0x01, 0x01, 0x01, 0x01 }, true)]
        [InlineData(new byte[] { 0x01 }, false)]
        [InlineData(new byte[] { }, false)]
        [InlineData(new byte[] { 0x74 }, false)]
        public void Constructor_ThrowsOnWrongVersion(byte[] incomingPacket, bool hasRightVersion)
        {
            if (!hasRightVersion)
            {
                Assert.Throws<InvalidOperationException>(() => new UsernameAndPasswordRequest(incomingPacket));
            }
            else
            {
                UsernameAndPasswordRequest _incomingPacket = new(incomingPacket);
            }
        }

    }
}
