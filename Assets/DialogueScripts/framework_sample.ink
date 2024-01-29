EXTERNAL SetLocation(location, tags)
EXTERNAL SetSpeaker(speaker, tags)

VAR timesKnocked = 0

// This divert is here to take the story somehwere during testing. However, top-level diverts are not encouraged. The story will jump to the start storylet, but if any function calls are present in the initial divert, they might be executed before the game makes the official jump
// -> location_hallway

=== start ===

// The system will always look for a knot with the name "storylet_start" to execute first. Inside of this knot, designers should set the current time of day and location. This will move all characters to where they need to be. Optionally, you can include prologue content under this knot to help introduce the player to the game.

// {SetLocation("outside_library", "")}

-> location_outside_library

-> DONE

=== storylet_something_nuts ===
# ---
# 
# ===

// So storylets might be limited by the player's location or the relationships of the character in the story.


-> DONE


=== location_ship_bridge ===
# ---
# tags: location, ship_bridge
# choiceLabel: Go to Ship Bridge
# connectedLocations: hallway
# ===

You are on the bridge of the ship

// + {canTravel_hallway()} [Go to hallway] -> location_hallway

-> DONE

=== action_nap_on_bridge ===
# ---
# choiceLabel: Take a nap
# eligibleLocs: players_room
# ===

-> DONE

=== action_nap_in_room ===
# ---
# choiceLabel: Take a nap
# eligibleLocs: players_room
# ===

// The choice label here is what is presented to the player in the action selection panel when we are looking for actions to perform. Inside this action, we can place diverts to a bunch of different ways that this might go down. For example, maybe due to some flag being set, the player's nap is interrupted by a "random" event. 

// The main problem with this is that we need a way to do casting within the story. This would make a choice label/divert available given that some query passes. The alternative is to give writers an external function that request that the storylet controller queue up another storylet. This function also allows them to specify a fallback storylet name to jump to incase none is found.

-> DONE


=== function canTravel_ship_bridge() ===

~ return true


=== location_hallway ===
#---
# choiceLabel: Go to Hallway
#connectedLocations:ship_bridge,players_room,evelyns_room
#===

// Calypso uses the list of connected locations to determine what locations are available to travel to

You're in the hallway facing toward the tail of the ship. Evelyn's room is to the left. Your room is on the right.

// The problem here is that we want to treat the location like a storylet. So we cant have direct diverts within the location becuase we need to mix mix these choices with dynamic choices made available by storylets. We CANNOT mix knot-level choices with storylet-level choices. navigating around the world operates at the realm of storylets. Conversations and intermediate scripts operate at the knot-level.


// + {canTravel_ship_bridge()} [Go to bridge] -> location_ship_bridge
// + {canTravel_evelyns_room()} [Go to Evelyn's room] -> location_evelyns_room
// + {!canTravel_evelyns_room()}[Knock on Evenlyn's door] -> action_knock_evelyns_room
// + {canTravel_players_room()} [Go to your room] -> location_players_room

-> DONE


=== function canTravel_hallway() ===

~ return true


=== location_evelyns_room ===
#---
# choiceLabel: Go to Evelyn's room.
#connectedLocations:hallway
#===

// The text below is flavor text for the location that might influence what choice the player makes.

You are in Evelyn's room. She is looking looking out the window, and listening to music

Evelyn: Do you need something?

// * Ask about job -> location_evelyns_room
// * Ask about herself -> location_evelyns_room
// * Ask about crew member -> location_evelyns_room
// + {canTravel_hallway()} [Go to hallway] -> location_hallway
    
-> DONE

=== action_knock_evelyns_room() ===
# ---
# choiceLabel: Knock on Evelyn's door
# eligibleLocs: hallway
# ===

You knock on evelyn's door...
~ timesKnocked = timesKnocked + 1

{
    - timesKnocked < 3:
        <> But don't get an answer.
    - else:
        <> And you hear the door unlock.
}



-> location_hallway


=== function _knock_evelyns_room() ===
// Storylets can supply an optional precondition function that evaluates if the storylet is eligible to be shown as a potential action. This function serves as a precondition check, but it does not perform the casting operations required to make an instance of the its corresponding storylet.

// The precondition function is the same base name of the storylet/location/action with a leading underscore.


~ return true



=== function canTravel_evelyns_room() ===

~ return timesKnocked >= 3

=== location_players_room ===
# ---
# choiceLabel: Go to your room.
# ===

You're in your room.

// + {canTravel_hallway()} [Go to hallway] -> location_hallway

-> DONE

=== function canTravel_players_room() ===
// These functions are auto-detected by Calypso and used to determine if this location can be traveled to

~ return true


=== location_outside_library ===
#---
# choiceLabel: Go to Outside Library.
# connectedLocations: campus_walkway, library
#===

{SetLocation("outside_library", "")}

-> DONE

=== location_dining_hall ===
#---
# choiceLabel: Go to Dining Hall.
# connectedLocations: campus_walkway
#===

{SetLocation("dining_hall", "")}

-> DONE

=== location_library ===
#---
# choiceLabel: Go to Library.
# connectedLocations: outside_library
#===

{SetLocation("library", "")}

-> DONE

=== location_gym ===
#---
# choiceLabel: Go to Gym.
# connectedLocations: campus_walkway
#===

{SetLocation("gym", "")}

-> DONE

=== location_classroom ===
#---
# choiceLabel: Go to Classroom.
# connectedLocations: campus_walkway
#===

{SetLocation("classroom", "")}

-> DONE

=== location_campus_walkway ===
#---
# choiceLabel: Go to Campus Walkway.
# connectedLocations: outside_library, gym, dining_hall, classroom, evelyn_dormroom, astrid_dormroom
#===

{SetLocation("campus_walkway", "")}

-> DONE

=== location_evelyn_dormroom ===
#---
# choiceLabel: Go to Evelyn's room.
# connectedLocations: campus_walkway
#===

{SetLocation("evelyn_dormroom", "")}

-> DONE

=== location_astrid_dormroom ===
#---
# choiceLabel: Go to Astrid's room.
# connectedLocations: campus_walkway
#===

{SetLocation("astrid_dormroom", "")}

-> DONE

-> END
