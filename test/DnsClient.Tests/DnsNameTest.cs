﻿using System;
using System.Linq;
using Xunit;

namespace DnsClient.Tests
{
    public class DnsNameTest
    {
        [Fact]
        public void DnsName_ReceiveEscapedName()
        {
            var bytes = new byte[] { 4, 65, 195, 164, 98, 7, 98, 195, 156, 108, 195, 182, 98, 4, 99, 92, 46, 100, 3, 99, 111, 109, 0 };

            var offset = 0;
            var dnsName = DnsName.FromBytes(bytes, ref offset);

            //  should be "Aäb.bÜlöb.c\.d.com."
            var unEscapedString = dnsName.ToString(true);

            //  escaped should be A\195\164b.b\195\156l\195\182b.c\\.d.com.
            var escapedString = dnsName.ToString(false);
        }

        [Fact]
        public void DnsName_ReadEscapedName()
        {
            var value = "a\\195a.b\\166bc.c\\201\\.@{}dd\\e.e\\;f.f";
        }

        [Fact]
        public void DnsName_ReadUtfName()
        {
            var value = "aöa.bäb.c|öc.dljY:d.e;-'Üe.f?as.dasd";
        }

        public void DnsName_GetUtfName()
        {
        }

        [Fact]
        public void DnsName_SimpleValid()
        {
            var name = new DnsName("abc.xyz.example.com");
            var strValue = name.ToString();

            // ending
            Assert.Equal(name, "abc.xyz.example.com.");

            // domain name has 4 labels
            Assert.Equal(name.Size, 4);

            Assert.True(name.IsHostName);

            // Octet string version: 3abc3xyz7example3com0 => 21 chars
            Assert.Equal(name.Octets, 21);
        }

        [Fact]
        public void DnsName_LongName()
        {
            var longName = new DnsName("12341234123412341234123412341234123412341234.123123123123123123123123123123123123123123123123.123123123123123123");

            Assert.Equal(longName.Size, 3);
            Assert.True(longName.IsHostName);
        }

        [Fact]
        public void DnsName_EscapingNoHostName()
        {
            var name = (DnsName)"abc.zy\\.z.com";

            Assert.Equal(name[1], "zy.z");
            Assert.Equal(name.Size, 3);
            Assert.False(name.IsHostName);
        }

        [Fact]
        public void DnsName_ReadEscapedBytes()
        {
            var name = (DnsName)"l\\195\\156\\195\\164'la\\195\\188\\195\\182#\\.2x";

            var s = name.ToString();
            var utf8 = name.ToString(true);
            Assert.Equal(name[0], "lÜä'laüö#.2x");
            Assert.Equal(name.Size, 1);
            Assert.False(name.IsHostName);
        }

        [Fact]
        public void DnsName_Concat2()
        {
            var name = new DnsName("üöäxyz.ns1");
            name.Concat("hallo.com");

            Assert.Equal(name.ToString(true), "üöäxyz.ns1.hallo.com.");
            Assert.Equal(name.Size, 4);
            Assert.False(name.IsHostName);
        }

        [Fact]
        public void DnsName_Concat()
        {
            var name = new DnsName("xyz.ns1");
            name.Concat("abc.com");

            Assert.Equal(name.ToString(), "xyz.ns1.abc.com.");
            Assert.Equal(name.Size, 4);
            Assert.True(name.IsHostName);
        }

        [Fact]
        public void DnsName_ImplicitToString()
        {
            var name = new DnsName("xyz.ns1.hallo.com");
            Assert.Equal(name, "xyz.ns1.hallo.com.");
        }

        [Fact]
        public void DnsName_LabelTooLarge()
        {
            // max length of one label is 63
            var label = string.Join("", Enumerable.Repeat("a", 64));
            Action act = () => new DnsName(label + ".org");

            Assert.ThrowsAny<InvalidOperationException>(act);
        }

        [Fact]
        public void DnsName_NameTooLarge()
        {
            // max length of the name in total is 255 counting the octets including zero ending
            var label = string.Join("", Enumerable.Repeat("a", 63));
            Action act = () => new DnsName(label + "." + label + "." + label + "." + label);

            Assert.ThrowsAny<InvalidOperationException>(act);
        }

        [Fact]
        public void DnsName_ManyLabels()
        {
            var label = string.Join(".", Enumerable.Repeat("a", 127));
            var name = new DnsName(label);

            Assert.Equal(name.Octets, 255);
            Assert.True(name.IsHostName);
        }

        [Fact]
        public void DnsName_FromBytesValid()
        {
            int offset = 0;
            var name = DnsName.FromBytes(new byte[] { 5, 90, 90, 92, 46, 90, 2, 56, 56, 0 }, ref offset);

            Assert.Equal(name.Size, 2);
            Assert.Equal(name.Octets, 10);
            Assert.False(name.IsHostName);
        }

        [Fact]
        public void DnsName_FromBytesInvalidLength()
        {
            int offset = 0;
            Action act = () => DnsName.FromBytes(new byte[] { 3, 90, 90, 90, 6, 56, 56, 0 }, ref offset);

            Assert.ThrowsAny<ArgumentOutOfRangeException>(act);
        }

        [Fact]
        public void DnsName_FromBytesInvalidOffset()
        {
            int offset = 1;
            Action act = () => DnsName.FromBytes(new byte[] { 2 }, ref offset);

            Assert.ThrowsAny<ArgumentNullException>(act);
        }
    }
}