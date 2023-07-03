using Calypso;

GameWorld world = new GameWorld();

// Create cast of characters
Actor vanessa = new Actor("Vanessa", "vanessa");
Actor james = new Actor("James", "james");
Actor xavier = new Actor("Xavier", "xavier");
Actor ximena = new Actor("Ximena", "ximena");

// Add actors to the world
world.Actors.Add(vanessa);
world.Actors.Add(james);
world.Actors.Add(xavier);
world.Actors.Add(ximena);

// Modify the base values for some relationships. New relationships can be added
// when needed. If two characters have never interacted, then we don't need to
// worry about their relationship until they do.
world.RelationshipManager.GetRelationship(ximena, james).SetBaseReputation(-10);
world.RelationshipManager.GetRelationship(james, ximena).SetBaseReputation(5);
world.RelationshipManager.GetRelationship(ximena, vanessa).SetBaseReputation(10);
world.RelationshipManager.GetRelationship(vanessa, ximena).SetBaseReputation(10);
world.RelationshipManager.GetRelationship(xavier, james).SetBaseReputation(20);

// Just testing out some ToString methods
Console.WriteLine(vanessa.ToString());
Console.WriteLine(ximena.ToString());
Console.WriteLine(world.RelationshipManager.GetRelationship(vanessa, ximena).ToString());




