﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using pws;

namespace PwsTest
{
    [TestClass]
    public class PhpCgiProcessTest
    {
        [TestMethod]
        public void TestFindUsablePort()
        {
            int port = PhpCgiProcess.FindUsablePort();
            Assert.IsTrue(port > 9000);
        }
    }
}