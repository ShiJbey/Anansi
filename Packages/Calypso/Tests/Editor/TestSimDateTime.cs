using System;
using NUnit.Framework;
using Calypso;

public class TestSimDateTime
{
    [Test]
    public void TestDateConstructor()
    {
        // Use the Assert class to test conditions
        var d0 = new SimDateTime(1, 1, 1);
        Assert.AreEqual(Days.Monday, d0.Day);
        Assert.AreEqual(1, d0.Date);
        Assert.AreEqual(Months.January, d0.Month);
        Assert.AreEqual(1, d0.Year);
        Assert.AreEqual(0, d0.Hour);
        Assert.AreEqual(0, d0.Minutes);

        var d1 = new SimDateTime(13, 7, 2001, 8, 0);
        Assert.AreEqual(Days.Saturday, d1.Day);
        Assert.AreEqual(13, d1.Date);
        Assert.AreEqual(Months.July, d1.Month);
        Assert.AreEqual(2001, d1.Year);
        Assert.AreEqual(8, d1.Hour);
        Assert.AreEqual(0, d1.Minutes);

        // Year cannot be less than 1
        Assert.Throws<ArgumentException>(
            () => new SimDateTime(10, 10, 0, 0, 0));

        // Month cannot be less than 1
        Assert.Throws<ArgumentException>(
            () => new SimDateTime(28, 0, 2023, 0, 0));

        // Month cannot be greater than 12
        Assert.Throws<ArgumentException>(
            () => new SimDateTime(28, 13, 2023, 0, 0));

        // Day cannot be less than 1
        Assert.Throws<ArgumentException>(
            () => new SimDateTime(0, 10, 2023, 0, 0));

        // Day connot be greater than 28
        Assert.Throws<ArgumentException>(
            () => new SimDateTime(29, 10, 2023, 0, 0));

        // Hour cannot be less than 0
        Assert.Throws<ArgumentException>(
            () => new SimDateTime(10, 10, 2023, -1, 0));

        // Hour cannot be greater than 23
        Assert.Throws<ArgumentException>(
            () => new SimDateTime(10, 10, 2023, 26, 0));

        // Minutes cannot be less than 0
        Assert.Throws<ArgumentException>(
            () => new SimDateTime(10, 10, 2023, 5, -1));

        // Minutes cannot be greater than 59
        Assert.Throws<ArgumentException>(
            () => new SimDateTime(10, 10, 2023, 5, 72));
    }

    [Test]
    public void TestCopyConstructor()
    {
        var d0 = new SimDateTime(13, 7, 2001, 8, 0);
        var d1 = new SimDateTime(d0);

        Assert.AreEqual(d0, d1);
        Assert.AreEqual(Days.Saturday, d1.Day);
        Assert.AreEqual(13, d1.Date);
        Assert.AreEqual(Months.July, d1.Month);
        Assert.AreEqual(2001, d1.Year);
        Assert.AreEqual(8, d1.Hour);
        Assert.AreEqual(0, d1.Minutes);
    }

    [Test]
    public void TestGetTimeOfDay()
    {
        Assert.AreEqual(
            TimeOfDay.Morning, 
            new SimDateTime(13, 7, 2001, 8, 0).TimeOfDay);

        Assert.AreEqual(
            TimeOfDay.Afternoon,
            new SimDateTime(13, 7, 2001, 14, 0).TimeOfDay);

        Assert.AreEqual(
            TimeOfDay.Evening,
            new SimDateTime(13, 7, 2001, 20, 0).TimeOfDay);

        Assert.AreEqual(
            TimeOfDay.Night,
            new SimDateTime(13, 7, 2001, 1, 0).TimeOfDay);
    }

    [Test]
    public void TestTotalMinutes()
    {
        var d0 = new SimDateTime(23, 3, 2001, 10, 0);
        Assert.AreEqual(967792920, d0.TotalMinutes);

        var d1 = new SimDateTime(23, 2, 2001, 8, 0);
        Assert.AreEqual(967752480, d1.TotalMinutes);
    }

    [Test]
    public void TestSubtractDates()
    {
        var d0 = new SimDateTime(23, 3, 2001, 10, 0);
        var d1 = new SimDateTime(23, 2, 2001, 8, 0);

        var delta = d0 - d1;

        Assert.AreEqual(0, delta.Years);
        Assert.AreEqual(1, delta.Months);
        Assert.AreEqual(0, delta.Days);
        Assert.AreEqual(2, delta.Hours);
    }

    [Test]
    public void TestAddDeltaToDate()
    {
        var d0 = new SimDateTime(23, 3, 2001, 10, 0);
        var delta = new DeltaTime(0, 5, 27, 0, 0);

        d0 += delta;

        Assert.AreEqual(2001, d0.Year);
        Assert.AreEqual(Months.September, d0.Month);
        Assert.AreEqual(22, d0.Date);
        Assert.AreEqual(10, d0.Hour);
        Assert.AreEqual(0, d0.Minutes);
    }

    [Test]
    public void TestDateLessThan()
    {
        Assert.IsFalse(new SimDateTime(1, 1, 1) < new SimDateTime(1, 1, 1));
        Assert.IsTrue(new SimDateTime(1, 1, 1) < new SimDateTime(1, 1, 2000, 0, 0));
        Assert.IsFalse(new SimDateTime(1, 1, 3000, 0, 0) < new SimDateTime(1, 1, 1));
    }

    [Test]
    public void TestDateLessThanEquals()
    {
        Assert.IsTrue(new SimDateTime(1, 1, 1) <= new SimDateTime(1, 1, 1));
        Assert.IsTrue(new SimDateTime(1, 1, 1) <= new SimDateTime(1, 1, 2000, 0, 0));
        Assert.IsFalse(new SimDateTime(1, 1, 3000, 0, 0) <= new SimDateTime(1, 1, 1));
    }

    [Test]
    public void TestDateEquals()
    {
        Assert.IsTrue(new SimDateTime(1, 1, 1) == new SimDateTime(1, 1, 1));
        Assert.IsFalse(new SimDateTime(1, 1, 1) == new SimDateTime(1, 1, 2000, 0, 0));
        Assert.IsFalse(new SimDateTime(1, 1, 3000, 0, 0) == new SimDateTime(1, 1, 1));
        Assert.IsTrue(new SimDateTime(1, 1, 3000, 0, 0) == new SimDateTime(1, 1, 3000, 0, 0));
        Assert.IsTrue(new SimDateTime(22, 8, 3000, 9, 54) == new SimDateTime(22, 8, 3000, 9, 54));
    }

    [Test]
    public void TestDateNotEquals()
    {
        Assert.IsFalse(new SimDateTime(1, 1, 1) != new SimDateTime(1, 1, 1));
        Assert.IsTrue(new SimDateTime(1, 1, 1) != new SimDateTime(1, 1, 2000, 0, 0));
        Assert.IsTrue(new SimDateTime(1, 1, 3000, 0, 0) != new SimDateTime(1, 1, 1));
        Assert.IsFalse(new SimDateTime(1, 1, 3000, 0, 0) != new SimDateTime(1, 1, 3000, 0, 0));
        Assert.IsFalse(new SimDateTime(22, 8, 3000, 9, 54) != new SimDateTime(22, 8, 3000, 9, 54));
    }

    [Test]
    public void TestDateGreaterThanEquals()
    {
        Assert.IsTrue(new SimDateTime(1, 1, 1) >= new SimDateTime(1, 1, 1));
        Assert.IsFalse(new SimDateTime(1, 1, 1) >= new SimDateTime(1, 1, 2000, 0, 0));
        Assert.IsTrue(new SimDateTime(1, 1, 3000, 0, 0) >= new SimDateTime(1, 1, 1));
        Assert.IsTrue(new SimDateTime(1, 1, 3000, 0, 0) >= new SimDateTime(1, 1, 3000, 0, 0));
        Assert.IsTrue(new SimDateTime(22, 8, 3000, 9, 54) >= new SimDateTime(22, 8, 3000, 9, 54));
    }

    [Test]
    public void TestDateGreaterThan()
    {
        Assert.IsFalse(new SimDateTime(1, 1, 1) > new SimDateTime(1, 1, 1));
        Assert.IsFalse(new SimDateTime(1, 1, 1) > new SimDateTime(1, 1, 2000, 0, 0));
        Assert.IsTrue(new SimDateTime(1, 1, 3000, 0, 0) > new SimDateTime(1, 1, 1));
        Assert.IsFalse(new SimDateTime(1, 1, 3000, 0, 0) > new SimDateTime(1, 1, 3000, 0, 0));
        Assert.IsFalse(new SimDateTime(22, 8, 3000, 9, 54) > new SimDateTime(22, 8, 3000, 9, 54));
    }

    [Test]
    public void TestToString()
    {
        var d1 = new SimDateTime(13, 7, 2001, 8, 0);
        Assert.AreEqual("Saturday, July 13, 2001 @ 08:00", d1.ToString());
    }

    [Test]
    public void TestAdvanceTime()
    {
        var date = new SimDateTime(1, 1, 1);
        date.AdvanceTime(new DeltaTime(0, 0, 0, 26, 0));
        Assert.AreEqual(new SimDateTime(2, 1, 1, 2, 0), date);
        date.AdvanceTime(new DeltaTime(0, 0, 4, 24, 0));
        Assert.AreEqual(new SimDateTime(7, 1, 1, 2, 0), date);
        date.AdvanceTime(new DeltaTime(0, 0, 28, 0, 0));
        Assert.AreEqual(new SimDateTime(7, 2, 1, 2, 0), date);
        date.AdvanceTime(new DeltaTime(0, 12, 0, 0, 0));
        Assert.AreEqual(new SimDateTime(7, 2, 2, 2, 0), date);

        date = new SimDateTime(27, 12, 2022);
        date.AdvanceTime(new DeltaTime(0, 0, 1, 0, 0));
        Assert.AreEqual(new SimDateTime(28, 12, 2022), date);
        date.AdvanceTime(new DeltaTime(0, 0, 1, 0, 0));
        Assert.AreEqual(new SimDateTime(1, 1, 2023), date);
        date.AdvanceTime(new DeltaTime(0, 0, 336, 0, 0));
        Assert.AreEqual(new SimDateTime(1, 1, 2024), date);
    }
}
