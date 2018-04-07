using NUnit.Framework;
using SampleCosmosCore2App.Models.User;
using System;
using System.Collections.Generic;
using System.Text;

namespace SampleCosmosCore2AppTests.Membership.Data
{
    [TestFixture]
    public class UserAuthenticationModelTests
    {

        [Test]
        public void GetMaskedIdentity_EmptyString_ReturnsEmptyPlaceholder()
        {
            var auth = new UserAuthenticationModel()
            {
                Identity = String.Empty
            };

            var result = auth.GetMaskedIdentity();

            Assert.AreEqual(UserAuthenticationModel.EMPTY_MASKED_VALUE, result);
        }

        [TestCase("a", "X")]
        [TestCase("aa", "aX")]
        [TestCase("aaa", "aXX")]
        [TestCase("aaaa", "aaXX")]
        [TestCase("aaaaa", "aaXXX")]
        [TestCase("aaaaaa", "aaXXXX")]
        [TestCase("aaaaaaa", "aaXXXXX")]
        [TestCase("aaaaaaaa", "aaXXXXXX")]
        [TestCase("aaaaaaaaa", "aaXXXXXXX")]
        [TestCase("aaaaaaaaaa", "aaXXXXXXXX")]
        [TestCase("aaaaaaaaaaa", "aaXXXXXXXXX")]
        public void GetMaskedIdentity_MultipleLengths_AlwaysMasksPartOfString(string identity, string expectedHash)
        {
            var auth = new UserAuthenticationModel()
            {
                Identity = identity
            };

            var result = auth.GetMaskedIdentity();

            Assert.IsTrue(result.EndsWith("X"));
            Assert.AreEqual(expectedHash, result);
        }

    }
}
