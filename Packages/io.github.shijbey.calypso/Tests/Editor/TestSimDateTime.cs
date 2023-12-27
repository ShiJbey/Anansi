using System;
using NUnit.Framework;
using Calypso;
using UnityEngine;

public class TestSimDateTime
{
    [Test]
    public void TestDateConstructor()
    {
        // Use the Assert class to test conditions
        var d0 = new SimDateTime();
        Assert.AreEqual(1, d0.Day);
        Assert.AreEqual(TimeOfDay.Morning, d0.TimeOfDay);

        var d1 = new SimDateTime(27, TimeOfDay.Afternoon);
        Assert.AreEqual(TimeOfDay.Afternoon, d1.TimeOfDay);
        Assert.AreEqual(27, d1.Day);

        // Day cannot be less than 1
        Assert.Throws<ArgumentException>(
            () => new SimDateTime(0, TimeOfDay.Morning));
    }

    [Test]
    public void TestCopyConstructor()
    {
        var d0 = new SimDateTime(31, TimeOfDay.Evening);
        var d1 = new SimDateTime(d0);

        Assert.AreEqual(d0, d1);
        Assert.AreEqual(31, d1.Day);
        Assert.AreEqual(TimeOfDay.Evening, d1.TimeOfDay);
    }

    [Test]
    public void TestDateLessThan()
    {
        Assert.IsFalse(new SimDateTime() < new SimDateTime());
        Assert.IsTrue(
            new SimDateTime(1, TimeOfDay.Morning) < new SimDateTime(12, TimeOfDay.Morning));
        Assert.IsTrue(
            new SimDateTime(1, TimeOfDay.Morning) < new SimDateTime(1, TimeOfDay.Afternoon));
        Assert.IsFalse(
            new SimDateTime(1, TimeOfDay.Evening) < new SimDateTime(1, TimeOfDay.Morning));
    }

    [Test]
    public void TestDateLessThanEquals()
    {
        Assert.IsTrue(new SimDateTime() <= new SimDateTime());
        Assert.IsTrue(
            new SimDateTime(1, TimeOfDay.Morning) <= new SimDateTime(12, TimeOfDay.Morning));
        Assert.IsTrue(
            new SimDateTime(1, TimeOfDay.Morning) <= new SimDateTime(1, TimeOfDay.Afternoon));
        Assert.IsFalse(
            new SimDateTime(1, TimeOfDay.Evening) <= new SimDateTime(1, TimeOfDay.Morning));
    }

    [Test]
    public void TestDateEquals()
    {
        Assert.IsTrue(new SimDateTime() == new SimDateTime());
        Assert.IsFalse(
            new SimDateTime(1, TimeOfDay.Morning) == new SimDateTime(12, TimeOfDay.Morning));
        Assert.IsFalse(
            new SimDateTime(1, TimeOfDay.Morning) == new SimDateTime(1, TimeOfDay.Afternoon));
        Assert.IsFalse(
            new SimDateTime(1, TimeOfDay.Evening) == new SimDateTime(1, TimeOfDay.Morning));
        Assert.IsTrue(
            new SimDateTime(37, TimeOfDay.Evening) == new SimDateTime(37, TimeOfDay.Evening));
    }

    [Test]
    public void TestDateNotEquals()
    {
        Assert.IsFalse(new SimDateTime() != new SimDateTime());
        Assert.IsTrue(
            new SimDateTime(1, TimeOfDay.Morning) != new SimDateTime(12, TimeOfDay.Morning));
        Assert.IsTrue(
            new SimDateTime(1, TimeOfDay.Morning) != new SimDateTime(1, TimeOfDay.Afternoon));
        Assert.IsTrue(
            new SimDateTime(1, TimeOfDay.Evening) != new SimDateTime(1, TimeOfDay.Morning));
        Assert.IsFalse(
            new SimDateTime(37, TimeOfDay.Evening) != new SimDateTime(37, TimeOfDay.Evening));
    }

    [Test]
    public void TestDateGreaterThanEquals()
    {
        Assert.IsTrue(new SimDateTime() >= new SimDateTime());
        Assert.IsFalse(
            new SimDateTime(1, TimeOfDay.Morning) >= new SimDateTime(12, TimeOfDay.Morning));
        Assert.IsFalse(
            new SimDateTime(1, TimeOfDay.Morning) >= new SimDateTime(1, TimeOfDay.Afternoon));
        Assert.IsTrue(
            new SimDateTime(2, TimeOfDay.Evening) >= new SimDateTime(1, TimeOfDay.Morning));
        Assert.IsTrue(
            new SimDateTime(37, TimeOfDay.Evening) >= new SimDateTime(37, TimeOfDay.Evening));
    }

    [Test]
    public void TestDateGreaterThan()
    {
        Assert.IsFalse(new SimDateTime() > new SimDateTime());
        Assert.IsFalse(
            new SimDateTime(1, TimeOfDay.Morning) > new SimDateTime(12, TimeOfDay.Morning));
        Assert.IsFalse(
            new SimDateTime(1, TimeOfDay.Morning) > new SimDateTime(1, TimeOfDay.Afternoon));
        Assert.IsTrue(
            new SimDateTime(2, TimeOfDay.Evening) > new SimDateTime(1, TimeOfDay.Morning));
        Assert.IsFalse(
            new SimDateTime(37, TimeOfDay.Evening) > new SimDateTime(37, TimeOfDay.Evening));
    }

    [Test]
    public void TestAdvanceTime()
    {
        var date = new SimDateTime(1, TimeOfDay.Night);
        date.AdvanceTime();
        Assert.AreEqual(2, date.Day);
        date.AdvanceTime();
        Assert.AreEqual(2, date.Day);
        Assert.AreEqual(TimeOfDay.Afternoon, date.TimeOfDay);
        date.AdvanceTime();
        Assert.AreEqual(TimeOfDay.Evening, date.TimeOfDay);
        date.AdvanceTime();
        date.AdvanceTime();
        Assert.AreEqual(3, date.Day);
        Assert.AreEqual(TimeOfDay.Morning, date.TimeOfDay);
    }
}
