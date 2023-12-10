using NUnit.Framework;
using Calypso.RePraxis;

public class TestRePraxisDatabase
{
    /// <summary>
    /// Test that create, retrieval, updating, and deletion (CRUD)
    /// </summary>
    [Test]
    public void TestDatabaseCRUD()
    {
        var db = new RePraxisDatabase();

        // Create values
        db["A.relationships.B.reputation"] = 10;
        db["A.relationships.B.type"] = "Rivalry";

        // Retrieve values
        Assert.AreEqual(10, db["A.relationships.B.reputation"]);
        Assert.AreEqual("Rivalry", db["A.relationships.B.type"]);
        Assert.AreEqual(true, db["A"]);

        // Update a value
        db["A.relationships.B.reputation"] = -99;
        Assert.AreEqual(-99, db["A.relationships.B.reputation"]);

        // Delete a value
        db.Remove("A.relationships.B.reputation");
        Assert.AreEqual(false, db["A.relationships.B.reputation"]);
    }

    [Test]
    public void TestQuery()
    {
        var db = new RePraxisDatabase();

        db["astrid.relationships.jordan.reputation"] = 30;
        db["astrid.relationships.jordan.type"] = "Rivalry";
        db["astrid.relationships.britt.reputation"] = -10;
        db["astrid.relationships.britt.type"] = "ExLover";
        db["astrid.relationships.lee.reputation"] = 20;
        db["astrid.relationships.lee.type"] = "Friend";
        db["player.relationships.jordan.reputation"] = -20;
        db["player.relationships.jordan.type"] = "enemy";

        // Relational expression with a single variable
        var r0 = new DBQuery()
            .Where("astrid.relationships.?other.reputation >= 10")
            .Run(db);

        Assert.AreEqual(true, r0.Success);
        Assert.AreEqual(2, r0.Bindings.Length);

        // Relational expression with multiple variables
        var r1 = new DBQuery()
            .Where("?A.relationships.?other.reputation <= 0")
            .Run(db);

        Assert.AreEqual(true, r1.Success);
        Assert.AreEqual(2, r1.Bindings.Length);

        // Assertion expression without variables
        var r2 = new DBQuery()
            .Where("astrid.relationships.britt")
            .Run(db);

        Assert.AreEqual(true, r2.Success);
        Assert.AreEqual(0, r2.Bindings.Length);

        // Failing assertion without variables
        var r3 = new DBQuery()
            .Where("astrid.relationships.haley")
            .Run(db);

        Assert.AreEqual(false, r3.Success);
        Assert.AreEqual(0, r3.Bindings.Length);

        // Compound query with multiple variables
        var r4 = new DBQuery()
            .Where("?speaker.relationships.?other.reputation >= 10")
            .Where("player.relationships.?other.reputation < 0")
            .Run(db);

        Assert.AreEqual(true, r4.Success);
        Assert.AreEqual(1, r4.Bindings.Length);
    }
}
