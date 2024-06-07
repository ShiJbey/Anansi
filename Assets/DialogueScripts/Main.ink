INCLUDE ./EvelynDialogue.ink
INCLUDE ./SampleDialogue.ink

// Anansi provides various external functions to assist with communicating with external systems such as the GUI, Story Database, and Time System.

EXTERNAL SetPlayerLocation(locationId)
EXTERNAL DbInsert(statement)
EXTERNAL DbDelete(statement)
EXTERNAL DbAssert(statement)
EXTERNAL AdvanceTime()
EXTERNAL QueueStorylet(storyletId)
EXTERNAL QueueStoryletWithTags(tags, fallback)
EXTERNAL GetInput(dataType, prompt, variableName)

// Global variables are shared across all storylets because they use the same story instance. Storylet queries may overwrite these values. So, be mindful of your variable usage

// VAR speaker = "not-specified"
VAR PlayerName = "player"
VAR WorldSeed = 0
// VAR location = "not-specified"
// VAR timesKnocked = 0

// Do not use any top-level diverts. The GameManager will automatically jump to the "start" knot.

=== start ===
#---
#===

// The system will always look for a knot with the name "start" to execute first. Inside of this knot, designers should set the current time of day and location. This will move all characters to where they need to be. Optionally, you can include prologue content under this knot to help introduce the player to the game.

-> library

-> DONE

=== nap_in_room ===
# ---
# choiceLabel: Take a nap
# @query
# player.location!player_dormroom
# @end
# hidden: true
# tags: action
# ===

You decided to take a nap...

{AdvanceTime()}

Time has advanced.

-> DONE


=== function _knock_evelyns_room() ===
// Storylets can supply an optional precondition function that evaluates if the storylet is eligible to be shown as a potential action. This function serves as a precondition check, but it does not perform the casting operations required to make an instance of the its corresponding storylet.

// The precondition function is the same base name of the storylet/location/action with a leading underscore.

~ return true

=== outside_library ===
#---
# choiceLabel: Go to Outside Library.
# hidden: true
# tags: location
#===

{SetPlayerLocation("outside_library")}

{outside_library == 1:
    You're standing outside of the campus library.

    You look at your Student ID. The name says... {GetInput("text", "What is your name?", "PlayerName")}

    It says {PlayerName}. They usually spell it wrong, but the registrar manager had the same name.

    What are the chances of that?

    Your student ID number is...{GetInput("int", "Enter student ID...", "WorldSeed")}
}

-> DONE

=== dining_hall ===
#---
# choiceLabel: Go to Dining Hall.
# hidden: true
# tags: location
#===

{SetPlayerLocation("dining_hall")}

-> DONE

=== library ===
#---
# choiceLabel: Go to Library.
# hidden: true
# tags: location
#===

{SetPlayerLocation("library")}

-> DONE

=== gym ===
#---
# choiceLabel: Go to Gym.
# hidden: true
# tags: location
#===

{SetPlayerLocation("gym")}

-> DONE

=== classroom ===
#---
# choiceLabel: Go to Classroom.
# hidden: true
# tags: location
#===

{SetPlayerLocation("classroom")}

-> DONE

=== campus_walkway ===
#---
# choiceLabel: Go to Campus Walkway.
# hidden: true
# tags: location
#===

{SetPlayerLocation("campus_walkway")}

-> DONE

=== evelyn_dormroom ===
#---
# choiceLabel: Go to Evelyn's room.
# hidden: true
# tags: location
#===

{SetPlayerLocation("evelyn_dormroom")}

{
    - DbAssert("evelyn.location!evelyn_dormroom"):
        You are in Evelyn's room. She is looking looking out the window, listening to music
    - else:
        Evelyn is not here.
}

-> DONE

=== astrid_dormroom ===
#---
# choiceLabel: Go to Astrid's room.
# hidden: true
# tags: location
#===

{SetPlayerLocation("astrid_dormroom")}

{
    - DbAssert("astrid.location!astrid_dormroom"):
        Astrid is studying at her desk.
    - else:
        Astrid is not here.
}

-> DONE

=== player_dormroom ===
#---
# choiceLabel: Go to your room.
# hidden: true
# tags: location
#===

{SetPlayerLocation("player_dormroom")}

You're in your room.

-> DONE

=== talk_to_astrid ===
# ---
# choiceLabel: Talk to Astrid
# @query
# astrid.location!?loc
# player.location!?loc
# @end
# hidden: true
# tags: action
# ===

~speaker = "astrid"
{QueueStoryletWithTags("convo, astrid", "conversation_fallback")}

-> DONE

=== talk_to_evelyn ===
# ---
# choiceLabel: Talk to Evelyn
# @query
# evelyn.location!?loc
# player.location!?loc
# @end
# hidden: true
# tags: action
# ===

~speaker = "evelyn"
{QueueStoryletWithTags("evelyn, convo", "conversation_fallback")}

-> DONE

=== talk_to_momo ===
# ---
# choiceLabel: Talk to Momo
# @query
# momo.location!?loc
# player.location!?loc
# @end
# hidden: true
# tags: action
# ===

~speaker = "momo"
{QueueStoryletWithTags("convo, astrid", "conversation_fallback")}

-> DONE

=== talk_to_giyu ===
# ---
# choiceLabel: Talk to Giyu
# @query
# giyu.location!?loc
# player.location!?loc
# @end
# hidden: true
# tags: action
# ===

~speaker = "giyu"
{QueueStoryletWithTags("convo", "conversation_fallback")}

-> DONE


=== conversation_fallback ===
# ---
# ===

They have nothing to say to you.

-> DONE


-> END
