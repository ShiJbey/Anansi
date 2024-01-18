INCLUDE ./globals.ink

// Global variables
VAR ChoseRedPill = false
VAR HealthPoints = 50
VAR other_character = ""

// -> storylet_start_knot

=== storylet_start_knot ===
# query >>
# ?speaker.relationships.?other.traits.friend
# neq ?other ?player
# ?player.relationships.?other.traits.rival
# end >>
# set >> other_character to ?other


Hello! This is the starting knot! #speaker >> {speaker}
Now, we'll go to knot 2! #speaker >> {speaker}

-> knot_2
-> DONE


=== knot_2 ===
Hello from knot 2! #speaker >> {speaker}
Time for a personality test. #speaker >> {other_character}
Red pill or blue pill? #speaker >> {speaker}
*** Red pill
~ChoseRedPill = true // update variable value
-> red_pill
*** Blue pill
~HealthPoints -= 20 // update variable value
-> blue_pill

-> DONE

=== red_pill ===
My god, how brave! #speaker >> {speaker}
-> continue_conversation
-> DONE

=== blue_pill
Bold move, my friend #speaker >> {speaker}
-> continue_conversation
-> DONE

=== continue_conversation
{HealthPoints < 50:
    You seem quite weak. I wonder why...
}
Alright. You have answered my question. #speaker >> {speaker}
{ ChoseRedPill:
#speaker >> {other_character}
You chose the red pill. But I'm still not sure I can trust you
}
{not red_pill: -> no_red_pill_comment}
-> DONE

=== no_red_pill_comment ===
#speaker >> {speaker}
You didn't choose the red pill. I'm not sure I can trust you.
-> DONE
